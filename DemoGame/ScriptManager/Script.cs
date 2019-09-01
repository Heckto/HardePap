using Game1.DataContext;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Game1.Scripting
{
    public enum ScriptState { Ready, Running, Done, Error, Cancelled };

    public class Script
    {
        public ScriptState State { get; set; }

        public ScriptRunner<object> script {get;set;}
        
        public Script(ScriptRunner<object> runner)
        {
            State = ScriptState.Ready;
            script = runner;
        }
    }
}