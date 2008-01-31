using System;

namespace VenueCompiler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Compiler game = new Compiler())
            {
                game.Run();
            }
        }
    }
}

