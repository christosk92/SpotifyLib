using System;
using System.IO;

namespace SpotifyLib.Helpers
{
    public static class StreamExtensions
    {
        public static void ReadComplete(this Stream stream, byte[] buffer, int offset, int count)
        {
            int num = 0;
            while (num < count)
                num += stream.Read(buffer, offset + num, count - num);
        }
        public static string ToBase64String(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        private static readonly int MAX_SKIP_BUFFER_SIZE = 2048;



        public static int ix(int obj0, int offset) => obj0 + offset;
        public static int SkipBytes(this MemoryStream input, int n)
        {
            int total = 0;
            int cur = 0;

            while ((total < n) && ((cur = (int)input.Skip(n - total)) > 0))
            {
                total += cur;
            }

            return total;
        }

        public static long Skip(this Stream input, long n)
        {
            long remaining = n;
            int nr;

            if (n <= 0)
            {
                return 0;
            }

            int size = (int)Math.Min(MAX_SKIP_BUFFER_SIZE, remaining);
            byte[] skipBuffer = new byte[size];
            while (remaining > 0)
            {
                nr = input.Read(skipBuffer, 0, (int)Math.Min(size, remaining));
                if (nr < 0)
                {
                    break;
                }

                remaining -= nr;
            }
            return n - remaining;
        }

        public static void Write(this MemoryStream input,
            string text)
        {
            var b = System.Text.Encoding.Default.GetBytes(text);
            input.Write(b, 0, b.Length);
        }


        public static void WriteShort(this Stream input, int v)
        {
            //src1 >>> src2;
            //(int)((uint)x >> 2);
            var x = new[]
            {
                (byte)((int) ((uint) v >> 8) & 0xFF)
            };
            var y = new[]
            {
                (byte) ((int) ((uint) v >> 0) & 0xFF)
            };
            input.Write(x, 0, x.Length);
            input.Write(y, 0, y.Length);
        }

        public static void WriteInt(this Stream input, int i)
        {
            var v = new[]
            {
                (byte)((int) ((uint) i >> 24) & (int)byte.MaxValue)
            };
            var w = new[]
            {
                (byte) ((int) ((uint) i >> 16) & (int)byte.MaxValue)
            };
            var x = new[]
            {
                (byte)((int) ((uint) i >> 8) & (int)byte.MaxValue)
            };
            var y = new[]
            {
                (byte) ((int) ((uint) i >> 0) & (int)byte.MaxValue)
            };
            input.Write(v, 0, x.Length);
            input.Write(w, 0, y.Length);
            input.Write(x, 0, x.Length);
            input.Write(y, 0, y.Length);
        }

        public static int GetInt(this MemoryStream input) => getInt(input.ToArray(), (int)input.Position, true);
        private static int getInt(byte[] obj0, int obj1, bool obj2) =>
            !obj2 ? getIntL(obj0, obj1) : getIntB(obj0, obj1);

        private static int getIntB(byte[] obj0, int obj1) =>
            makeInt(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3]);

        private static int getIntL(byte[] obj0, int obj1) =>
            makeInt(obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

        private static int makeInt(byte obj0, byte obj1, byte obj2, byte obj3) => (int)(sbyte)obj0 << 24 |
            ((int)(sbyte)obj1 & (int)byte.MaxValue) << 16 | ((int)(sbyte)obj2 & (int)byte.MaxValue) << 8 |
            (int)(sbyte)obj3 & (int)byte.MaxValue;


        public static int GetShort(this MemoryStream input) => getShort(input.ToArray(), (int)input.Position, true);
        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)getShortL(obj0, obj1) : (int)getShortB(obj0, obj1));
        }

        private static short getShortB(byte[] obj0, int obj1) => makeShort(obj0[obj1], obj0[obj1 + 1]);

        private static short getShortL(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1 + 1], obj0[obj1]);
        }

        private static short makeShort(byte obj0, byte obj1) =>
            (short)((int)(sbyte)obj0 << 8 | (int)(sbyte)obj1 & (int)byte.MaxValue);


    }
}