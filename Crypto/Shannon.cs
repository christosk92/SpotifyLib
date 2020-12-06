using System.Runtime.InteropServices;

namespace SpotifyLib.Crypto
{
    internal static class Integer
    {

        public static int RotateLeft(int i, int distance) => i << distance | (int)((uint)i >> -distance);

        public static int RotateRight(int i, int distance) => (int)((uint)i >> distance) | i << -distance;
    }

    public class Shannon : object
    {
        private const int N = 16;
        private const int FOLD = 16;
        private const int INITKONST = 1771488570;
        private const int KEYP = 13;
        private int[] R;
        private int[] CRC;
        private int[] initR;
        private int konst;
        private int sbuf;
        private int mbuf;
        private int nbuf;

        public Shannon()
        {
            Shannon shannon = this;
            this.R = new int[16];
            this.CRC = new int[16];
            this.initR = new int[16];
        }

        public virtual void key(byte[] key)
        {
            this.initState();
            this.loadKey(key);
            this.genKonst();
            this.saveState();
            this.nbuf = 0;
        }

        public virtual void nonce(byte[] nonce)
        {
            this.reloadState();
            this.konst = 1771488570;
            this.loadKey(nonce);
            this.genKonst();
            this.nbuf = 0;
        }

        public virtual void encrypt(byte[] buffer) => this.encrypt(buffer, buffer.Length);

        public virtual void finish(byte[] buffer) => this.finish(buffer, buffer.Length);

        public virtual void decrypt(byte[] buffer) => this.decrypt(buffer, buffer.Length);

        private int sbox([In] int obj0)
        {
            obj0 ^= Integer.RotateLeft(obj0, 5) | Integer.RotateLeft(obj0, 7);
            obj0 ^= Integer.RotateLeft(obj0, 19) | Integer.RotateLeft(obj0, 22);
            return obj0;
        }


        private int sbox2([In] int obj0)
        {
            obj0 ^= Integer.RotateLeft(obj0, 7) | Integer.RotateLeft(obj0, 22);
            obj0 ^= Integer.RotateLeft(obj0, 5) | Integer.RotateLeft(obj0, 19);
            return obj0;
        }
   private void crcFunc([In] int obj0)
        {
            int num = this.CRC[0] ^ this.CRC[2] ^ this.CRC[15] ^ obj0;
            for (int index = 1; index < 16; ++index)
                this.CRC[index - 1] = this.CRC[index];
            this.CRC[15] = num;
        }

         private void cycle()
        {
            int num1 = this.sbox(this.R[12] ^ this.R[13] ^ this.konst) ^ Integer.RotateLeft(this.R[0], 1);
            for (int index = 1; index < 16; ++index)
                this.R[index - 1] = this.R[index];
            this.R[15] = num1;
            int num2 = this.sbox2(this.R[2] ^ this.R[15]);
            int[] r = this.R;
            int index1 = 0;
            int[] numArray = r;
            numArray[index1] = numArray[index1] ^ num2;
            this.sbuf = num2 ^ this.R[8] ^ this.R[12];
        }

       private void addKey([In] int obj0)
        {
            int[] r = this.R;
            int index = 13;
            int[] numArray = r;
            numArray[index] = numArray[index] ^ obj0;
        }

        private void diffuse()
        {
            for (int index = 0; index < 16; ++index)
                this.cycle();
        }

      private void initState()
        {
            this.R[0] = 1;
            this.R[1] = 1;
            for (int index = 2; index < 16; ++index)
                this.R[index] = this.R[index - 1] + this.R[index - 2];
            this.konst = 1771488570;
        }

        private void loadKey([In] byte[] obj0)
        {
            byte[] numArray1 = new byte[4];
            int index1;
            for (index1 = 0; index1 < (obj0.Length & -4); index1 += 4)
            {
                this.addKey((int) obj0[index1 + 3] << 24 | (int) obj0[index1 + 2] << 16 | (int) obj0[index1 + 1] << 8 |
                            (int) obj0[index1]);
                this.cycle();
            }

            if (index1 < obj0.Length)
            {
                int index2 = 0;
                for (; index1 < obj0.Length; ++index1)
                {
                    byte[] numArray2 = numArray1;
                    int index3 = index2;
                    ++index2;
                    int num = (int) (sbyte) obj0[index1];
                    numArray2[index3] = (byte) num;
                }

                for (; index2 < 4; ++index2)
                    numArray1[index2] = (byte) 0;
                this.addKey((int) numArray1[3] << 24 | (int) numArray1[2] << 16 | (int) numArray1[1] << 8 |
                            (int) numArray1[0]);
                this.cycle();
            }

            this.addKey(obj0.Length);
            this.cycle();
            for (int index2 = 0; index2 < 16; ++index2)
                this.CRC[index2] = this.R[index2];
            this.diffuse();
            for (int index2 = 0; index2 < 16; ++index2)
            {
                int[] r = this.R;
                int index3 = index2;
                int[] numArray2 = r;
                numArray2[index3] = numArray2[index3] ^ this.CRC[index2];
            }
        }

