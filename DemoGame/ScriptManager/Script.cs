using Game1.DataContext;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Game1.Scripting
{
    public enum ScriptState { Ready, Running, Done, Error, Cancelled };

    public abstract class Script
    {
        public ScriptState State { get; set; }

        public GameContext dataContext;

        public async void Execute()
        {
            try
            {
                //var sw = new Stopwatch();
                //sw.Start();
                await RunScript();
                //sw.Stop();
                //Console.WriteLine(sw.ElapsedMilliseconds);
                State = ScriptState.Done;
            }
            catch (OperationCanceledException)
            {
                State = ScriptState.Cancelled;
            }
            catch (Exception)
            {
                State = ScriptState.Error;
            }
        }
        protected abstract Task RunScript();

        public Script() { }
        public Script(GameContext context)
        {
            dataContext = context;
            State = ScriptState.Ready;
        }
    }
}