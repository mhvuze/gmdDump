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
            //  UTF8 encoding
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
            long input_size = new FileInfo(input).Length;
            BinaryReader reader = new BinaryReader(File.OpenRead(input));
            

            // Handle input / output files
            if (Encoding.ASCII.GetString(reader.ReadBytes(3)) != "GMD")
            {
                Console.WriteLine("ERROR: Invalid input file specified, aborting.");
                return;
            }

            if (File.Exists(output))
                File.Delete(output);

            // Process input file
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            UInt32 string_count = reader.ReadUInt32();
            reader.BaseStream.Seek(0x04, SeekOrigin.Current);
            UInt32 table_size = reader.ReadUInt32();
            UInt32 table_start = Convert.ToUInt32(input_size) - table_size;
            reader.BaseStream.Seek(table_start, SeekOrigin.Begin);

            for (int i = 0; i < string_count; i++)
            {
                string str = Helper.readNullterminated(reader).Replace("\r\n", "<LINE>");
                using(StreamWriter writer = new StreamWriter(output, true, Encoding.UTF8))
                {
                    writer.WriteLine(str);
                }
            }

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }
    }
}
