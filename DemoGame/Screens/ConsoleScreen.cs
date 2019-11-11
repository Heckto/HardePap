using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using QuakeConsole;
using Game1.Scripting;

namespace Game1.Screens
{

    public class ConsoleScreen : BaseGameState, IConsoleState
    {
        private ConsoleComponent console;
        private readonly ScriptingEngine scriptManager;

        public ConsoleScreen(DemoGame game) : base(game)
        {
            BlockDrawing = false;
            BlockUpdating = false;
            console = game.Services.GetService<ConsoleComponent>();            
            console.ToggleOpenClose();            
            scriptManager = game.Services.GetService<ScriptingEngine>();            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            console.Update(gameTime);
            if (Input.WasPressed(0, Buttons.LeftStick, Keys.OemTilde))
            {
                console.ToggleOpenClose();
            }
            if (!console.IsVisible)
                GameManager.PopState();
        }

        public override void Draw(GameTime gameTime)
        {
            console.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
