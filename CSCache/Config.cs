using System;
using System.Collections.Generic;
using System.IO;

namespace Configuration
{
    public class Config
    {
        static bool isUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 128) || (p == 6));
            }
        }
        public static string? cacheLocation { get; } = null;
        public static string[] ignoredArguments { get; } = null;
        public static string? versionArgument { get; } = null;
        public static string[] resourcesArguments { get; } = null;
        public static string[] outputArguments { get; } = null;

	    public Config()
	    {
            string path = (isUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";
            if(File.Exists(path + "config.ini"))
            {
                string[] lines = System.IO.File.ReadAllLines(path + "config.ini");
                foreach (var line in lines)
                {
                    string[] part = line.Split('=');
                    part[0] = part[0].Replace(" ", "");

                    try
                    {
                        switch (part[0])
                        {
                            case "cacheLocation":
                                cacheLocation = part[1].Replace(" ", "");
                                break;
                            case "ignoredArguments:":
                                string[] splitArgs = part[1].Split(',');
                                ignoredArguments = new string[splitArgs.Length];

                                for (int i = 0; i < splitArgs.Length; i++)
                                {
                                    ignoredArguments[i] = splitArgs[i].Replace(" ", "");
                                }
                                break;
                            case "versionArgument":
                                versionArgument = part[1].Replace(" ", "");
                                break;
                            case "resourcesArguments":
                                string[] splitArgs = part[1].Split(',');
                                resourcesArguments = new string[splitArgs.Length];

                                for (int i = 0; i < splitArgs.Length; i++)
                                {
                                    resourcesArguments[i] = splitArgs[i].Replace(" ", "");
                                }
                                break;
                            case "outputArguments":
                                string[] splitArgs = part[1].Split(',');
                                outputArguments = new string[splitArgs.Length];

                                for (int i = 0; i < splitArgs.Length; i++)
                                {
                                    outputArguments[i] = splitArgs[i].Replace(" ", "");
                                }
                                break;
                        }
                    }
                    catch
                    {
                        Error("Error reading config file.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Config file not found. Loading default configuration.");
                LoadDefaultConfig();
            }
        }
        void LoadDefaultConfig()
        {

        }
        static void Error(string msg, params object[] args)
        {
            Console.WriteLine("ERROR: {0}", string.Format(msg, args));
            Environment.Exit(1);
        }
    }
}