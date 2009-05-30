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
            using (AnimationEditorGame game = new AnimationEditorGame())
            {
                game.Run();
            }
        }
    }
}

