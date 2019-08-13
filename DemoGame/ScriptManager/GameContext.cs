using Microsoft.Xna.Framework;
using Game1.Levels;
using Game1.Enemies;
using AuxLib.Camera;
using Microsoft.Xna.Framework.Input;

namespace Game1.Scripting
{
    public class GameContext
    {

        public BoundedCamera camera;
        public Level lvl;

        public void SpawnEnemy(string name,int x, int y)
        {
            var location = new Vector2(x, y);
            lvl.SpawnEnemy(name, location);
        }

        public void SpawnEnemy(string name)
        {
            var m = Matrix.Invert(camera.GetViewMatrix());
            var mousePos = Mouse.GetState().Position.ToVector2();
            var worldPos = Vector2.Transform(mousePos, m);
            lvl.SpawnEnemy(name, worldPos);
        }
    }
}
