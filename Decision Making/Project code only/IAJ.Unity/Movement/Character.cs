using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Movement
{
    public class Character
    {
        List<Vector3> path;
        float speed = 1.0f;
        float cellSize;
        int currentNode = 0;
        Grid<NodeRecord> _grid;
        GameObject obj;

        Transform character;

        public Character(Grid<NodeRecord> grid, GameObject prefab, float cellSize, float speed)
        {
            this._grid = grid;
            this.cellSize = cellSize;
            this.speed = speed;
            obj = GameObject.Instantiate(prefab);
           
           
            character = obj.GetComponent<Transform>();
            character.position = new Vector3(-1.0f, 0.0f, -1.0f);
            character.localScale *= cellSize * 0.8f;
            currentNode = 0;
            path = new List<Vector3>();
        }

        public void SetNewPath(List<NodeRecord> _path)
        {
            path = new List<Vector3>();
            foreach (var p in _path)
            {
                var pathPos = this._grid.GetWorldPosition(p.x, p.y);
                path.Add(new Vector3(pathPos.x + cellSize / 2, 0.0f, pathPos.z + cellSize / 2));
            }

            obj.transform.position = path[0];
            currentNode = 1;

        }

        public void Update()
        {
           
            if (currentNode < path.Count)
            {
                
                var direction = this.path[currentNode] - this.character.position;
                direction.y = 0.0f;
                var distance = direction.magnitude;

                //character.forward = direction;
                character.Translate(direction * 0.01f * speed);

                if (distance < 5.0f)
                {
                    currentNode += 1;
                   
                }
               
            }

        }
    }
}
