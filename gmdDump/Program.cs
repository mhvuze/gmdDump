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
            string output = Path.GetFileNameWithoutExtension(input) + ".txt"; // Path.GetDirectoryName(input) + "\\" + 
            bool BigEndian = false;
            long input_size = new FileInfo(input).Length;
            BinaryReader reader = new BinaryReader(File.OpenRead(input));

            // Handle input / output files
            int header = reader.ReadInt32();
            int version = reader.ReadInt32();

            if (header == 0x00444D47) { BigEndian = false; Console.WriteLine("File is LE."); }
            else if (header == 0x474D4400)  { BigEndian = true; Console.WriteLine("File is BE."); }
            else { Console.WriteLine("ERROR: Invalid input file specified, aborting."); return; }

            if (File.Exists(output))
                File.Delete(output);

            // Process input file
            UInt32 identifier_count = 0;
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
                int identifier_count_offset = 0;
                int identifier_size_offset = 0;
                int s_count_offset = 0;
                int t_size_offset = 0;

                // Handle version offset differences
                if (version == 0x00010201 || version == 0x00010101) // MH3U EU, MH3G JP
                {
                    s_count_offset = 0x10;
                    t_size_offset = 0x18;
                        
                }
                else if (version == 0x00010302) // MHX JP, MHW
                {
                    identifier_count_offset = 0x14;
                    identifier_size_offset = 0x1C;
                    s_count_offset = 0x18;
                    t_size_offset = 0x20; 
                }
                else
                {
                    Console.WriteLine("ERROR: Unsupported GMD version, aborting.");
                    return;
                }

                reader.BaseStream.Seek(identifier_count_offset, SeekOrigin.Begin);
                identifier_count = reader.ReadUInt32();

                reader.BaseStream.Seek(identifier_size_offset, SeekOrigin.Begin);
                UInt32 i_table_size = reader.ReadUInt32();

                reader.BaseStream.Seek(s_count_offset, SeekOrigin.Begin);
                string_count = reader.ReadUInt32();

                reader.BaseStream.Seek(t_size_offset, SeekOrigin.Begin);
                UInt32 table_size = reader.ReadUInt32();

                // Whacky handling of DD:DA PC gmd, since it also has version 0x00010201 but other offsets
                if (string_count == 0)
                {
                    reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
                    string_count = reader.ReadUInt32();

                    reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
                    table_size = reader.ReadUInt32();
                }

                UInt32 table_start = Convert.ToUInt32(input_size) - table_size - i_table_size;
                reader.BaseStream.Seek(table_start, SeekOrigin.Begin);
            }

            if (BigEndian == true)
            {
                // Process strings in string table
                for (int i = 0; i < string_count; i++)
                {
                    string str = Helper.readNullterminated(reader).Replace("\r\n", "<LINE>");
                    using (StreamWriter writer = new StreamWriter(output, true, Encoding.UTF8))
                    {
                        writer.WriteLine(str);
                    }
                }
            }
            else
            {
                // Process strings in identifier table
                for (int i = 0; i < identifier_count; i++)
                {
                    string str = Helper.readNullterminated(reader).Replace("\r\n", "<LINE>");
                    using (StreamWriter writer = new StreamWriter(Path.GetFileNameWithoutExtension(input) + ".tmp", true, Encoding.UTF8))
                    {
                        writer.WriteLine(str + "_____");
                    }
                }

                // Process strings in string table
                for (int i = 0; i < string_count; i++)
                {
                    string line = "";
                    string str = Helper.readNullterminated(reader).Replace("\r\n", "<LINE>");
                    using (StreamReader Oreader = new StreamReader(Path.GetFileNameWithoutExtension(input) + ".tmp", Encoding.UTF8))
                    {
                        for (var j = 0; j < i; j++) { Oreader.ReadLine(); }
                        line = Oreader.ReadLine() + str;
                    }
                    using (StreamWriter writer = new StreamWriter(output, true, Encoding.UTF8))
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            if (BigEndian == false) { File.Delete(Path.GetFileNameWithoutExtension(input) + ".tmp"); }
            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }
    }
}
