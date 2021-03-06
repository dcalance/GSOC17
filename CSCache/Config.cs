﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Text;
using System.Linq;

namespace CSCacheLib
{
    public class Config
    {
        public string cacheLocation { get; private set; } = null;
        public string[] ignoredArg { get; private set; } = null;
        public string[] versionArg { get; private set; } = null;
        public string[] referenceArg { get; private set; } = null;
        public string[] outputArg { get; private set; } = null;
        public string[] targetArg { get; private set; } = null;
        public string[] recurseArg { get; private set; } = null;
        public string[] moduleArg { get; private set; } = null;
        public string defaultExtension { get; private set; } = null;

        string path = (ConsoleTools.IsUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";

        public Config()
	    {
            if(!File.Exists(path + "config.xml"))
            {
                Console.WriteLine("Config file not found. Loading default configuration.");
                LoadDefaultConfig();
            }
            ProcessConfigFile(path + "config.xml");

            if (cacheLocation == null || 
                versionArg == null || 
                referenceArg == null || 
                outputArg == null || 
                defaultExtension == null || 
                moduleArg == null || 
                ignoredArg == null || 
                targetArg == null ||
                recurseArg == null)
            {
                ConsoleTools.Error("Error in file configuration.\nChange the configuration or delete the configuration and reload the program.", 3);
            }
        }

        string[] ProcessArray(string input)
        {
            input = input.Replace(" ", "");
            string[] result = input.Split(',');
            result = result.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return result;
        }

        void ProcessConfigFile(string configFilename)
        {
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = configFilename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;

            foreach (KeyValueConfigurationElement item in settings)
            {
                switch (item.Key)
                {
                    case "CacheLocation":
                        cacheLocation = item.Value;
                        break;
                    case "IgnoredArguments":
                        ignoredArg = ProcessArray(item.Value);
                        break;
                    case "VersionArgument":
                        versionArg = ProcessArray(item.Value);
                        break;
                    case "ReferenceArgument":
                        referenceArg = ProcessArray(item.Value);
                        break;
                    case "OutputArgument":
                        outputArg = ProcessArray(item.Value);
                        break;
                    case "TagetArgument":
                        targetArg = ProcessArray(item.Value);
                        break;
                    case "RecurseArgument":
                        recurseArg = ProcessArray(item.Value);
                        break;
                    case "AddModuleArgument":
                        moduleArg = ProcessArray(item.Value);
                        break;
                    case "DefaultExtension":
                        defaultExtension = item.Value;
                        break;
                }
            }
        }

        void LoadDefaultConfig()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<configuration>");
            sb.AppendLine("\t<appSettings>");
            sb.AppendLine($"\t\t<add key=\"CacheLocation\" value=\"{path}\"/>");
            sb.AppendLine("\t\t<add key=\"IgnoredArguments\" value=\"--stacktrace,--timestamp,-v\"/>");
            sb.AppendLine("\t\t<add key=\"VersionArgument\" value=\"--version\"/>");
            sb.AppendLine("\t\t<add key=\"ReferenceArgument\" value=\"-r:, -reference:\"/>");
            sb.AppendLine("\t\t<add key=\"OutputArgument\" value=\"-out:, -o, -output=\"/>");
            sb.AppendLine("\t\t<add key=\"TagetArgument\" value=\"-t:, -target:\"/>");
            sb.AppendLine("\t\t<add key=\"RecurseArgument\" value=\"-recurse:\"/>");
            sb.AppendLine("\t\t<add key=\"AddModuleArgument\" value=\"--addmodule:\"/>");
            sb.AppendLine("\t\t<add key=\"DefaultExtension\" value=\".exe\"/>");
            sb.AppendLine("\t</appSettings>");
            sb.AppendLine("</configuration>");
            Directory.CreateDirectory(path);
            File.WriteAllText(path + "config.xml", sb.ToString());
        }
    }
}
