using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Assets.Scripts.Grid;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.IO;
using System;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine.Networking;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using System.Linq;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts;

public class PathfindingManager : MonoBehaviour
{
    //Grid configuration
    public int width;
    public int height;
    public float cellSize;
    public string gridName;
    static string gridPath;
    string[,] textLines;

    //Essential Pathfind classes 
    public AStarPathfinding pathfinding { get; set; }

    //The Visual Grid
    private GameObject[,] visualGrid;
    public GameObject gridNodePrefab;

    //Private fields for internal use only
    Vector3 startingPosition = new Vector3();
    Vector3 goalPosition = new Vector3();
    int startingX, startingY;
    int goalX, goalY;

    //Public Debug options
    public bool showCoordinates;

    public GameObject orcPrefab;
    public GameObject chestPrefab;
    public GameObject manaPotion;
    public GameObject healthPotion;
    public GameObject skeletonPrefab;
    public GameObject dragonPrefab;


    //Path
    public GameObject character;
    List<NodeRecord> solution;
    public GameObject characterPrefab;
    public bool finished;
    public string TargetType;
    public List<NodeRecord> characterSolution;
    public List<NodeRecord> enemySolution;



    public void Initialize()
    {
        gridPath = "Assets/Resources/" + gridName + ".txt";

        //Handling grid and its configurations
        textLines = new string[height, width];
        HandleTextFile();
        this.pathfinding = new AStarPathfinding(width, height, cellSize, new NodePriorityHeap(), new ClosedDictionary(), new EuclideanDistance());
        visualGrid = new GameObject[width, height];
        GridMapVisual();
        pathfinding.grid.OnGridValueChanged += Grid_OnGridValueChange;

        this.character = Instantiate(characterPrefab);
        this.character.transform.position = new Vector3(250.0f, 0.0f, 280f);
        this.character.transform.localScale *= cellSize;
        this.character.GetComponent<AutonomousCharacter>().AStarPathFinding = this.pathfinding;
    }

    // Update is called once per frame
    void Update()
    {

     
        if (this.pathfinding.InProgress)
        {
            this.finished = this.pathfinding.Search(out this.solution, false);

            if (this.finished)
            {
                if (TargetType == "Character")
                {
                    characterSolution = this.solution;
                    this.solution = new List<NodeRecord>();
                }
                else if (TargetType == "Enemy")
                {
                    enemySolution = this.solution;
                    this.solution = new List<NodeRecord>();
                }
               
            }

            this.pathfinding.TotalProcessingTime += Time.deltaTime;

        }
    }


    public void InitializeSearch(int _startingX, int _startingY, int _goalX, int _goalY)
    {
      
        this.SetObjectColor(startingX, startingY, Color.cyan);
        this.SetObjectColor(goalX, goalY, Color.green);
        this.pathfinding.InitializePathfindingSearch(_startingX, _startingY, _goalX, _goalY);

    }

    public void InitializeSearch(Vector3 _startingPosition, Vector3 goalPosition, string type)
    {
        
        ClearGrid();
        TargetType = type;
        this.startingPosition = _startingPosition;
        int startingX, startingY;
        this.pathfinding.grid.GetXY(_startingPosition, out startingX, out startingY);

        int goalX, goalY;
        this.pathfinding.grid.GetXY(goalPosition, out goalX, out goalY);

        
        this.InitializeSearch(startingX, startingY, goalX, goalY);

    }

    public void DrawPath(List<NodeRecord> path)
    {
        int index = 0;
        foreach (var p in path)
        {
            index += 1;
            if (index == 1)
            {
                this.SetObjectColor(p.x, p.y, Color.cyan);
                continue;
            }

            if (index == path.Count)
            {
                this.SetObjectColor(p.x, p.y, Color.green + new Color(0.5f, 0.0f, 0.5f));
                break;
            }

            this.SetObjectColor(p.x, p.y, Color.green);
        }
    }

