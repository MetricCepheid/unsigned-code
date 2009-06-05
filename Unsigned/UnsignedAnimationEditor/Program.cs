using System;

namespace UnsignedAnimationEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Form1 form = new Form1();
            form.Show();
            Game1 game = new Game1(form.GetDrawSurface());
            form.Game = game;
            game.Run();
        }
    }
}

