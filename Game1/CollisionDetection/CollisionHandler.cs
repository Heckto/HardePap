using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helper;

namespace Game1.CollisionDetection
{
    public class CollisionHandler
    {
        private bool debug = true;

        private Level level;

        public List<Rectangle> colRectangles = new List<Rectangle>();
        public List<PathItem> colEdges = new List<PathItem>();

        public delegate void onBottomCollideWithRectangleDelegate(Rectangle rec, Vector2 checkPos);
        public event onBottomCollideWithRectangleDelegate onBottomCollideWithRectangle;

        public delegate void onTopCollideWithRectangleDelegate(Rectangle rec, Vector2 checkPos);
        public event onTopCollideWithRectangleDelegate onTopCollideWithRectangle;

        public delegate void onLeftCollideWithRectangleDelegate(Rectangle rec, Vector2 checkPos);
        public event onLeftCollideWithRectangleDelegate onLeftCollideWithRectangle;

        public delegate void onRightCollideWithRectangleDelegate(Rectangle rec, Vector2 checkPos);
        public event onRightCollideWithRectangleDelegate onRightCollideWithRectangle;

        public CollisionHandler(Level lvl)
        {
            level = lvl;

            var l = level.Layers.FirstOrDefault(elem => elem.Name == "collision");
            foreach (var elem in l.Items)
            {
                if (elem is RectangleItem)
                {                    
                    var r = elem as RectangleItem;                    
                    colRectangles.Add(new Rectangle((int)(r.Position.X), (int)(r.Position.Y), (int)r.Width, (int)r.Height));
                }
                else if (elem is PathItem)
                {
                    var p = elem as PathItem;
                    colEdges.Add(p);
                }
            }
        }

        public void RegisterCollisionRectangle(Rectangle item)
        {
            colRectangles.Add(item);
        }

        public void RegisterCollisionPolyline(PathItem item)
        {
            colEdges.Add(item);
        }

        public void Draw(SpriteBatch sb,Camera camera)
        {
            if (debug)
            {
                sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, camera.matrix); ;
                foreach (var r in colRectangles)
                    LineBatch.DrawRectangle(sb, Color.Green, r);
                foreach (var e in colEdges)
                {
                    for (var idx = 0; idx < e.WorldPoints.Length - 1; idx++)
                        LineBatch.DrawLine(sb, Color.Green, e.WorldPoints[idx], e.WorldPoints[idx + 1]);
                }
                sb.End();
            }
        }


        public bool checkForYRectangleCollision(Vector2 nLoc, Vector2 traj,Rectangle currentRect)
        {
            foreach (var rectangle in colRectangles)
            {
                //rectangle.Intersects(currentRect)
                if (rectangle.Contains(nLoc))
                //if (rectangle.Intersects(currentRect))
                {
                    if (traj.Y > 0)
                        onBottomCollideWithRectangle?.Invoke(rectangle, nLoc);
                    else if (traj.Y < 0)
                        onTopCollideWithRectangle?.Invoke(rectangle, nLoc);
                    return true;
                }
            }
            return false;
        }

        public bool checkForXRectangleCollision(Vector2 nLoc, Vector2 traj, Rectangle currentRect)
        {
            foreach (var rectangle in colRectangles)
            {
                if (rectangle.Contains(nLoc))
                {
                    if (traj.X > 0)
                        onRightCollideWithRectangle?.Invoke(rectangle, nLoc);
                    else if (traj.X < 0)
                        onLeftCollideWithRectangle?.Invoke(rectangle, nLoc);
                    return true;
                }
            }
            return false;
        }

        public void DoCollisionCheck(Vector2 current_location,Vector2 trajectory,Rectangle currentRect)
        {
            var checkLocation = current_location;
            if (trajectory.Y > 0)
                // bottom
                checkLocation = new Vector2(current_location.X, currentRect.Bottom);
            else if (trajectory.Y < 0)
                // top
                checkLocation = new Vector2(current_location.X, currentRect.Top);
            
            checkForYRectangleCollision(checkLocation, trajectory, currentRect);

            if (trajectory.X > 0)
                // bottom
                checkLocation = new Vector2(currentRect.Right, current_location.Y);
            else if (trajectory.X < 0)
                // top
                checkLocation = new Vector2(currentRect.Left, current_location.Y);

            checkForXRectangleCollision(checkLocation, trajectory, currentRect);
        }
    }
}
