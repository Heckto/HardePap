﻿SetUserInput(false);
camera.focussed = false;
HaltPlayer();
var targetPos = new Vector2(4450, 250);
await MoveCamera(targetPos, 0.02f);
await Task.Delay(2000);
var boss = SpawnEnemy("sjaak", 5450, 350);
boss.Direction = Game1.Sprite.SpriteObject.FaceDirection.Left;
boss.HandleInput = false;
var msg = "President MolenKampf has been kidnapped by the ninjas. Are you a bad enough dude to rescue him.\nWhat is a man? A miserable little pile of secrets. But enough talk... Have at you!";
var pic = "Macho";
await DisplayDialog(msg, pic);
await Task.Delay(1000);
await MoveCamera(lvl.player.Position - camera.Origin, 0.02f);
msg = "Stop annoying me, yeah! I play my music loud It takes the Old Dirty Niss, to move the crowd They say he had his dick in his mouth Frits Bom taught me that back in the house Now give me my money!";
pic = "ninja";
await DisplayDialog(msg, pic);
camera.focussed = true;
await Task.Delay(1500);
camera.Zoom = 3;
await Task.Delay(2000);
playSFX("kahn");
await Task.Delay(2000);
camera.Zoom = 1;
SetUserInput(true);