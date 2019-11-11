using System;
using System.Collections.Generic;
using System.Linq;
using AuxLib;

namespace AuxLib.CollisionDetection
{
        /// <summary>
        /// Basic spacial hashing of world's boxes.
        /// </summary>
        public class Grid
        {
        public Microsoft.Xna.Framework.Vector2 origin;    

            public class Cell
        {
            public Cell(int x, int y, float cellSize)
            {
                this.Bounds = new RectangleF(x * cellSize, y * cellSize, cellSize, cellSize);
            }

            public RectangleF Bounds { get; private set; }

            public IEnumerable<IShape> Children => this.children;

            private List<IShape> children = new List<IShape>();

            public void Add(IShape box)
            {
                this.children.Add(box);
            }

            public bool Contains(IShape box)
            {
                return this.children.Contains(box);
            }

            public bool Remove(IShape box)
            {
                return this.children.Remove(box);
            }

            public int Count()
            {
                return this.children.Count;
            }
        }

        public Grid(Microsoft.Xna.Framework.Vector2 origin,int width, int height, float cellSize)
        {
            this.Cells = new Cell[width, height];
            this.CellSize = cellSize;
            this.origin = origin;
        }

        public float CellSize { get; set; }

        #region Size

        public float Width => this.Columns * CellSize;

        public float Height => this.Rows * CellSize;

        public int Columns => this.Cells.GetLength(0);

        public int Rows => this.Cells.GetLength(1);

        #endregion

        public Cell[,] Cells { get; private set; }

        public IEnumerable<Cell> QueryCells(float x, float y, float w, float h)
        {
            x = x - origin.X;
            y = y - origin.Y;
            var minX = (int)(x / this.CellSize);
            var minY = (int)(y / this.CellSize);
            var maxX = (int)((x + w - 1) / this.CellSize);
            //var maxX = (int)((x + w - 1) / this.CellSize) + 1;
            var maxY = (int)((y + h - 1) / this.CellSize);
            //var maxY = (int)((y + h - 1) / this.CellSize) + 1;

            minX = Math.Max(0, minX);
            minY = Math.Max(0, minY);
            maxX = Math.Min(this.Columns - 1, maxX);
            maxY = Math.Min(this.Rows - 1, maxY);

            List<Cell> result = new List<Cell>();

            for (int ix = minX; ix <= maxX; ix++)
            {
                for (int iy = minY; iy <= maxY; iy++)
                {
                    var cell = Cells[ix, iy];

                    if (cell == null)
                    {
                        cell = new Cell(ix, iy, CellSize);
                        Cells[ix, iy] = cell;
                    }

                    result.Add(cell);
                }
            }

            return result;

        }

        public IEnumerable<IShape> QueryBoxes(float x, float y, float w, float h)
        {
            var cells = this.QueryCells(x, y, w, h);

            return cells.SelectMany((cell) => cell.Children).Distinct();
        }

        public void Add(IBox box)
        {
            var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

            foreach (var cell in cells)
            {
                if (!cell.Contains(box))
                    cell.Add(box);
            }
        }

        public void Add(IPolyLine line)
        {

            var box = line.getPolylineBB();
            var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

            foreach (var cell in cells)
            {
                if (!cell.Contains(line))
                    cell.Add(line);
            }
        }

        public void Update(IShape box, RectangleF from)
        {
            var fromCells = this.QueryCells(from.X, from.Y, from.Width, from.Height);
            var removed = false;
            foreach (var cell in fromCells)
            {
                removed |= cell.Remove(box);
            }

            if (removed)
            {
                if (box is IBox)
                    this.Add((IBox)box);
                else
                    this.Add((IPolyLine)box);
            }
        }

        public bool Remove(IBox box)
        {
            var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

            var removed = false;
            foreach (var cell in cells)
            {
                removed |= cell.Remove(box);
            }

            return removed;
        }

        public bool Remove(IPolyLine line)
        {
            var box = line.getPolylineBB();
            var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

            var removed = false;
            foreach (var cell in cells)
            {
                removed |= cell.Remove(line);
            }

            return removed;
        }

        public override string ToString()
        {
            return string.Format("[Grid: Width={0}, Height={1}, Columns={2}, Rows={3}]", Width, Height, Columns, Rows);
        }
    }
}