    // Create the grid according to the text file set in the "Assets/Resources/grid.txt"
    private void GridMapVisual()
    {
        int manaPotionCounter = 1;
        int healthPotionCounter = 1;
        int orCounter = 1;
        int skullCounter = 1;
        int chestCounter = 1;


        //Informing the grid of nodes that are not walkable
        for (int i = 0; i < textLines.GetLength(0); i++)
            for (int j = 0; j < textLines.GetLength(1); j++)

                if (textLines[i, j] == "1")
                {
                    var node = pathfinding.grid.GetGridObject(j, height - i - 1);
                    node.isWalkable = false;
                    pathfinding.grid.SetGridObject(node.x, node.y, node);

                }
                else if (textLines[i, j] == "x")
                {
                  // Skelleton
                    var skell = GameObject.Instantiate(skeletonPrefab);
                    skell.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    skell.transform.Translate(cellSize * 0.5f, 1.0f, cellSize * 0.5f);
                    skell.transform.localScale *= cellSize * 0.8f;
                    skell.name = "Skelleton" + skullCounter;
                    skullCounter++;

                }

                else if (textLines[i, j] == "D")
                {
                    //Dragon
                    var drag = GameObject.Instantiate(dragonPrefab);
                    drag.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    drag.transform.Translate(cellSize * 0.5f, -16.0f, cellSize * 0.5f);
                    drag.transform.localScale *= cellSize * 10.0f;

                }

                else if (textLines[i, j] == "o")
                {
                   //Orcs
                    var orc = GameObject.Instantiate(orcPrefab);
                    orc.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    orc.transform.Translate(cellSize / 2, -cellSize * 0.5f, -5.0f);
                    orc.transform.localScale *= cellSize * 0.8f;
                    orc.name = "Orc" + orCounter;
                    orCounter++;

                }

                else if (textLines[i, j] == "h")
                {
                   // H for Health
                    var health = GameObject.Instantiate(healthPotion);
                    health.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    health.transform.Translate(cellSize * 0.5f, 1.0f, cellSize * 0.5f);
                    health.transform.localScale *= cellSize * 1.0f;
                    health.name = "HealthPotion" + healthPotionCounter;
                    healthPotionCounter++;

                }

                else if (textLines[i, j] == "m")
                {
                   // M for Mana
                    var mana = GameObject.Instantiate(manaPotion);
                    mana.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    mana.transform.Translate(cellSize * 0.5f, 1.0f, cellSize * 0.5f);
                    mana.transform.localScale *= cellSize * 1.0f;
                    mana.name = "ManaPotion" + manaPotionCounter;
                    manaPotionCounter++;
                }

                else if (textLines[i, j] == "c")
                {
                    // C for Chest
                    var chest = GameObject.Instantiate(chestPrefab);
                    chest.transform.position = pathfinding.grid.GetWorldPosition(j, height - i - 1);
                    chest.transform.Translate(-cellSize/2, -cellSize/2,0.0f);
                    chest.transform.localScale *= cellSize * 1.5f;
                    chest.name = "Chest" + chestCounter;
                    chestCounter++;

                }


        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++){
                  
                   visualGrid[x, y] = CreateGridObject(this.gridNodePrefab, pathfinding.grid.GetGridObject(x, y)?.ToString(), cellSize, pathfinding.grid.GetWorldPosition(x, y) + new Vector3(cellSize, 2, cellSize) * 0.5f, 40, Color.black, Color.white);


            }
        UpdateGrid();
    }

    // Instantiating a Grid Object from the prefab, I know, its a lot of small line in a row but its working :)
    private GameObject CreateGridObject(GameObject prefab, string value, float cellsize, Vector3 position, int fontSize, Color fontColor, Color imageColor)
    {

        var obj = GameObject.Instantiate(prefab);
        Transform transform = obj.transform;
        transform.localScale = new Vector3(cellsize - 1, cellSize - 1, cellSize - 1);
        transform.localPosition = position;

        if (showCoordinates)
        {
            TextMesh text = obj.GetComponentInChildren<TextMesh>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = fontColor;
        }
        SpriteRenderer s = obj.GetComponent<SpriteRenderer>();
        s.color = imageColor;
        return obj;

    }

    // Reset the Grid to black and white
    public void ClearGrid()
    {
        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++)
            {
                var node = pathfinding.grid.GetGridObject(x, y);
                if (node.isWalkable)
                    this.SetObjectColor(x, y, Color.white);
                else this.SetObjectColor(x, y, Color.black);
            }


    }

    //Setting the color of the Node 
    public void SetObjectColor(int x, int y, Color color)
    {
        visualGrid[x, y].GetComponent<SpriteRenderer>().color = color;
    }

    
    public void UpdateGrid()
    {
        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++)
            {
                NodeRecord node = pathfinding.grid.GetGridObject(x, y);
                if (!node.isWalkable)
                    this.SetObjectColor(x, y, Color.black);
                
                //Debugging Options
      /*          if (node.status == NodeStatus.Open)
                    this.SetObjectColor(node.x, node.y, Color.blue);
                if (node.status == NodeStatus.Closed)
                    this.SetObjectColor(node.x, node.y, Color.red);
    */ 
            }
    }

    private void Grid_OnGridValueChange(object sender, Assets.Scripts.Grid.Grid<NodeRecord>.OnGridValueChangedEventArgs e)
    {
        NodeRecord node = pathfinding.grid.GetGridObject(e.x, e.y);
        if (node != null)
        {
            if (!node.isWalkable)
                this.SetObjectColor(e.x, e.y, Color.black);
         }
    }

    public GlobalPath CalculateSolution(List<NodeRecord> solution)
    {
        var currentNode = solution.Last();

        var path = new GlobalPath
        {
            IsPartial = false,
            Length = currentNode.gCost
        };
        

        var goalPosition = pathfinding.grid.GetWorldPosition(currentNode.x, currentNode.y);
        goalPosition.x += cellSize * 0.5f;
        goalPosition.z += cellSize * 0.5f;
        path.PathPositions.Add(goalPosition);

        //I need to remove the first Node and the last Node because they correspond to the dummy first and last Polygons that were created by the initialization.
        //And for instance I don't want to be forced to go to the center of the initial polygon before starting to move towards my destination.

        //skip the last node, but only if the solution is not partial (if the solution is partial, the last node does not correspond to the dummy goal polygon)
        if (currentNode.parent != null)
        {
            currentNode = currentNode.parent;
        }

        while (currentNode.parent != null)
        {
            path.PathNodes.Add(currentNode); //we need to reverse the list because this operator add elements to the end of the list
            var worldPos = pathfinding.grid.GetWorldPosition(currentNode.x, currentNode.y);
            worldPos.x += cellSize * 0.5f;
            worldPos.z += cellSize * 0.5f;
            path.PathPositions.Add(worldPos);
            if (currentNode.parent.parent == null) break; //this skips the first node
            currentNode = currentNode.parent;
        }

        path.PathNodes.Reverse();
        path.PathPositions.Reverse();
        return path;

    }

      public GlobalPath smoothPath(Vector3 position, GlobalPath actual)
		{
			var smoothedPath = new GlobalPath ();
      
        smoothedPath.PathPositions.Add (position);
        int x, y;
        pathfinding.grid.GetXY(position, out x, out y);

		smoothedPath.PathPositions.AddRange (actual.PathPositions);
        smoothedPath.PathNodes.Add(pathfinding.grid.GetGridObject(x, y));
        smoothedPath.PathNodes.AddRange(actual.PathNodes);

			int i = 0;
			while (i < smoothedPath.PathPositions.Count - 2) {
				if (walkable(smoothedPath.PathPositions[i], smoothedPath.PathPositions[i + 2])) {
				smoothedPath.PathPositions.RemoveAt(i + 1);
                smoothedPath.PathNodes.RemoveAt(i + 1);
				}
				else {
					i++;
				}
			}
        if (TargetType == "Character")
            foreach (var n in smoothedPath.PathNodes)
                this.SetObjectColor(n.x, n.y, Color.green);
         else if (TargetType == "Enemy")
            foreach (var n in smoothedPath.PathNodes)
                this.SetObjectColor(n.x, n.y, Color.red);

        return smoothedPath;
		}

	protected bool walkable(Vector3 p1, Vector3 p2)
	{
		Vector3 direction = p2 - p1;
        int posx, posy;
        var distance = direction.magnitude;
        if (distance < cellSize)
            return true;
        var segments = distance / cellSize;
      
        for (float i = 1; i < segments; i++)
        {
            float offSet = i / segments;
            var newPosition = p1 + direction * offSet;
           
            pathfinding.grid.GetXY(newPosition, out posx, out posy);
           
            
            var gridObj = pathfinding.grid.GetGridObject(posx, posy);
            if (gridObj == null || !gridObj.isWalkable)
            {
                return false;
            }
        
        }

      
        return true;
		}

    // Reading the text file that where the grid "definition" is stored
    public void HandleTextFile()
    {

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(gridPath);
        var fileContent = reader.ReadToEnd();
        reader.Close();
        var lines = fileContent.Split("\n"[0]);
        
        int i = 0;
         foreach(var l in lines){
            var words = l.Split();
            var j = 0;

            var w = words[0];

            foreach (var letter in w)
            {
                textLines[i, j] = letter.ToString();
                j++;

                if (j == textLines.GetLength(1))
                    break;
            }            

            i++;
            if (i == textLines.GetLength(0))
                break;
        }


    }

}
