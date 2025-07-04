using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NFSADsRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            List<byte> searchBytes = new List<byte>();
            int count = args[0].Length;
            for (int i = 0; i + 1 < count; i += 2)
            {
                searchBytes.Add(Convert.ToByte(args[0].Substring(i, 2), 16));
            }
            int searchSize = searchBytes.Count;
            foreach (string line in Directory.GetFileSystemEntries(Path.Combine(path, "TRACKS"), "*.bun"))
            {
                if (Path.GetFileName(line).StartsWith("STREAM", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytesFile = File.ReadAllBytes(line);
                    int fileSize = bytesFile.Length;
                    bool find = true;
                    for (int i = 0; i + searchSize < fileSize; i++)
                    {
                        find = true;
                        for (int j = 0; j < searchSize; j++)
                        {
                            if (bytesFile[i + j] != searchBytes[j])
                            {
                                find = false;
                                break;
                            }
                        }
                        if (find)
                        {
                            int length = bytesFile[i - 4];
                            if (length == 0 && args.Length > 2 && args[2] == "*")
                            {
                                for (int j = 4; j < 100; j++)
                                {
                                    if (bytesFile[i + j] == 0 && bytesFile[i + j + 1] == 65 && bytesFile[i + j + 2] == 19 && bytesFile[i + j + 3] == 128)
                                    {
                                        length = j;
                                        break;
                                    }
                                }
                            }
                            if (length > 0 && bytesFile[i + length] == 0 && bytesFile[i + length + 1] == 65 && bytesFile[i + length + 2] == 19 && bytesFile[i + length + 3] == 128)
                            {
                                for (int j = 0; j < length; j++)
                                {
                                    bytesFile[i + j] = 0;
                                }
                                File.WriteAllBytes(line, bytesFile);
                                Console.WriteLine("Replace: " + args[1] + " in: " + line);
                            }
                        }
                    }
                    bytesFile = null;
                }
            }
        }
    }
}
