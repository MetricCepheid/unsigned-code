using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Unsigned
{
    public class XFireClient
    {
        [DllImport("XFireSDK.Dll")]
        private static extern int XfireIsLoaded();
        [DllImport("XFireSDK.Dll")]
        private static extern int XfireSetCustomGameDataA(int num_keys, String[] keys, String[] values);
        [DllImport("XFireSDK.Dll")]
        private static extern int XfireSetCustomGameDataW(int num_keys, String[] keys, String[] values);
        [DllImport("XFireSDK.Dll")]
        private static extern int XfireSetCustomGameDataUTF8(int num_keys, String[] keys, String[] values);

        public static void SetCustomGameData(int numKeys, String[] keys, String[] values)
        {
            if (XfireIsLoaded() != 0)
            {
                XfireSetCustomGameDataUTF8(numKeys, keys, values);
            }
        }
    }
}
