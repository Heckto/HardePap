using Microsoft.Xna.Framework;
using AuxLib.ScreenManagement;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using Game1.Screens;

namespace Game1.GameObjects.Levels
{
    /// <summary>
    ///  UHHHHH
    /// </summary>
    public class TransitionManager
    {
        private GameStateManager stateManager;
        private DemoGame gameInstance;        
        public bool canTransition = true;
        public bool isTransitioning = false;

        public TransitionManager(DemoGame game, GameStateManager gameStateManager)
        {
            
            gameInstance = game;
            stateManager = gameStateManager;

        }

        public void TransitionToMap(string mapName)
        {
            if (canTransition)
            {
                
                isTransitioning = true;
                var levelfile = Path.Combine(DemoGame.ContentManager.RootDirectory, mapName);
                stateManager.PushState(new PlayState(gameInstance, levelfile), new FadeTransition(gameInstance.GraphicsDevice, Color.Black, 2.0f));
            }
        }


    }
}
