using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.EventSystems;

public class JPSPlusPathfinding : Pathfinding
{
    private const int NORTH = 0;
    private const int EAST = 1;
    private const int SOUTH = 2;
    private const int WEST = 3;
    private const int NE = 4;
    private const int SE = 5;
    private const int SW = 6;
    private const int NW = 7;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public override uint NodesPerSearch { get; set; }
    public override uint TotalProcessedNodes { get; protected set; }
    public override int MaxOpenNodes { get; protected set; }
    public override float TotalProcessingTime { get; set; }
    public override uint MaxExploredNodesPerFrame { get; set; }

    public override bool InProgress { get; set; }

    public override Grid<NodeRecord> grid { get; set; }

    private int[,,] distance;
    private bool[,,] jumpPoints;

    private int width;
    private int height;

    private Dictionary<int, int[]> validDirLookupTable;

    public override IOpenSet Open { get; protected set; }
    public override IClosedSet Closed { get; protected set; }

    public override IHeuristic Heuristic { get; protected set; }

    public override int StartPositionX { get; set; }
    public override int StartPositionY { get; set; }
    public override int GoalPositionX { get; set; }
    public override int GoalPositionY { get; set; }
    public override NodeRecord GoalNode { get; set; }
    public override NodeRecord StartNode { get; set; }


