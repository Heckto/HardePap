using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Game1.Scripting
{
    public class ScriptingManager
    {
        private string ScriptDir;
        private ScriptOptions scriptOptions;        
        private Dictionary<string,ScriptRunner<object>> ActiveScripts = new Dictionary<string,ScriptRunner<object>>();

        public ScriptingManager(string scriptDir = "")
        {
            scriptOptions = ScriptOptions.Default
                    .WithReferences(typeof(RuntimeBinderException).Assembly)
                    .WithImports("System");

            if (!String.IsNullOrEmpty(scriptDir))
            {
                ScriptDir = scriptDir;

                var scriptFiles = Directory.GetFiles(ScriptDir, "*.csx");
                foreach (var f in scriptFiles)
                {
                    var scriptId = Path.GetFileNameWithoutExtension(f);
                    var code = File.ReadAllText(f);
                    LoadScript(scriptId, code);
                }
            }
        }





        public void LoadScript(string scriptId, string code)
        {
            Task.Run(() =>
            {                
                var scriptObj = CSharpScript.Create(code, scriptOptions, typeof(GameContext)).CreateDelegate();
                if (ActiveScripts.ContainsKey(scriptId))
                    ActiveScripts[scriptId] = scriptObj;
                else
                    ActiveScripts.Add(scriptId, scriptObj);
            });
        }

        public async Task<object> ExecuteScript(string scriptId, GameContext context)
        {
            if (ActiveScripts.ContainsKey(scriptId))
            {
                return await ActiveScripts[scriptId].Invoke(context);
            }
            return Task.FromResult("invalid script");
        }

        public void ExecuteAllScripts(GameContext context)
        {
            foreach(var scriptId in ActiveScripts.Keys)
            {
                ActiveScripts[scriptId].Invoke(context);
            }
        }

        public async Task<object> ExecuteExpression(string expression, GameContext context)
        {
            return await CSharpScript.EvaluateAsync(expression, scriptOptions, context, typeof(GameContext));
        }

        public string getRunningScripts()
        {
            return String.Join(Environment.NewLine, ActiveScripts.Keys.ToList());
        }

    }

    
}
