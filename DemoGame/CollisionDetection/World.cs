using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using AuxLib.Debug;
using AuxLib;
using Game1.CollisionDetection.Responses;

namespace Game1.CollisionDetection
{
    public class World : IWorld
    {

        public bool isGrounded = false;

        public World(Microsoft.Xna.Framework.Rectangle rect, float cellSize = 256)
        {
            var iwidth = (int)Math.Ceiling(rect.Width / cellSize);
            var iheight = (int)Math.Ceiling(rect.Height / cellSize);

            var origin = new Microsoft.Xna.Framework.Vector2(rect.X, rect.Y);

            grid = new Grid(origin, iwidth, iheight, cellSize);

        }

        public RectangleF Bounds => new RectangleF(grid.origin.X, grid.origin.Y, grid.Width, grid.Height);

        #region Boxes

        private Grid grid;

        public IBox CreateRectangle(float x, float y, float width, float height)
        {
            var box = new Box(this, x, y, width, height);
            grid.Add(box);
            return box;
        }

        public IPolyLine CreatePolyLine(Vector2f[] points)
        {
            var box = new PolyLine(this, points);
            grid.Add(box);
            return box;
        }

        public IMoveableBody CreateMoveableBody(float x, float y, float width, float height, bool grounded = true)
        {
            var box = new MoveableBody(this, x, y, width, height, grounded);
            grid.Add(box);
            return box;
        }

        public IEnumerable<IShape> Find(float x, float y, float w, float h)
        {
            x = Math.Max(0, Math.Min(x, Bounds.Right - w));
            y = Math.Max(0, Math.Min(y, Bounds.Bottom - h));

            return grid.QueryBoxes(x, y, w, h);
        }

        public IEnumerable<IShape> Find(RectangleF area)
        {
            return Find(area.X, area.Y, area.Width, area.Height);
        }

        public bool Remove(IBox box)
        {
            return grid.Remove(box);
        }

        public bool Remove(IPolyLine box)
        {
            return grid.Remove(box);
        }

        public void Update(IShape box, RectangleF from)
        {
            grid.Update(box, from);
        }

        #endregion

        #region Hits

        public IHit Hit(Vector2f point, IEnumerable<IShape> ignoring = null)
        {
            var boxes = Find(point.X, point.Y, 0, 0);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            foreach (var other in boxes)
            {
                var hit = other.Resolve(point);
                //var hit = Humper.Hit.Resolve(point, other);

                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        public IHit Hit(Vector2f origin, Vector2f destination, IEnumerable<IShape> ignoring = null)
        {
            var min = Vector2f.Min(origin, destination);
            var max = Vector2f.Max(origin, destination);

            var wrap = new RectangleF(min, max - min);
            var boxes = Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            IHit nearest = null;

            foreach (var other in boxes)
            {
                var hit = other.Resolve(origin, destination);
                //var hit = Humper.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin)))
                {
                    nearest = hit;
                }
            }

            return nearest;
        }

        public IHit Hit(RectangleF origin, RectangleF destination, IEnumerable<IShape> ignoring = null)
        {
            var wrap = new RectangleF(origin, destination);
            var boxes = Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);



            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            IHit nearest = null;
            foreach (var other in boxes)
            {
                var hit = other.Resolve(origin, destination);
                //var hit = Humper.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Location)))
                {
                    nearest = hit;
                }
            }

            return nearest;
        }

        #endregion

        #region Movements

        public IMovement Simulate(Box box, float x, float y, Func<ICollision, ICollisionResponse> filter)
        {
            isGrounded = (box as MoveableBody).Grounded;
            var origin = box.Bounds;
            var destination = new RectangleF(x, y, box.Width, box.Height);

            var hits = new List<IHit>();

            var result = new Movement()
            {
                Origin = origin,
                Goal = destination,
                Destination = Simulate(hits, new List<IShape>() { box }, box, origin, destination, filter),
                Hits = hits,
            };

            return result;
        }

        private RectangleF Simulate(List<IHit> hits, List<IShape> ignoring, Box box, RectangleF origin, RectangleF destination, Func<ICollision, ICollisionResponse> filter)
        {
            var nearest = Hit(origin, destination, ignoring);

            if (nearest != null)
            {
                hits.Add(nearest);

                var impact = new RectangleF(nearest.Position, origin.Size);
                var collision = new Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
                var response = filter(collision);

                if (response != null && destination != response.Destination)
                {
                    ignoring.Add(nearest.Box);
                    return Simulate(hits, ignoring, box, impact, response.Destination, filter);
                }
            }

            return destination;
        }

        #endregion

        #region Diagnostics

        public void DrawDebug(SpriteBatch spriteBatch, SpriteFont font, int x, int y, int w, int h)
        {

            // Drawing boxes
            var boxes = grid.QueryBoxes(x, y, w, h);
            var color = new Microsoft.Xna.Framework.Color(165, 155, 250);
            foreach (var box in boxes)
            {

                if (box is IBox)
                    spriteBatch.DrawRectangle((box as Box).Bounds, color, 0.3f);
                else if (box is IPolyLine)
                {
                    var p = (box as IPolyLine).Points;
                    var points = new Microsoft.Xna.Framework.Vector2[p.Length];
                    for (var idx = 0; idx < p.Length; idx++)
                        points[idx] = new Microsoft.Xna.Framework.Vector2(p[idx].X, p[idx].Y);
                    spriteBatch.DrawPolyline(color, points);
                }
            }

            // Drawing cells
            var cells = grid.QueryCells(x, y, w, h);
            foreach (var cell in cells)
            {
                var count = cell.Count();
                var alpha = count > 0 ? 1f : 0.4f;
                var rec = new Microsoft.Xna.Framework.Rectangle((int)(cell.Bounds.X + grid.origin.X), (int)(cell.Bounds.Y + grid.origin.Y), (int)cell.Bounds.Width, (int)cell.Bounds.Height);
                spriteBatch.DrawStroke(rec, new Microsoft.Xna.Framework.Color(Microsoft.Xna.Framework.Color.White, alpha));
                spriteBatch.DrawString(font, count.ToString(), (int)(cell.Bounds.Center.X + grid.origin.X), (int)(cell.Bounds.Center.Y + grid.origin.Y), Microsoft.Xna.Framework.Color.White, alpha);
            }

        }

        #endregion
    }
}

