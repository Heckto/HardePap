using System;
using System.Collections;
using System.Threading.Tasks;
using Game1.DataContext;
using Game1.Scripting;
using Microsoft.Xna.Framework;

public class Script1 : Script
{
    public Script1(GameContext context) : base(context) { }

    public async override void Update(GameTime gameTime)
    {        
        dataContext.DisableUserInput();
        dataContext.camera.focussed = false;
        var targetPos = new Vector2(15000, 3000);
        await dataContext.MoveCamera(targetPos, 50);
        await Task.Delay(2000);
        dataContext.camera.focussed = true;
        State = ScriptState.Done;
    }
}