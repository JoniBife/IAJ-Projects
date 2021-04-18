using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System;
using UnityEngine.UI;
using System.Linq;

namespace Assets.Scripts.Grid
{
    public class Grid<TGridObject>
    {

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private TGridObject[,] gridArray;
        private float cellSize;
       

        public Grid(int width, int height, float cellSize, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new TGridObject[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++)
                for (int y = 0; y < gridArray.GetLength(1); y++) {

                    gridArray[x, y] = createGridObject(this, x, y);
                  
                }
           
        }


        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) * cellSize;
        }

        public int getWidth()
        {
            return this.width;
        }
        public int getHeight()
        {
            return this.height;
        }
        public void SetGridObject(int x, int y, TGridObject value)
        {
            //What happens when the value is unaceptable let's igone them for now
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                gridArray[x, y] = value;

                OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }

        }

        // Get from World Position to grid coordinate
        public void GetXY(Vector3 WorldPosition, out int x, out int y)
        {
          
            x = Mathf.FloorToInt(WorldPosition.x / cellSize);
            //Careful with the z coordinate
            y = Mathf.FloorToInt(WorldPosition.z / cellSize);

        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            SetGridObject(x, y, value);
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
                return gridArray[x, y];
            else return default(TGridObject);
        }

    
        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetGridObject(x, y);
        }


        public List<TGridObject> getAll()
        {
            return gridArray.Cast<TGridObject>().ToList();
        }
     
    }

}