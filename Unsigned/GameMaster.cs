using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    class GameMaster
    {
        private static GameMaster SINGLETON_GameMaster = null;

        private bool IsPaused = false;
        private int pausetimer;

        byte[] failStatus;
        public static byte FS_GOOD = 0, FS_FAILING = 1, FS_DNE = 2;
        float failTime;

        SpecialEffectsSettings currentSettings;

        private GameMaster()
        {
            
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_GameMaster == null)
                SINGLETON_GameMaster = new GameMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_GameMaster != null)
                SINGLETON_GameMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static GameMaster GetSingleton()
        {
            return SINGLETON_GameMaster;
        }
    }
}
