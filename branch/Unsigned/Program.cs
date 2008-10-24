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
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName));
#if !XBOX
            using (UnsignedGame game = new UnsignedGame())
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
#endif
        }
    }
}

