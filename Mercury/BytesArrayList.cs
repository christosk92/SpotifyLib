using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Org.BouncyCastle.Utilities;
using SpotifyLib.Helpers;

namespace SpotifyLib.Mercury
{
    internal class InternalStream : MemoryStream
    {
        private int offset = 0;
        private int sub = 0;
        private readonly List<byte[]> _elementData;

        public InternalStream(List<byte[]> elementData)
        {
            this._elementData = elementData;
        }


        public override int Read(byte[] buffer,
            int offset,
            int count)
        {
            if (offset < 0 || count < 0 || count > buffer.Length - offset)
            {
                throw new IndexOutOfRangeException();
            }
            else if (count == 0)
            {
                return 0;
            }

            if (sub >= _elementData.Count)
                return -1;

            int i = 0;
            while (true)
            {
                int copy = Math.Min(count - i, _elementData[sub].Length - offset);
                Array.Copy(_elementData[sub], offset,
                    buffer,
                    offset + i,
                    copy);
                i += copy;
                offset += copy;

                if (i == count)
                    return i;

                if (offset >= _elementData[sub].Length)
                {
                    offset = 0;
                    if (++sub >= _elementData.Count)
                        return i == 0 ? -1 : i;
                }
            }
        }


        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
    }


    [ImmutableObject(true)]
    public class BytesArrayList : IEnumerable<byte[]>
    {
        private List<byte[]> _elementData;
        private int _size => _elementData.Count;

        public BytesArrayList()
        {
            _elementData = new List<byte[]>();
        }

        public string ReadIntoString(int index) => Encoding.Default.GetString(_elementData[0]);
        public BytesArrayList CopyOfRange(int from, int to)
            => new BytesArrayList(
                _elementData.Skip(from)
                    .Take(to - from)
                    .ToArray());
        public void Add(byte[] e)
        {
            _elementData.Add(e);
        }
        private BytesArrayList(byte[][] buffer)
        {
            _elementData = buffer.ToList();
        }
        public Stream Stream([NotNull] string[] payloads)
        {
            byte[][] bytes = new byte[payloads.Length][];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = System.Text.Encoding.Default.GetBytes(payloads[i]);
            return new BytesArrayList(bytes).Stream();
        }
        public String ToHex()
        {
            String[] array = new String[_size];
            byte[][] copy = _elementData.ToArray();
            for (int i = 0; i < copy.Length; i++) array[i] = Utils.bytesToHex(copy[i]);
            return Arrays.ToString(array);
        }
        public Stream Stream() => new InternalStream(_elementData);
        public IEnumerator<byte[]> GetEnumerator() => _elementData.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
