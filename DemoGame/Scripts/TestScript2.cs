using System;
using System.Collections;
using System.Threading.Tasks;
using Game1.DataContext;
using Game1.Scripting;
using Microsoft.Xna.Framework;

public class Script2 : Script
{
    protected async override Task RunScript()
    {
        State = ScriptState.Done;
    }
}