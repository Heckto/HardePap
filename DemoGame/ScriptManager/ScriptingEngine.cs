using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Game1.DataContext;
using Microsoft.Xna.Framework;
using System.Threading;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using QuakeConsole;

namespace Game1.Scripting
{
    public class ScriptingEngine : ICommandInterpreter
    {
        private readonly FrameNotifyer frameNotifyer;
        
        private readonly Dictionary<string, Script> Scripts = new Dictionary<string, Script>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Script> runningScripts = new Dictionary<string, Script>(StringComparer.InvariantCultureIgnoreCase);
        private readonly GameContext dataContext;

        private readonly CancellationTokenSource cancelSource;
        private readonly CancellationToken cancelToken;
        private readonly ScriptOptions scriptingOptions;

        public ScriptingEngine(GameContext context)
        {
            var references = new Assembly[] {   typeof(Vector2).Assembly,
                                                typeof(Task).Assembly,
                                                GetType().Assembly,
            };


            var namespaces = new string[] {     "Microsoft.Xna.Framework",
                                                "System.Threading.Tasks",
                                                "Game1.Sprite.SpriteObject"
            };
            scriptingOptions = ScriptOptions.Default.WithReferences(references).WithImports(namespaces);

            dataContext = context;
            frameNotifyer = new FrameNotifyer()
            {
                gameTime = new GameTime(),
                token = new CancellationTokenSource()
            };
            dataContext.currentFrameSource = new TaskCompletionSource<FrameNotifyer>();
            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;
        }

        public void LoadScript(string[] files)
        {
            ClearAllScripts();
            Task.Run(() =>
            {
                foreach (var f in files)
                {
                    var code = File.ReadAllText(f);
                    var script = CSharpScript.Create(code, scriptingOptions, typeof(GameContext));
                    var runner = script.CreateDelegate(cancelToken);
                    Scripts.Add(Path.GetFileNameWithoutExtension(f), new Script(runner));
                }
            }).Wait();
        }

        public void ClearAllScripts()
        {
            Scripts.Clear();
            runningScripts.Clear();
        }

        public void Update(GameTime gameTime)
        {
            var removeList = new List<string>();
            foreach (var item in runningScripts)
            {
                if (item.Value.State == ScriptState.Error)
                    Console.WriteLine("ERROR");
                else if (item.Value.State == ScriptState.Done)
                    removeList.Add(item.Key);
            }
            if (removeList.Count() > 0)
            {
                foreach (var removeItem in removeList)
                {
                    // Allow for multiple runs of the same script
                    Scripts[removeItem].State = ScriptState.Ready;
                    runningScripts.Remove(removeItem);
                }
            }

            frameNotifyer.gameTime = gameTime;
            var previousFrameSource = dataContext.currentFrameSource;

            dataContext.currentFrameSource = new TaskCompletionSource<FrameNotifyer>();

            previousFrameSource.SetResult(frameNotifyer);
        }

        public void CancelScript()
        {
            cancelSource.Cancel();
            frameNotifyer.token.Cancel();
        }

        public async void ExecuteScript(string scriptId)
        {
            try
            {
                if (Scripts.ContainsKey(scriptId))
                {                    
                    var scriptObj = Scripts[scriptId];
                    await scriptObj.script(dataContext, cancelToken);
                    runningScripts.Add(scriptId, scriptObj);
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

       

        public async Task<string> ExecuteExpression(string command)
        {
            var result = await CSharpScript.RunAsync(command, null, dataContext, typeof(GameContext));
            return result.ReturnValue.ToString();       
        }

        #region Meta commands       

        public string getRunningScripts()
        {
            var msg = String.Empty;
            foreach (var item in runningScripts)
            {
                msg += item.Key + " " + item.Value.ToString() + " " + Environment.NewLine;
            }
            return msg;
        }

        public string getLoadedScripts()
        {
            var msg = String.Empty;
            foreach (var item in Scripts)
            {
                msg += $"{item.Key} {item.Value.State}" + Environment.NewLine;
            }
            return msg;
        }

        public bool hasScriptLoaded(string script)
        {
            return Scripts.ContainsKey(script);
        }

        public async void Execute(IConsoleOutput output, string command)
        {
            try
            {
                var reply = string.Empty;
                var arguments = command.Split(new char[] { ' ' });

                if (arguments[0].Equals("Loaded", StringComparison.OrdinalIgnoreCase))
                {
                    reply = getLoadedScripts();
                }
                else if (arguments[0].Equals("Running", StringComparison.OrdinalIgnoreCase))
                {
                    reply = getRunningScripts();
                }
                else if (arguments[0].Equals("Exec", StringComparison.OrdinalIgnoreCase))
                {
                    var scriptFile = arguments[1];
                    ExecuteScript(scriptFile);
                }
                else if (arguments[0].Equals("Cancel", StringComparison.OrdinalIgnoreCase))
                {
                    CancelScript();
                }
                else
                {
                    reply = await ExecuteExpression(command);
                }
                output.Append(reply);
            }
            catch(Exception ex)
            {
                output.Append(ex.Message);
            }
            
        }

        public void Autocomplete(IConsoleInput input, bool forward)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}