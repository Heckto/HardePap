using Microsoft.Xna.Framework;
using AuxLib.ScreenManagement;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using Game1.Screens;

namespace Game1.Levels
{
    public class TransitionManager
    {
        private GameStateManager stateManager;
        private DemoGame gameInstance;
        private readonly string contentDirectory;
        public bool canTransition = true;
        public bool isTransitioning = false;

        public TransitionManager(DemoGame game, GameStateManager gameStateManager, string contentDir)
        {
            contentDirectory = contentDir;
            gameInstance = game;
            stateManager = gameStateManager;

        }

        public void TransitionToMap(string mapName)
        {
            if (canTransition)
            {
                isTransitioning = true;
                var levelfile = Path.Combine(contentDirectory, mapName);
                stateManager.PushState(new PlayState(gameInstance, levelfile), new FadeTransition(gameInstance.GraphicsDevice, Color.Black, 2.0f));
            }
        }


    }
}
