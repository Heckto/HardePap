using AuxLib.ScreenManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Screens
{

    public interface IConsoleState : IGameState { }
    public interface IIntroState : IGameState { }
    public interface IPlayGameState : IGameState { }
    public interface IOptionsState : IGameState { }

    public interface IFadingState : IGameState
    {
        Color Color { get; set; }
    }


    public partial class BaseGameState : GameState
    {
        protected DemoGame OurGame;
        protected ContentManager Content;
        public BaseGameState(Game game) : base(game) 
        {
            
            Content = new ContentManager(game.Services,"Content");
            OurGame = (DemoGame)game;
        }
    }
}