    public JPSPlusPathfinding(int width, int height, float cellSize, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
    {
        grid = new Grid<NodeRecord>(width, height, cellSize, (Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y));
        this.Open = open;
        this.Closed = closed;
        this.Heuristic = heuristic;

        this.width = width;
        this.height = height;
        distance = new int[width, height, 8];
        jumpPoints = new bool[width, height, 4];

        validDirLookupTable = new Dictionary<int, int[]>();
        validDirLookupTable.Add(-1, new int[] { SOUTH, SE, EAST, NE, NORTH, NW, WEST, SW }); //SPECIAL CASE FOR THE FIRST NODE
        validDirLookupTable.Add(SOUTH, new int[] { SOUTH, WEST, SW, SE, EAST }); //SOUTH
        validDirLookupTable.Add(SE, new int[] { SOUTH, SE, EAST }); //SOUTH EAST
        validDirLookupTable.Add(EAST, new int[] { SOUTH, SE, EAST, NE, NORTH }); //EAST
        validDirLookupTable.Add(NE, new int[] { EAST, NE, NORTH }); //NORTH EAST
        validDirLookupTable.Add(NORTH, new int[] { NORTH, NE, EAST, NW, WEST }); //NORTH
        validDirLookupTable.Add(NW, new int[] { NORTH, NW, WEST }); //NORTH WEST
        validDirLookupTable.Add(WEST, new int[] { NORTH, NW, WEST, SW, SOUTH }); //WEST
        validDirLookupTable.Add(SW, new int[] { WEST, SW, SOUTH }); //SOUTH WEST

    }


    //------------------------------ PRE_PROCESSING -------------------------------------//
    public void PreProcessGrid()
    {
        FindPrimaryJumpPoints();

        FindStraightJumpPointsAndWallsWE();
        FindStraightJumpPointsAndWallsEW();
        FindStraightJumpPointsAndWallsNS();
        FindStraightJumpPointsAndWallsSN();

        FindDiagonalJumpPointsAndWallsSN();
        FindDiagonalJumpPointsAndWallsNS();
    }

    private void FindPrimaryJumpPoints()
    {
        for (int y = 0; y < height; ++y)
        {

            for (int x = 0; x < width; ++x)
            {
                NodeRecord curr = grid.GetGridObject(x, y);

                NodeRecord north = grid.GetGridObject(x, y + 1);
                NodeRecord south = grid.GetGridObject(x, y - 1);
                NodeRecord west = grid.GetGridObject(x - 1, y);
                NodeRecord east = grid.GetGridObject(x + 1, y);

                // Is it an obstacle?
                if (!curr.isWalkable)
                {
                    NodeRecord northWestDiag = grid.GetGridObject(x - 1, y + 1);

                    if (northWestDiag != null && northWestDiag.isWalkable && north.isWalkable && west.isWalkable)
                    {
                        jumpPoints[northWestDiag.x, northWestDiag.y, NORTH] = true;
                        jumpPoints[northWestDiag.x, northWestDiag.y, WEST] = true;
                        //northWestDiag.status = NodeStatus.Closed;
                        //grid.SetGridObject(northWestDiag.x, northWestDiag.y, northWestDiag);
                    }

                    NodeRecord northEastDiag = grid.GetGridObject(x + 1, y + 1);

                    if (northEastDiag != null && northEastDiag.isWalkable && north.isWalkable && east.isWalkable)
                    {
                        jumpPoints[northEastDiag.x, northEastDiag.y, NORTH] = true;
                        jumpPoints[northEastDiag.x, northEastDiag.y, EAST] = true;
                        //northEastDiag.status = NodeStatus.Closed;
                        //grid.SetGridObject(northEastDiag.x, northEastDiag.y, northEastDiag);
                    }


                    NodeRecord southWestDiag = grid.GetGridObject(x - 1, y - 1);

                    if (southWestDiag != null && southWestDiag.isWalkable && south.isWalkable && west.isWalkable)
                    {
                        jumpPoints[southWestDiag.x, southWestDiag.y, SOUTH] = true;
                        jumpPoints[southWestDiag.x, southWestDiag.y, WEST] = true;
                        //southWestDiag.status = NodeStatus.Closed;
                        //grid.SetGridObject(southWestDiag.x, southWestDiag.y, southWestDiag);
                    }

                    NodeRecord southEastDiag = grid.GetGridObject(x + 1, y - 1);

                    if (southEastDiag != null && southEastDiag.isWalkable && south.isWalkable && east.isWalkable)
                    {
                        jumpPoints[southEastDiag.x, southEastDiag.y, SOUTH] = true;
                        jumpPoints[southEastDiag.x, southEastDiag.y, EAST] = true;
                        //southEastDiag.status = NodeStatus.Closed;
                        //grid.SetGridObject(southEastDiag.x, southEastDiag.y, southEastDiag);
                    }
                }
            }
        }
    }

    private void FindStraightJumpPointsAndWallsWE()
    {

        for (int y = 0; y < height; ++y)
        {

            int count = -1;
            bool jumpPointLastSeen = false;
            for (int x = 0; x < width; ++x)
            {
                if (IsWall(x, y))
                {
                    count = -1;
                    jumpPointLastSeen = false;
                    distance[x, y, WEST] = 0;
                    continue;
                }
                count++;
                if (jumpPointLastSeen)
                {
                    distance[x, y, WEST] = count;
                }
                else //Wall last seen
                {
                    distance[x, y, WEST] = -count;
                }
                if (jumpPoints[x, y, WEST])
                {
                    count = 0;
                    jumpPointLastSeen = true;
                }
            }
        }
    }
    private void FindStraightJumpPointsAndWallsEW()
    {
        for (int y = 0; y < height; ++y)
        {
            int count = -1;
            bool jumpPointLastSeen = false;
            for (int x = width - 1; x >= 0; --x)
            {
                if (IsWall(x, y))
                {
                    count = -1;
                    jumpPointLastSeen = false;
                    distance[x, y, EAST] = 0;
                    continue;
                }
                count++;
                if (jumpPointLastSeen)
                {
                    distance[x, y, EAST] = count;
                }
                else //Wall last seen
                {
                    distance[x, y, EAST] = -count;
                }
                if (jumpPoints[x, y, EAST])
                {
                    count = 0;
                    jumpPointLastSeen = true;
                }
            }
        }
    }
    private void FindStraightJumpPointsAndWallsNS()
    {
        for (int x = 0; x < width; ++x)
        {
            int count = -1;
            bool jumpPointLastSeen = false;

            for (int y = height - 1; y >= 0; --y)
            {
                if (IsWall(x, y))
                {
                    count = -1;
                    jumpPointLastSeen = false;
                    distance[x, y, NORTH] = 0;
                    continue;
                }
                count++;
                if (jumpPointLastSeen)
                {
                    distance[x, y, NORTH] = count;
                }
                else //Wall last seen
                {
                    distance[x, y, NORTH] = -count;
                }
                if (jumpPoints[x, y, NORTH])
                {
                    count = 0;
                    jumpPointLastSeen = true;
                }
            }
        }
    }
    private void FindStraightJumpPointsAndWallsSN()
    {
        for (int x = 0; x < width; ++x)
        {
            int count = -1;
            bool jumpPointLastSeen = false;

            for (int y = 0; y < height; ++y)
            {
                if (IsWall(x, y))
                {
                    count = -1;
                    jumpPointLastSeen = false;
                    distance[x, y, SOUTH] = 0;
                    continue;
                }
                count++;
                if (jumpPointLastSeen)
                {
                    distance[x, y, SOUTH] = count;
                }
                else //Wall last seen
                {
                    distance[x, y, SOUTH] = -count;
                }
                if (jumpPoints[x, y, SOUTH])
                {
                    count = 0;
                    jumpPointLastSeen = true;
                }
            }
        }
    }

    private void FindDiagonalJumpPointsAndWallsSN()
    {
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (!IsWall(x, y))
                {
                    // SouthWest jumpPoints and Walls
                    if (x == 0 || y == 0 || IsWall(x - 1, y) ||
                    IsWall(x, y - 1) || IsWall(x - 1, y - 1))
                    {
                        //Wall one away
                        distance[x, y, SW] = 0;
                    }
                    else if (!IsWall(x - 1, y) && !IsWall(x, y - 1) &&
                    (distance[x - 1, y - 1, SOUTH] > 0 ||
                    distance[x - 1, y - 1, WEST] > 0))
                    {
                        //Straight jump point one away
                        distance[x, y, SW] = 1;
                    }
                    else
                    {
                        //Increment from last
                        int jumpDistance =
                        distance[x - 1, y - 1, SW];
                        if (jumpDistance > 0)
                        {
                            distance[x, y, SW] =
                            1 + jumpDistance;
                        }
                        else
                        {
                            distance[x, y, SW] =
                            -1 + jumpDistance;
                        }
                    }

                    // SouthEast jumpPoints and Walls
                    if (x == width - 1 || y == 0 || IsWall(x + 1, y) ||
                    IsWall(x, y - 1) || IsWall(x + 1, y - 1))
                    {
                        //Wall one away
                        distance[x, y, SE] = 0;
                    }
                    else if (!IsWall(x + 1, y) && !IsWall(x, y - 1) &&
                    (distance[x + 1, y - 1, SOUTH] > 0 ||
                    distance[x + 1, y - 1, EAST] > 0))
                    {
                        //Straight jump point one away
                        distance[x, y, SE] = 1;
                    }
                    else
                    {
                        //Increment from last
                        int jumpDistance =
                        distance[x + 1, y - 1, SE];
                        if (jumpDistance > 0)
                        {
                            distance[x, y, SE] =
                            1 + jumpDistance;
                        }
                        else
                        {
                            distance[x, y, SE] =
                            -1 + jumpDistance;
                        }
                    }
                }
            }
        }
    }
    private void FindDiagonalJumpPointsAndWallsNS()
    {
        for (int y = height - 1; y >= 0; --y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (!IsWall(x, y))
                {
                    // NorthWest jumpPoints and Walls
                    if (x == 0 || y == height - 1 || IsWall(x - 1, y) ||
                    IsWall(x, y + 1) || IsWall(x - 1, y + 1))
                    {
                        //Wall one away
                        distance[x, y, NW] = 0;
                    }
                    else if (!IsWall(x - 1, y) && !IsWall(x, y + 1) &&
                    (distance[x - 1, y + 1, NORTH] > 0 ||
                    distance[x - 1, y + 1, WEST] > 0))
                    {
                        //Straight jump point one away
                        distance[x, y, NW] = 1;
                    }
                    else
                    {
                        //Increment from last
                        int jumpDistance =
                        distance[x - 1, y + 1, NW];
                        if (jumpDistance > 0)
                        {
                            distance[x, y, NW] =
                            1 + jumpDistance;
                        }
                        else
                        {
                            distance[x, y, NW] =
                            -1 + jumpDistance;
                        }
                    }

                    // NorthEast jumpPoints and Walls
                    if (x == width - 1 || y == height - 1 || IsWall(x + 1, y) ||
                    IsWall(x, y + 1) || IsWall(x + 1, y + 1))
                    {
                        //Wall one away
                        distance[x, y, NE] = 0;
                    }
                    else if (!IsWall(x + 1, y) && !IsWall(x, y + 1) &&
                    (distance[x + 1, y + 1, NORTH] > 0 ||
                    distance[x + 1, y + 1, EAST] > 0))
                    {
                        //Straight jump point one away
                        distance[x, y, NE] = 1;
                    }
                    else
                    {
                        //Increment from last
                        int jumpDistance =
                        distance[x + 1, y + 1, NE];
                        if (jumpDistance > 0)
                        {
                            distance[x, y, NE] =
                            1 + jumpDistance;
                        }
                        else
                        {
                            distance[x, y, NE] =
                            -1 + jumpDistance;
                        }
                    }
                }
            }
        }
    }

    private bool IsWall(int x, int y)
    {
        NodeRecord nr = grid.GetGridObject(x, y);
        if (nr != null)
            return !nr.isWalkable;
        return true;
    }
    //-----------------------------------------------------------------------------------//

    //--------------------------------RUN_TIME ------------------------------------------//
    public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false)
    {
        for (int exploredNodes = 0; exploredNodes < MaxExploredNodesPerFrame; ++exploredNodes)
        {
            int numberOfOpenNodes = Open.CountOpen();

            if (numberOfOpenNodes > MaxOpenNodes)
                MaxOpenNodes = numberOfOpenNodes;

            // Are there no more nodes to search?
            if (numberOfOpenNodes == 0)
            {
                solution = null;
                return true;
            }

            // CurrentNode is the best one from the Open set, start with that
            NodeRecord currentNode = Open.GetBestAndRemove();

            // Is this the Goal node?
            if (currentNode.x == GoalNode.x && currentNode.y == GoalNode.y)
            {
                solution = CalculatePath(currentNode);
                return true;
            }

            currentNode.status = NodeStatus.Closed;
            Closed.AddToClosed(currentNode);
            grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

            ++TotalProcessedNodes;

            NodeRecord parent = currentNode.parent;
            int direction;

            if (parent == null)
                direction = -1;
            else
                direction = ToDir(new Vector2(currentNode.x - parent.x, currentNode.y - parent.y));

            Vector2 dir_to_goal = new Vector2(GoalNode.x - currentNode.x, GoalNode.y - currentNode.y);
            int[] validDirections;
            bool found = validDirLookupTable.TryGetValue(direction, out validDirections);
            if (found)
            {
                foreach (int dir in validDirections)
                {
                    NodeRecord successor = null;
                    float givenCost = 0;

                    if (IsCardinal(dir) && IsGoalInExactDirection(dir, dir_to_goal.normalized)
                        && dir_to_goal.magnitude <= Mathf.Abs(distance[currentNode.x, currentNode.y, dir]))
                    {
                        successor = GoalNode;
                        givenCost = currentNode.gCost + CalculateDistanceCost(currentNode, successor);
                    }
                    else if (IsDiag(dir) && IsGoalInGeneralDirection(dir, dir_to_goal) &&
                            (Mathf.Abs(dir_to_goal.x) <= Mathf.Abs(distance[currentNode.x, currentNode.y, dir]) ||
                            Mathf.Abs(dir_to_goal.y) <= Mathf.Abs(distance[currentNode.x, currentNode.y, dir])))
                    {

                        int minDiff = (int)Mathf.Min(Mathf.Abs(currentNode.x - GoalNode.x), Mathf.Abs(currentNode.y - GoalNode.y));

                        if (minDiff == 0) // If minDiff is zero the target jump point will be itself
                            minDiff = Mathf.Abs(distance[currentNode.x, currentNode.y, dir]);

                        Vector2 dir_coords = getDirCoords(dir);
                        successor = grid.GetGridObject(currentNode.x + minDiff * (int)dir_coords.x, currentNode.y + minDiff * (int)dir_coords.y);
                        givenCost = currentNode.gCost + CalculateDistanceCost(currentNode, successor);

                    }
                    else if (distance[currentNode.x, currentNode.y, dir] > 0)
                    {
                        Vector2 dir_coords = getDirCoords(dir);

                        int dist = distance[currentNode.x, currentNode.y, dir];
                        successor = grid.GetGridObject(currentNode.x + dist * (int)dir_coords.x, currentNode.y + dist * (int)dir_coords.y);
                        givenCost = currentNode.gCost + CalculateDistanceCost(currentNode, successor);

                    }

                    //Trad A*

                    if (successor != null)
                    {
                        ProcessSuccessorNode(successor, currentNode, (int)givenCost);

                    }


                }
            }
        }
        solution = null;

        return false;
    }
    //-----------------------------------------------------------------------------------//

    public override List<NodeRecord> CalculatePath(NodeRecord endNode)
    {
        List<NodeRecord> path = new List<NodeRecord>();
        path.Add(endNode);
        NodeRecord currentNode = endNode;
        //Go through the list of nodes from the end to the beggining
        while (currentNode.parent != null)
        {
            path.Add(currentNode.parent);
            currentNode = currentNode.parent;

        }
        //the list is reversed
        path.Reverse();
        return path;
    }

    public bool IsCardinal(int direction)
    {
        return direction == NORTH || direction == SOUTH || direction == WEST || direction == EAST;
    }

    public bool IsDiag(int direction)
    {
        return direction >= 4 && direction <= 7;
    }

    public bool IsGoalInExactDirection(int direction, Vector2 dir_to_goal)
    {
        switch(direction)
        {
            case NORTH: return dir_to_goal.x == 0 && dir_to_goal.y > 0;
            case EAST : return dir_to_goal.x > 0 && dir_to_goal.y == 0;
            case WEST : return dir_to_goal.x < 0 && dir_to_goal.y == 0;
            case SOUTH: return dir_to_goal.x == 0 && dir_to_goal.y < 0;
            default: return false;
        };
    }

    public bool IsGoalInGeneralDirection(int direction, Vector2 dir_to_goal)
    {
        if (dir_to_goal.x == 0 && dir_to_goal.y == 0)
            return false;

        switch(direction)
        {
            case NE:return dir_to_goal.x >= 0 && dir_to_goal.y >= 0;
            case NW:return dir_to_goal.x <= 0 && dir_to_goal.y >= 0;
            case SE:return dir_to_goal.x >= 0 && dir_to_goal.y <= 0;
            case SW:return dir_to_goal.x <= 0 && dir_to_goal.y <= 0;
            default: return false;
        };
    }

    public int ToDir(Vector2 vector)
    {
        if (vector.x > 0 && vector.y > 0)
            return NE;

        if (vector.x < 0 && vector.y > 0)
            return NW;

        if (vector.x > 0 && vector.y < 0)
            return SE;

        if (vector.x < 0 && vector.y < 0)
            return SW;

        if (vector.x == 0 && vector.y > 0)
            return NORTH;

        if (vector.x == 0 && vector.y < 0)
            return SOUTH;

        if (vector.x > 0 && vector.y == 0)
            return EAST;

        if (vector.x < 0 && vector.y == 0)
            return WEST;
        return -1;
    }

    private int CalculateDistanceCost(NodeRecord a, NodeRecord b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private Vector2 getDirCoords(int direction)
    {
        switch(direction)
        {
            case NORTH: return new Vector2(0, 1);
            case EAST: return new Vector2(1, 0);
            case WEST: return new Vector2(-1, 0);
            case SOUTH: return new Vector2(0, -1);
            case NE: return new Vector2(1, 1);
            case NW: return new Vector2(-1, 1);
            case SE: return new Vector2(1, -1);
            case SW: return new Vector2(-1, -1);
            default: return new Vector2(0, 0);
        };
    }

    protected virtual void ProcessSuccessorNode(NodeRecord successorNode, NodeRecord currentNode, int givenCost)
    {
        NodeRecord successor = GenerateSucessorNodeRecord(currentNode, successorNode);
        NodeRecord successorinOpen = Open.SearchInOpen(successor);
        NodeRecord successorinClosed = Closed.SearchInClosed(successor);

        if (successorinOpen == null && successorinClosed == null)
        {
            Open.AddToOpen(successor);
            successor.status = NodeStatus.Open;
            grid.SetGridObject(successor.x, successor.y, successor);
        }
        else if (successorinOpen != null && successor.CompareTo(successorinOpen) < 0)
        {
            Open.Replace(successorinOpen, successor);
            successor.status = NodeStatus.Open;
            grid.SetGridObject(successor.x, successor.y, successor);
        }
        /*else if (successorinClosed != null && successor.CompareTo(successorinClosed) < 0)
        {
            Closed.RemoveFromClosed(successorinClosed);
            Open.AddToOpen(successor);
            successor.status = NodeStatus.Open;
            grid.SetGridObject(successor.x, successor.y, successor);
        }*/
    }
    protected virtual NodeRecord GenerateSucessorNodeRecord(NodeRecord currentNode, NodeRecord successor)
    {
        var childNodeRecord = new NodeRecord(successor.x, successor.y, height * successor.x + successor.y)
        {
            parent = currentNode,
            gCost = currentNode.gCost + CalculateDistanceCost(currentNode, successor),
            hCost = this.Heuristic.H(successor, this.GoalNode)
        };

        childNodeRecord.CalculateFCost();

        return childNodeRecord;
    }

    public override void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
    {
        this.StartPositionX = startX;
        this.StartPositionY = startY;
        this.GoalPositionX = goalX;
        this.GoalPositionY = goalY;
        this.StartNode = grid.GetGridObject(StartPositionX, StartPositionY);
        this.GoalNode = grid.GetGridObject(GoalPositionX, GoalPositionY);

        //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
        if (this.StartNode == null || this.GoalNode == null) return;

        this.InProgress = true;
        this.TotalProcessedNodes = 0;
        this.TotalProcessingTime = 0.0f;
        this.MaxOpenNodes = 0;

        var initialNode = new NodeRecord(StartNode.x, StartNode.y, height * StartNode.x + StartNode.y)
        {
            gCost = 0,
            hCost = this.Heuristic.H(this.StartNode, this.GoalNode)
        };

        initialNode.CalculateFCost();

        this.Open.Initialize();
        this.Open.AddToOpen(initialNode);
        this.Closed.Initialize();
    }

    public override NodeRecord GetNode(int x, int y)
    {
        throw new System.NotImplementedException();
    }
}
