#if XBOX
using System;
using System.Collections.Generic;
using System.Text;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    public class PeripheralManager
    {
        private static PeripheralManager _singleton = null;
        public static PeripheralManager Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new PeripheralManager();
                return _singleton;
            }
        }

        private List<Type> peripheralTypes;

        private Peripheral[] connections;
        private Peripheral[] ownedPeripherals;

        private PeripheralManager()
        {
            peripheralTypes = new List<Type>();
            ownedPeripherals = new Peripheral[4];
            connections = new Peripheral[0];
        }

        public void ReloadDLLs()
        {
        }

        public Peripheral GetPeripheral(int index)
        {
            return ownedPeripherals[index];
        }

        public void ConfirmOwnership(int index, Peripheral p)
        {
            ownedPeripherals[index] = p;
        }

        public void RelinquishOwnership(int index)
        {
            ownedPeripherals[index] = null;
        }

        public void RelinquishOwnership(Peripheral p)
        {
            for (int i = 0; i < 4; i++)
                if (ownedPeripherals[i] == p)
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
    }
}
#endif