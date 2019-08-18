using Microsoft.Xna.Framework;
using Game1.Levels;
using Game1.Enemies;
using AuxLib.ScreenManagement;
using AuxLib.Camera;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using Game1.Screens;
using System;

namespace Game1.DataContext
{
    public class GameContext
    {

        public BoundedCamera camera;
        public Level lvl;

        public TransitionManager transitionManager;

        public GameStateManager gameManager;

        public void SpawnEnemy(string name, int x, int y)
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

        public string ListPlayers()
        {
            var msg = lvl.player.ToString();
            return msg;
        }

        public string ListEnemies()
        {
            var msg = String.Empty;
            foreach(var entry in lvl.Sprites)
            {
                msg += entry.Key + " " + entry.Value.ToString() + Environment.NewLine;
            }
            return msg;
        }
    }

    public class TransitionManager
    {
        private GameStateManager stateManager;
        private DemoGame gameInstance;
        private readonly string contentDirectory;
        public bool isTransitioning = false;

        public TransitionManager(DemoGame game, GameStateManager gameStateManager,string contentDir)
        {
            contentDirectory = contentDir;
            gameInstance = game;
            stateManager = gameStateManager;
            
        }

        public void TransitionToMap(string mapName)
        {
            isTransitioning = true;
            var levelfile = Path.Combine(contentDirectory, mapName);
            stateManager.PushState(new PlayState(gameInstance, levelfile), new FadeTransition(gameInstance.GraphicsDevice, Color.Black, 2.0f));
        }
    }
}
