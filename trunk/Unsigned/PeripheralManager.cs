using System;
using System.Collections.Generic;
using System.Text;
using UnsignedPeripheralPlugins;
#if WINDOWS
using System.Reflection;
#endif

namespace Unsigned
{
    public class PeripheralManager
    {
        private static PeripheralManager SINGLETON_PeripheralManager = null;

        private List<Type> peripheralTypes;

        private Assembly[] controllerAssemblies;

        private Peripheral[] connections;
        private Peripheral[] ownedPeripherals;

        private PeripheralManager()
        {
            peripheralTypes = new List<Type>();
            controllerAssemblies = new Assembly[1];
            controllerAssemblies[0] = Assembly.LoadFile(System.IO.Directory.GetCurrentDirectory()+"\\Xbox360Controller.dll");
            ownedPeripherals = new Peripheral[4];
            connections = new Peripheral[0];
        }

        public void ReloadDLLs()
        {
            peripheralTypes = new List<Type>();
            for(int i=0;i<controllerAssemblies.Length;i++)
                foreach (Type typ in controllerAssemblies[i].GetTypes())
                {
                    if (!peripheralTypes.Contains(typ))
                        peripheralTypes.Add(typ);
                }

        }

        public Peripheral GetPeripheral(int currentNoteIndex)
        {
            return ownedPeripherals[currentNoteIndex];
        }

        public void ConfirmOwnership(int currentNoteIndex, Peripheral p)
        {
            ownedPeripherals[currentNoteIndex] = p;
        }

        public void RelinquishOwnership(int currentNoteIndex)
        {
            ownedPeripherals[currentNoteIndex] = null;
        }

        public void RelinquishOwnership(Peripheral p)
        {
            for(int i=0;i<4;i++)
                if(ownedPeripherals[i]==p)
                    ownedPeripherals[i] = null;
        }

        public void CheckConnections()
        {
            List<Peripheral> list = new List<Peripheral>();
            for (int i = 0; i < peripheralTypes.Count; i++)
            {
                Peripheral[] arr = (Peripheral[])peripheralTypes[i].GetMethod("GetControllers").Invoke(Activator.CreateInstance(peripheralTypes[i]), null);
                for (int k = 0; k < arr.Length; k++)
                    list.Add(arr[k]);
            }

            {
                Peripheral[] arr = Activator.CreateInstance<KeyboardPeripheral>().GetControllers();//.Invoke(Activator.CreateInstance(peripheralTypes[i]), null);
                for (int k = 0; k < arr.Length; k++)
                    list.Add(arr[k]);
            }

            connections = list.ToArray();
        }

        public void QueryAll()
        {
            for (int i = 0; i < connections.Length; i++)
                connections[i].Query();
        }

        public Peripheral[] GetPeripherals()
        {
            return connections;
        }

        public void AddPeripheralType(Type peripheralType)
        {
            peripheralTypes.Add(peripheralType);
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_PeripheralManager == null)
                SINGLETON_PeripheralManager = new PeripheralManager();
            else
                throw new InvalidOperationException("PeripheralManager has already been instantiated");
        }

        public static PeripheralManager GetSingleton()
        {
            if (SINGLETON_PeripheralManager == null)
                throw new InvalidOperationException("Attempt to access PeripheralManager singleton before creation");
            return SINGLETON_PeripheralManager;
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_PeripheralManager != null)
                SINGLETON_PeripheralManager = null;
            else
                throw new InvalidOperationException("PeripheralManager has already been destroyed");
        }
    }
}
