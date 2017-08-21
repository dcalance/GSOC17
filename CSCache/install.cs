using System;
using System.IO;

namespace Install
{
    class Install
    {
        public static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 128) || (p == 6));
            }
        }

        static string path = (IsUnix) ? @"/usr/local/bin/cscache"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\CSCache\";

        static void Main(string[] args)
        {
			Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));		
            Console.WriteLine($"Default location : {path}");
            Console.WriteLine("Input the folder where you want to install the tool (enter to install in default location):");
            string input = Console.ReadLine();

            if (input != string.Empty)
            {
                path = input;
            }
            Directory.CreateDirectory(path);
            File.Copy("CSCacheLib.dll", path + "CSCacheLib.dll", true);
            File.Copy("CSCache.exe", path + "CSCache.exe", true);
            File.Copy("Options.dll", path + "Options.dll", true);
            if (IsUnix)
            {
                File.Copy("CSCache", path + "CSCache", true);
            }
            else
            {
                File.Copy("CSCache.bat", path + "CSCache.bat", true);
            }
            File.Delete("CSCacheLib.dll");
            File.Delete("CSCache.exe");
            File.Delete("Options.dll");

            Console.WriteLine("Adding folder to path.");
            string pathVar = Environment.GetEnvironmentVariable("PATH");
            string newPathVar = pathVar;
            if (IsUnix)
                newPathVar += ":";
            else
                newPathVar += ";";
            newPathVar += path;
            var target = EnvironmentVariableTarget.Machine;
            Environment.SetEnvironmentVariable("PATH", newPathVar, target);
            Console.WriteLine("Installation successful.");
        }
    }
}