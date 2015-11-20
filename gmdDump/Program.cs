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
            string input = args[0];
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".txt";
            bool BigEndian = false;
            long input_size = new FileInfo(input).Length;
            BinaryReader reader = new BinaryReader(File.OpenRead(input));

            // Handle input / output files
            int header = reader.ReadInt32();
            int version = reader.ReadInt32();

            if (header == 0x00444D47)
            {
                BigEndian = false;
            }
            else if (header == 0x474D4400) 
            {
                BigEndian = true;
            }
            else
            {
                Console.WriteLine("ERROR: Invalid input file specified, aborting.");
                return;
            }

            if (File.Exists(output))
                File.Delete(output);

            // Process input file
            UInt32 string_count = 0;

            if (BigEndian == true)
            {
                reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
                string_count = reader.ReadUInt32();
                reader.BaseStream.Seek(0x04, SeekOrigin.Current);
                UInt32 table_size = reader.ReadUInt32();

                string_count = Helper.swapEndianness(string_count);
                table_size = Helper.swapEndianness(table_size);

                UInt32 table_start = Convert.ToUInt32(input_size) - table_size;
                reader.BaseStream.Seek(table_start, SeekOrigin.Begin);
            }
            else
            {
                int s_count_offset = 0;
                int t_size_offset = 0;

                // Handle version offset differences
                if (version == 0x00010201 || version == 0x00010101) // MH3U EU, MH3G JP
                {
                    s_count_offset = 0x10;
                    t_size_offset = 0x18;
                }
                else if (version == 0x00010302) // MHX JP
                {
                    s_count_offset = 0x18;
                    t_size_offset = 0x20; 
                }
                else
                {
                    Console.WriteLine("ERROR: Unsupported GM version, aborting.");
                    return;
                }

                reader.BaseStream.Seek(s_count_offset, SeekOrigin.Begin);
                string_count = reader.ReadUInt32();
                reader.BaseStream.Seek(t_size_offset, SeekOrigin.Begin);
                UInt32 table_size = reader.ReadUInt32();

                UInt32 table_start = Convert.ToUInt32(input_size) - table_size;
                reader.BaseStream.Seek(table_start, SeekOrigin.Begin);
            }

            // Process strings in string table
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