        private void genKonst() => this.konst = this.R[0];

        private void saveState()
        {
            for (int index = 0; index < 16; ++index)
                this.initR[index] = this.R[index];
        }

        private void reloadState()
        {
            for (int index = 0; index < 16; ++index)
                this.R[index] = this.initR[index];
        }

        private void macFunc([In] int obj0)
        {
            this.crcFunc(obj0);
            int[] r = this.R;
            int index = 13;
            int[] numArray = r;
            numArray[index] = numArray[index] ^ obj0;
        }
        public virtual void encrypt(byte[] buffer, int n)
        {
            int index1 = 0;
            if (this.nbuf != 0)
            {
                for (; this.nbuf != 0 && n != 0; n += -1)
                {
                    this.mbuf ^= (int)buffer[index1] << 32 - this.nbuf;
                    byte[] numArray1 = buffer;
                    int index2 = index1;
                    byte[] numArray2 = numArray1;
                    numArray2[index2] = (byte)((int)(sbyte)numArray2[index2] ^ this.sbuf >> 32 - this.nbuf & (int)byte.MaxValue);
                    ++index1;
                    this.nbuf -= 8;
                }
                if (this.nbuf != 0)
                    return;
                this.macFunc(this.mbuf);
            }
            for (int index2 = n & -4; index1 < index2; index1 += 4)
            {
                this.cycle();
                int num1 = (int)buffer[index1 + 3] << 24 | (int)buffer[index1 + 2] << 16 | (int)buffer[index1 + 1] << 8 | (int)buffer[index1];
                this.macFunc(num1);
                int num2 = num1 ^ this.sbuf;
                buffer[index1 + 3] = (byte)(num2 >> 24 & (int)byte.MaxValue);
                buffer[index1 + 2] = (byte)(num2 >> 16 & (int)byte.MaxValue);
                buffer[index1 + 1] = (byte)(num2 >> 8 & (int)byte.MaxValue);
                buffer[index1] = (byte)(num2 & (int)byte.MaxValue);
            }
            n &= 3;
            if (n == 0)
                return;
            this.cycle();
            this.mbuf = 0;
            for (this.nbuf = 32; this.nbuf != 0 && n != 0; n += -1)
            {
                this.mbuf ^= (int)buffer[index1] << 32 - this.nbuf;
                byte[] numArray1 = buffer;
                int index2 = index1;
                byte[] numArray2 = numArray1;
                numArray2[index2] = (byte)((int)(sbyte)numArray2[index2] ^ this.sbuf >> 32 - this.nbuf & (int)byte.MaxValue);
                ++index1;
                this.nbuf -= 8;
            }
        }

        public virtual void decrypt(byte[] buffer, int n)
        {
            int index1 = 0;
            if (this.nbuf != 0)
            {
                for (; this.nbuf != 0 && n != 0; n += -1)
                {
                    byte[] numArray1 = buffer;
                    int index2 = index1;
                    byte[] numArray2 = numArray1;
                    numArray2[index2] = (byte)((int)(sbyte)numArray2[index2] ^ this.sbuf >> 32 - this.nbuf & (int)byte.MaxValue);
                    this.mbuf ^= (int)buffer[index1] << 32 - this.nbuf;
                    ++index1;
                    this.nbuf -= 8;
                }
                if (this.nbuf != 0)
                    return;
                this.macFunc(this.mbuf);
            }
            for (int index2 = n & -4; index1 < index2; index1 += 4)
            {
                this.cycle();
                int num = ((int)buffer[index1 + 3] << 24 | (int)buffer[index1 + 2] << 16 | (int)buffer[index1 + 1] << 8 | (int)buffer[index1]) ^ this.sbuf;
                this.macFunc(num);
                buffer[index1 + 3] = (byte)(num >> 24 & (int)byte.MaxValue);
                buffer[index1 + 2] = (byte)(num >> 16 & (int)byte.MaxValue);
                buffer[index1 + 1] = (byte)(num >> 8 & (int)byte.MaxValue);
                buffer[index1] = (byte)(num & (int)byte.MaxValue);
            }
            n &= 3;
            if (n == 0)
                return;
            this.cycle();
            this.mbuf = 0;
            for (this.nbuf = 32; this.nbuf != 0 && n != 0; n += -1)
            {
                byte[] numArray1 = buffer;
                int index2 = index1;
                byte[] numArray2 = numArray1;
                numArray2[index2] = (byte)((int)(sbyte)numArray2[index2] ^ this.sbuf >> 32 - this.nbuf & (int)byte.MaxValue);
                this.mbuf ^= (int)buffer[index1] << 32 - this.nbuf;
                ++index1;
                this.nbuf -= 8;
            }
        }

