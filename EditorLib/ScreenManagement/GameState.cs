using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Input;

namespace AuxLib.ScreenManagement
{
    public abstract partial class GameState : DrawableGameComponent, IGameState
    {
        public bool BlockDrawing = false;
        public bool BlockUpdating = false;
        protected IGameStateManager GameManager;
        protected IInputHandler Input;
        protected Rectangle TitleSafeArea;

        public GameState(Game game) : base(game)
        {
            GameManager = game.Services.GetService<GameStateManager>();
            Input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        internal protected virtual void StateChanged(object sender, EventArgs e)
        {
            if (GameManager.State == Value)
                Visible = Enabled = true;
            else 
                Visible = Enabled = false;
        }

        

        public GameState Value
        {
            get { return (this); }
        }

    }
}
