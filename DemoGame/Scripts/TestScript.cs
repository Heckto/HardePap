using System;
using System.Collections;
using System.Threading.Tasks;
using Game1.DataContext;
using Game1.Scripting;
using Microsoft.Xna.Framework;

public class Script1 : Script
{
    protected async override Task RunScript()
    {
        dataContext.SetUserInput(false);
        dataContext.camera.focussed = false;
        var targetPos = new Vector2(8000, 2500);
        await dataContext.MoveCamera(targetPos, 0.02f);
        await Task.Delay(2000);
        var boss = dataContext.SpawnEnemy("sjaak", 9100, 2900);
        boss.Direction = Game1.Sprite.SpriteObject.FaceDirection.Left;
        boss.HandleInput = false;
        var msg = "President MolenKampf has been kidnapped by the ninjas. Are you a bad enough dude to rescue him.\nWhat is a man? A miserable little pile of secrets. But enough talk... Have at you!";
        var pic = "Macho";
        await dataContext.DisplayDialog(msg,pic);
        await Task.Delay(1000);
        await dataContext.MoveCamera(dataContext.lvl.player.Position - dataContext.camera.Origin, 0.02f);

        await dataContext.MovePlayer(dataContext.lvl.player.Position + new Vector2(500, 0),0.0002f);

        msg = "Stop annoying me, yeah! I play my music loud It takes the Old Dirty Niss, to move the crowd They say he had his dick in his mouth Frits Bom taught me that back in the house Now give me my money!";
        pic = "ninja";
        await dataContext.DisplayDialog(msg, pic);

        dataContext.camera.focussed = true;
        await Task.Delay(1500);
        dataContext.camera.Zoom = 3;
        await Task.Delay(2000);
        dataContext.playSFX("kahn");
        await Task.Delay(2000);
        dataContext.camera.Zoom = 1;
        dataContext.SetUserInput(true);
    }
}