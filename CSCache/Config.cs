using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Text;

namespace CSCacheLib
{
    public class Config
    {
        public string cacheLocation { get; private set; } = null;
        public string[] ignoredArg { get; private set; } = null;
        public string versionArg { get; private set; } = null;
        public string resourcesArg { get; private set; } = null;
        public string outputArg { get; private set; } = null;
        public string targetArg { get; private set; } = null;
        public string recurseArg { get; private set; } = null;

        string path = (ConsoleTools.IsUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";

        public Config()
	    {
            if(File.Exists(path + "config.xml"))
            {
                ProcessConfigFile(path + "config.xml");
            }
            else
            {
                Console.WriteLine("Config file not found. Loading default configuration.");
                LoadDefaultConfig();
                ProcessConfigFile(path + "config.xml");
            }
            if(cacheLocation == null || ignoredArg == null || versionArg == null || resourcesArg == null || outputArg == null)
            {
                ConsoleTools.Error("Error in file configuration.");
            }
        }

        string[] ProcessArray(string input)
        {
            string[] result = input.Split(',');
            return result;
        }

        string ProcessPath(string input)
        {
            string[] inputSplit = input.Split('%');
            if (inputSplit.Length > 2)
            {
                return Environment.GetEnvironmentVariable(inputSplit[1]) + inputSplit[2];
            }
            else
            {
                return input;
            }   
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
                        cacheLocation = ProcessPath(item.Value);
                        break;
                    case "IgnoredArguments":
                        ignoredArg = ProcessArray(item.Value);
                        break;
                    case "VersionArgument":
                        versionArg = item.Value;
                        break;
                    case "ResourcesArgument":
                        resourcesArg = item.Value;
                        break;
                    case "OutputArgument":
                        outputArg = item.Value;
                        break;
                    case "TagetArgument":
                        targetArg = item.Value;
                        break;
                    case "RecurseArgument":
                        recurseArg = item.Value;
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
            sb.AppendLine("\t\t<add key=\"ResourcesArgument\" value=\"-r:\"/>");
            sb.AppendLine("\t\t<add key=\"OutputArgument\" value=\"-out:\"/>");
            sb.AppendLine("\t\t<add key=\"TagetArgument\" value=\"-t:\"/>");
            sb.AppendLine("\t</appSettings>");
            sb.AppendLine("</configuration>");
            File.WriteAllText(path + "config.xml", sb.ToString());
        }
    }
}