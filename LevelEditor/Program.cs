using System;
using System.Windows.Forms;
using AuxLib.Logging;

namespace LevelEditor
{
#if WINDOWS || LINUX
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
            Logger.Instance.log("Application started.");

            try
            {
                Application.EnableVisualStyles();
                Application.Run(new MainForm());
                Logger.Instance.log("Creating Game1 object.");
            }
            catch (Exception e)
            {
                Logger.Instance.log("Exception caught: \n\n " + e.Message + "\n\n" + e.StackTrace);
                if (e.InnerException != null) Logger.Instance.log("Inner Exception: " + e.InnerException.Message);
                MessageBox.Show("An exception was caught. Application will end. Please check the file log.txt.");
            }
            finally
            {
                Logger.Instance.log("Application ended.");
            }


        }
    }
#endif
}
