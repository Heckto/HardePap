SetUserInput(false);
SetTransition(false);
camera.focussed = false;
await MovePlayer(lvl.player.Position + new Vector2(500, 0),0.0002f);
camera.focussed = true;
SetTransition(true);
SetUserInput(true);