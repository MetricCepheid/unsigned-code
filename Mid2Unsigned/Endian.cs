namespace System
{
    public abstract class Endian
    {
        public static int Invert(int a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToInt32(arr, 0);
        }

        public static uint Invert(uint a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToUInt32(arr, 0);
        }

        public static short Invert(short a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToInt16(arr, 0);
        }

        public static ushort Invert(ushort a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToUInt16(arr, 0);
        }

        public static long Invert(long a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToInt64(arr, 0);
        }

        public static ulong Invert(ulong a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToUInt64(arr, 0);
        }

        public static float Invert(float a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToSingle(arr, 0);
        }

        public static double Invert(double a)
        {
            byte[] arr = System.BitConverter.GetBytes(a);
            System.Array.Reverse(arr);
            return System.BitConverter.ToDouble(arr, 0);
        }
    }
}