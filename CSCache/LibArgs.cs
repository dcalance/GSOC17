using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
using System.IO;

namespace CSCacheLib
{
    class LibArgs
    {
        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: CSCache [COMPILE COMMAND] [OPTIONS]");
            Console.WriteLine("Example : CSCache \"mcs input1.cs -r:lib1.dll\" -clear=all");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
        public static void ClearCache(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles().Where(x => x.Extension == ".cache").OrderByDescending(p => p.CreationTime).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                files[i].Delete();
            }
            Console.WriteLine("Cache cleared successfully.");
        }
    }
}
