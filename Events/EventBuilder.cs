using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using SpotifyLib.Enums;

namespace SpotifyLib.Events
{
    public class EventBuilder
    {
        private readonly MemoryStream body = new MemoryStream(256);

        public EventBuilder([NotNull] EventType type)
        {
            AppendNoDelimiter(type.Id.ToString());
            Append(type.Unknown.ToString());
        }

        public EventBuilder Append([CanBeNull] string str)
        {
            body.WriteByte(0x09);
            AppendNoDelimiter(str);
            return this;
        }

        public EventBuilder Append(char c)
        {
            var j = (byte)c;
            body.WriteByte(0x09);
            body.WriteByte(j);
            return this;
        }

        private void AppendNoDelimiter([CanBeNull] string str)
        {
            if (str == null) str = "";
            var bytesToWrite = Encoding.UTF8.GetBytes(str);
            body.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        public static string ToString([NotNull] byte[] body)
        {
            var result = new StringBuilder();
            foreach (var b in body)
                if (b == 0x09) result.Append('|');
                else result.Append((char)b);

            return result.ToString();
        }

        public byte[] ToArray()
        {
            return body.ToArray();
        }
    }
}