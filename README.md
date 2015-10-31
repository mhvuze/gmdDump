# gmdDump
Simple C# tool for dumping nicely formatted strings from Monster Hunter 3 Ultimate gmd files.

Just drag and drop a file onto the executable or use gmdDump.exe file.gmd.

Only tested with MH3U/MH3G gmd but it might work with other MT Framework gmd files as well as long as the string count is stored at 0x10 (LE) / 0x18 (BE) and the table size at 0x18 / 0x20.