using System;

namespace Game1
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
                using (var game = new DemoGame(args[0]))
                    game.Run();
            else
                using (var game = new DemoGame())
                game.Run();
        }
    }
}
