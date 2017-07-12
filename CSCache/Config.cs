using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace CSCacheLib
{
    public class Config
    {

        public static string cacheLocation { get; } = null;
        public static string[] ignoredArguments { get; } = null;
        public static string versionArgument { get; } = null;
        public static string[] resourcesArguments { get; } = null;
        public static string[] outputArguments { get; } = null;

	    public Config()
	    {
            string path = (ConsoleTools.IsUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";
            if(File.Exists(path + "config.xml"))
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = path + "config.xml";
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                KeyValueConfigurationCollection settings = config.AppSettings.Settings;
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