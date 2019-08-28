using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Game1.DataContext;
using Microsoft.Xna.Framework;
using System.Threading;

namespace Game1.Scripting
{
    public class ScriptingHost
    {
        private FrameNotifyer frameNotifyer;

        private Dictionary<string, Script> Scripts = new Dictionary<string, Script>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, MethodInfo> contextMethods = new Dictionary<string, MethodInfo>(StringComparer.InvariantCultureIgnoreCase);

        private Dictionary<string, Script> runningScripts = new Dictionary<string, Script>(StringComparer.InvariantCultureIgnoreCase);
        private GameContext dataContext;

        private CompilerParameters parameters;

        public ScriptingHost(GameContext context)
        {
            dataContext = context;
            var t = context.GetType();
            MethodInfo[] mi = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in mi)
            {
                contextMethods.Add(method.Name, method);
            }

            parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };

            var assemblies = GetType().Assembly.GetReferencedAssemblies().ToList();
            var assemblyLocations = assemblies.Select(a => Assembly.ReflectionOnlyLoad(a.FullName).Location).ToList();
            assemblyLocations.Add(GetType().Assembly.Location);
            parameters.ReferencedAssemblies.AddRange(assemblyLocations.ToArray());

            frameNotifyer = new FrameNotifyer()
            {
                gameTime = new GameTime(),
                token = new CancellationTokenSource()
            };
            dataContext.currentFrameSource = new TaskCompletionSource<FrameNotifyer>();
        }

        public async void LoadScript(string[] files, GameContext context)
        {
            await Task.Run(() =>
            {
                var compiler = new CSharpCodeProvider();
                var result = compiler.CompileAssemblyFromFile(parameters, files);
                if (result.Errors.Count > 0)
                {
                    foreach (CompilerError error in result.Errors)
                        Console.WriteLine(error.ErrorText);
                }
                else
                {
                    var types = result.CompiledAssembly.GetTypes();
                    var scriptTypes = types.Where(p => typeof(Script).IsAssignableFrom(p));
                    foreach (var scriptType in scriptTypes)
                    {
                        var c = scriptType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                        if (c == null)
                            throw new InvalidOperationException(string.Format("A constructor for type '{0}' was not found.", typeof(Script)));
                        var instance = (Script)c.Invoke(new object[] { });
                        instance.dataContext = context;
                        Scripts.Add(scriptType.Name, instance);
                    }
                }
            });
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
            frameNotifyer.token.Cancel();
        }

        public void ExecuteScript(string scriptId)
        {
            try
            {
                if (Scripts.ContainsKey(scriptId))
                {
                    var scriptObj = Scripts[scriptId];
                    scriptObj.Execute();
                    scriptObj.State = ScriptState.Running;
                    runningScripts.Add(scriptId, scriptObj);
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ExecuteExpression(string command, out object reply)
        {
            reply = null;
            var arguments = command.Split(new char[] { ' ' });

            if (arguments[0].Equals("?"))
                reply = getCommands();
            else if (arguments[0].Equals("Loaded",StringComparison.OrdinalIgnoreCase))
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
                reply = ExecuteExpression(command);
            }
        }

        public string ExecuteExpression(string command)
        {            
            var arguments = command.Split(new char[] { ' ' });
            try
            {
                if (arguments.Length > 0 && contextMethods.ContainsKey(arguments[0]))
                {
                    var argumentList = arguments.Skip(1).Take(arguments.Length - 1).ToArray();
                    return contextMethods[arguments[0]].Invoke(dataContext, argumentList as object[]).ToString();
                }
                else
                    return $"Invalid command {arguments}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }            
        }

        #region Meta commands

        public string getCommands()
        {
            var msg = String.Empty;
            foreach (var item in contextMethods)
            {
                msg += item.Key + " " + item.Value.ToString() + " " + Environment.NewLine;
            }
            return msg;
        }

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

        #endregion
    }
}