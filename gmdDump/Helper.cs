using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gmdDump
{
    class Helper
    {
        public static string readNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }

        // from http://stackoverflow.com/a/3294698/5343630
        public static uint swapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) +  // First byte
                   ((x & 0x0000ff00) << 8) +   // Second byte
                   ((x & 0x00ff0000) >> 8) +   // Third byte
                   ((x & 0xff000000) >> 24);   // Fourth byte
        }
    }
}