        public virtual void finish(byte[] buffer, int n)
        {
            int index1 = 0;
            if (this.nbuf != 0)
                this.macFunc(this.mbuf);
            this.cycle();
            this.addKey(1771488570 ^ this.nbuf << 3);
            this.nbuf = 0;
            for (int index2 = 0; index2 < 16; ++index2)
            {
                int[] r = this.R;
                int index3 = index2;
                int[] numArray = r;
                numArray[index3] = numArray[index3] ^ this.CRC[index2];
            }
            this.diffuse();
            while (n > 0)
            {
                this.cycle();
                if (n >= 4)
                {
                    buffer[index1 + 3] = (byte)(this.sbuf >> 24 & (int)byte.MaxValue);
                    buffer[index1 + 2] = (byte)(this.sbuf >> 16 & (int)byte.MaxValue);
                    buffer[index1 + 1] = (byte)(this.sbuf >> 8 & (int)byte.MaxValue);
                    buffer[index1] = (byte)(this.sbuf & (int)byte.MaxValue);
                    n += -4;
                    index1 += 4;
                }
                else
                {
                    for (int index2 = 0; index2 < n; ++index2)
                        buffer[index1 + index2] = (byte)(this.sbuf >> index1 * 8 & (int)byte.MaxValue);
                    break;
                }
            }
        }

        public virtual void stream(byte[] buffer)
        {
            int num1 = 0;
            int length;
            for (length = buffer.Length; this.nbuf != 0 && length != 0; length += -1)
            {
                byte[] numArray1 = buffer;
                int num2 = num1;
                ++num1;
                int index = num2;
                byte[] numArray2 = numArray1;
                numArray2[index] = (byte)((int)(sbyte)numArray2[index] ^ this.sbuf & (int)byte.MaxValue);
                this.sbuf >>= 8;
                this.nbuf -= 8;
            }
            for (int index1 = length & -4; num1 < index1; num1 += 4)
            {
                this.cycle();
                byte[] numArray1 = buffer;
                int index2 = num1 + 3;
                byte[] numArray2 = numArray1;
                numArray2[index2] = (byte)((int)(sbyte)numArray2[index2] ^ this.sbuf >> 24 & (int)byte.MaxValue);
                byte[] numArray3 = buffer;
                int index3 = num1 + 2;
                byte[] numArray4 = numArray3;
                numArray4[index3] = (byte)((int)(sbyte)numArray4[index3] ^ this.sbuf >> 16 & (int)byte.MaxValue);
                byte[] numArray5 = buffer;
                int index4 = num1 + 1;
                byte[] numArray6 = numArray5;
                numArray6[index4] = (byte)((int)(sbyte)numArray6[index4] ^ this.sbuf >> 8 & (int)byte.MaxValue);
                byte[] numArray7 = buffer;
                int index5 = num1;
                byte[] numArray8 = numArray7;
                numArray8[index5] = (byte)((int)(sbyte)numArray8[index5] ^ this.sbuf & (int)byte.MaxValue);
            }
            int num3 = length & 3;
            if (num3 == 0)
                return;
            this.cycle();
            for (this.nbuf = 32; this.nbuf != 0 && num3 != 0; num3 += -1)
            {
                byte[] numArray1 = buffer;
                int num2 = num1;
                ++num1;
                int index = num2;
                byte[] numArray2 = numArray1;
                numArray2[index] = (byte)((int)(sbyte)numArray2[index] ^ this.sbuf & (int)byte.MaxValue);
                this.sbuf >>= 8;
                this.nbuf -= 8;
            }
        }

        public virtual void macOnly(byte[] buffer)
        {
            int index1 = 0;
            int length = buffer.Length;
            if (this.nbuf != 0)
            {
                for (; this.nbuf != 0 && length != 0; length += -1)
                {
                    Shannon shannon = this;
                    int mbuf = shannon.mbuf;
                    byte[] numArray = buffer;
                    int index2 = index1;
                    ++index1;
                    int num = (int)(sbyte)numArray[index2] << 32 - this.nbuf;
                    shannon.mbuf = mbuf ^ num;
                    this.nbuf -= 8;
                }
                if (this.nbuf != 0)
                    return;
                this.macFunc(this.mbuf);
            }
            for (int index2 = length & -4; index1 < index2; index1 += 4)
            {
                this.cycle();
                this.macFunc((int)buffer[index1 + 3] << 24 | (int)buffer[index1 + 2] << 16 | (int)buffer[index1 + 1] << 8 | (int)buffer[index1]);
            }
            int num1 = length & 3;
            if (num1 == 0)
                return;
            this.cycle();
            this.mbuf = 0;
            for (this.nbuf = 32; this.nbuf != 0 && num1 != 0; num1 += -1)
            {
                Shannon shannon = this;
                int mbuf = shannon.mbuf;
                byte[] numArray = buffer;
                int index2 = index1;
                ++index1;
                int num2 = (int)(sbyte)numArray[index2] << 32 - this.nbuf;
                shannon.mbuf = mbuf ^ num2;
                this.nbuf -= 8;
            }
        }
    }
}
