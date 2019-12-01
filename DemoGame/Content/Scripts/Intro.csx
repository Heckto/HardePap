SetUserInput(false);
SetTransition(false);
camera.focussedOnPlayer = false;
await Task.Delay(2000);
await MovePlayer(lvl.player.Position + new Vector2(1000, 0),0.0002f);
DisplayHUDText("test","Forbidden Forest",2,1);
camera.focussedOnPlayer = true;
SetTransition(true);
SetUserInput(true);