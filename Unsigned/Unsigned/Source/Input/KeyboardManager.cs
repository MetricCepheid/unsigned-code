using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class KeyboardPeripheral : Peripheral
    {
        private struct KeyMap
        {
            public PeripheralButton button;
            public Keys key;

            public KeyMap(PeripheralButton button, Keys key)
            {
                this.button = button;
                this.key = key;
            }
        }

        private static Dictionary<String,List<KeyMap>>[] keymaps;
        // dont ask
        private static Dictionary<String,Dictionary<PeripheralButton, List<Keys>>>[] keysFromButtons;

        private const PeripheralButton Whammy = (PeripheralButton)(PeripheralButton.TOTAL+1);

        private bool[] bufferedMap;
        private KeyboardState previousState;
        private int playerIndex;

        /// <summary>
        /// Makes an INVALID keyboard object
        /// </summary>
        public KeyboardPeripheral()
        {
            playerIndex = -1;
        }

        public KeyboardPeripheral(int pIndex)
        {
            playerIndex = pIndex;
            bufferedMap = new bool[(int)PeripheralButton.TOTAL*2];
        }

        public static void LoadMapping(String xmlFilename)
        {
            XmlTextReader xin = new XmlTextReader(xmlFilename);

            List<Dictionary<String, List<KeyMap>>> list = null;
            Dictionary<String, List<KeyMap>> tempDict = null;
            List<KeyMap> tempKeys = null;
            String tempMapType = null;
            int playerIndex = -1;
            while (xin.Read())
            {
                switch (xin.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            if (xin.Name.ToLower().Equals("keymapping"))
                            {
                                list = new List<Dictionary<string, List<KeyMap>>>();
                            }
                            else if (xin.Name.ToLower().Equals("player"))
                            {
                                tempDict = new Dictionary<string, List<KeyMap>>();
                                xin.MoveToFirstAttribute();
                                playerIndex = Int32.Parse(xin.Value);
                            }
                            else if (xin.Name.ToLower().Equals("map"))
                            {
                                tempKeys = new List<KeyMap>();
                                xin.MoveToFirstAttribute();
                                tempMapType = xin.Value;
                            }
                            else if (xin.Name.ToLower().Equals("key"))
                            {
                                PeripheralButton button = PeripheralButton.NONE;
                                Keys key = (Keys)(-1);
                                while (xin.MoveToNextAttribute())
                                {
                                    if (xin.Name.ToLower().Equals("name"))
                                    {
                                        button = GetButtonFromString(xin.Value);
                                        if (button == PeripheralButton.NONE)
                                        {
                                            if (xin.Value.ToLower().Equals("whammy"))
                                                button = Whammy;
                                        }
                                    }
                                    else if (xin.Name.ToLower().Equals("button"))
                                        key = GetKeyFromString(xin.Value);
                                }
                                tempKeys.Add(new KeyMap(button,key));
                            }
                            break;
                        }
                    case XmlNodeType.EndElement:
                        {
                            if (xin.Name.ToLower().Equals("keymapping"))
                            {
                                keymaps = list.ToArray();
                            }
                            else if (xin.Name.ToLower().Equals("player"))
                            {
                                list.Add(tempDict);
                                tempDict = null;
                            }
                            else if (xin.Name.ToLower().Equals("map"))
                            {
                                tempDict.Add(tempMapType, tempKeys);
                                tempKeys = null;
                            }
                            break;
                        }
                }
            }

            xin.Close();

            // generate keysFromButtons
            keysFromButtons = new Dictionary<String,Dictionary<PeripheralButton,List<Keys>>>[keymaps.Length];
            for(int i=0;i<keymaps.Length;i++)
            {
                keysFromButtons[i] = new Dictionary<String,Dictionary<PeripheralButton,List<Keys>>>();
                
                foreach(String str in keymaps[i].Keys)
                {
                    Dictionary<PeripheralButton,List<Keys>> tdict = new Dictionary<PeripheralButton,List<Keys>>();
                    List<KeyMap> ls = keymaps[i][str];
                    
                    for(int k=0;k<ls.Count;k++)
                    {
                        if(tdict.ContainsKey(ls[k].button))
                        {
                            tdict[ls[k].button].Add(ls[k].key);
                        }
                        else
                        {
                            List<Keys> l = new List<Keys>();
                            l.Add(ls[k].key);
                            tdict.Add(ls[k].button,l);
                        }
                    }
                    keysFromButtons[i].Add(str,tdict);
                }
            }
        }

        public override void Query()
        {
            KeyboardState currentState = Keyboard.GetState();

            for (int i = 0; i < (int)PeripheralButton.TOTAL; i++)
                bufferedMap[i] = false;
            for(int i=0;i<keymaps[playerIndex][mode].Count;i++)
            {
                if (currentState.IsKeyDown(keymaps[playerIndex][mode][i].key) && previousState.IsKeyUp(keymaps[playerIndex][mode][i].key))
                {
                    bufferedMap[(int)keymaps[playerIndex][mode][i].button] = true;
                }
            }

            previousState = currentState;
        }

        public override bool IsConnected()
        {
            return true;
        }

        public override bool WasPressed(PeripheralButton button)
        {
            return bufferedMap[(int)button];
        }

        public override bool IsPressed(PeripheralButton peripheralButton)
        {
            if (playerIndex >= keysFromButtons.Length)
                return false;
            if (!keysFromButtons[playerIndex].ContainsKey(mode))
                return false;
            if (!keysFromButtons[playerIndex][mode].ContainsKey(peripheralButton))
                return false;
            for (int i = 0; i < keysFromButtons[playerIndex][mode][peripheralButton].Count; i++)
                if (previousState.IsKeyDown(keysFromButtons[playerIndex][mode][peripheralButton][i]))
                    return true;
            return false;
        }

        public override Peripheral[] GetControllers()
        {
            KeyboardPeripheral[] ret = new KeyboardPeripheral[keymaps.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = new KeyboardPeripheral(i);
            return ret;
        }

        public override ulong GetFrets()
        {
            ulong ret = 0;
            if (IsPressed(PeripheralButton.FRET0))
                ret |= (((ulong)1) << 0);
            if (IsPressed(PeripheralButton.FRET1))
                ret |= (((ulong)1) << 1);
            if (IsPressed(PeripheralButton.FRET2))
                ret |= (((ulong)1) << 2);
            if (IsPressed(PeripheralButton.FRET3))
                ret |= (((ulong)1) << 3);
            if (IsPressed(PeripheralButton.FRET4))
                ret |= (((ulong)1) << 4);
            return ret;
        }

        public override ulong GetBufferedFrets()
        {
            ulong ret = 0;
            if (WasPressed(PeripheralButton.FRET0))
                ret |= (((ulong)1) << 0);
            if (WasPressed(PeripheralButton.FRET1))
                ret |= (((ulong)1) << 1);
            if (WasPressed(PeripheralButton.FRET2))
                ret |= (((ulong)1) << 2);
            if (WasPressed(PeripheralButton.FRET3))
                ret |= (((ulong)1) << 3);
            if (WasPressed(PeripheralButton.FRET4))
                ret |= (((ulong)1) << 4);
            return ret;
        }

        public static Keys GetKeyFromString(String keyStr)
        {
            for (Keys i = (Keys)0; i < Keys.OemClear; i++)
                if (i.ToString().ToLower().Equals(keyStr.ToLower()))
                    return i;
            return (Keys)(-1);
        }

        public override float GetAnalogValue(PeripheralAnalog analogControl)
        {
            switch (analogControl)
            {
                case PeripheralAnalog.WHAMMY_BAR:
                    for (int i = 0; i < keymaps[playerIndex][mode].Count; i++)
                        if (keymaps[playerIndex][mode][i].button == Whammy)
                            if (previousState.IsKeyDown(keymaps[playerIndex][mode][i].key))
                                return 1.0f;
                    return 0.0f;
            }
            return 0.0f;
        }

        private static String[] suppInstr = { "LGT", "RGT", "BAS", "SET", "LVX", "BVX", "BVA", "BVB" };
        public override String[] GetSupportedInstruments()
        {
            return suppInstr;
        }
    }
}
