﻿using System;
using System.Collections;
using Game1.DataContext;
using Game1.Scripting;
using Microsoft.Xna.Framework;

public class Script2 : Script
{
    public Script2(GameContext context) : base(context) { }

    public override void Update(GameTime gameTime)
    {

        State = ScriptState.Done;
    }
}