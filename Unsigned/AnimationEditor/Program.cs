using System;

namespace AnimationEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Form1 form = new Form1();
            form.Show();
            AnimationEditorGame game = new AnimationEditorGame(form.GetDrawSurface());
            game.Run(); 
        }
    }
}

