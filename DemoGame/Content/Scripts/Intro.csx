﻿SetUserInput(false);
SetTransition(false);
camera.focussedOnPlayer = false;
camera.Position = new Vector2(3300, 3139);
await Task.Delay(2000);
//SetAnimation("Run");
await MovePlayer(new Vector2(1000, 0),0.01f);
DisplayHUDText("test","Forbidden Forest",2,1);
camera.focussedOnPlayer = true;
SetTransition(true);
SetUserInput(true);