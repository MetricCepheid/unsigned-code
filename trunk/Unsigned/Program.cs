using System;

namespace Unsigned
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
#if !DEBUG
                try
                {
#endif
                    game.Run();
#if !DEBUG
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Problem: " + e.Message + "\n" + e.StackTrace);
                }
#endif
            }
        }
    }
}

