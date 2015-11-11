using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gmdDump
{
    class Program
    {
        static void Main(string[] args)
        {
            //  3DS GMD; UTF8 encoding
            // 0000|4|Magic gmd/x00
            // 0004|4|Unknown
            // 0008|4|Language flag (00 = JPN, 01 = ENG, 02 = FRE, 03 = SPA, 04 = GER, 05 = ITA)
            // 000C|4|Segment title count
            // 0010|4|String count
            // 0014|4|?? count
            // 0018|4|String table size
            // 001C|4|Name length x
            // 0020|x|Name string

            string input = args[0];
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".txt";

            // Read File
            FileStream _MyStream = new FileStream(input, FileMode.Open);
            byte[] _Buffer = new byte[_MyStream.Length];
            _MyStream.Read(_Buffer, 0, _Buffer.Length);
            _MyStream.Close();

            // Resize Buffer
            int Size = (_Buffer[0x18] | (_Buffer[0x19] << 8) | (_Buffer[0x1A] << 16) | (_Buffer[0x1B] << 32));
            _Buffer = _Buffer.Skip(_Buffer.Length - Size).ToArray();

            // Decoding
            string OutputStr = "# " + Encoding.UTF8.GetString(_Buffer, 0, _Buffer.Length);
            OutputStr = OutputStr.Replace("\0", "\r\n# ");
            OutputStr = OutputStr.Substring(0, OutputStr.Length - 2);
            
            using (StreamWriter writer = new StreamWriter(output, true, Encoding.UTF8))
            {
                writer.Write(OutputStr);
            }

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }
    }
}
