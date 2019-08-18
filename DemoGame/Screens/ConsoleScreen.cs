using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using QuakeConsole;
using Game1.DataContext;
using Game1.Scripting;

namespace Game1.Screens
{   

    public class ConsoleScreen : BaseGameState, IConsoleState
    {
        private ConsoleComponent console;
        private ScriptingManager scriptManager;
        private GameContext context;

        public ConsoleScreen(DemoGame game) : base(game)
        {
            BlockDrawing = false;
            BlockUpdating = true;

            console = game.Services.GetService<ConsoleComponent>();
            
            console.ToggleOpenClose();
            console.LogInput = handleConsoleInput;
            
            scriptManager = game.Services.GetService<ScriptingManager>();            
            context = game.Services.GetService<GameContext>();
        }

        public async void handleConsoleInput(string command)
        {
            try
            {
                await scriptManager.ExecuteExpression(command, context);
            }
            catch(Exception ex)
            {                
                console.Output.Append(ex.Message);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            console.Update(gameTime);

            if (Input.WasPressed(0, Buttons.LeftStick, Keys.OemTilde))
            {
                console.ToggleOpenClose();
                //OurGame.Components.Remove(console);
                
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
