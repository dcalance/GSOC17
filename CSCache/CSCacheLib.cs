using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mono.Options;

namespace CSCacheLib
{
    public class CSCache
    {
        static string compilatorName = null;
        static string compilatorInfo = null;
        static string outputExtension = null;
        static string outputFile = null;
        static bool isOutputSpecified = false;

        static List<string> compilatorArgs = new List<string>();
        static List<string> inputFiles = new List<string>();
        static List<string> resourceFiles = new List<string>();
        static Config configuration = new Config();

        public static void CSCache_main(string[] args)
        {
            //string inputCompilator = null;
            //bool recievedComp = false;
            int error = -1;
            bool showHelp = false;
            //Console.WriteLine(configuration.cacheLocation);

            var options = new OptionSet {
                { "c|clear=", "the name of someone to greet.", n => { if (n == "all") LibArgs.ClearCache(configuration.cacheLocation); } },
                { "v|version", "show the current version of the tool.", v => { } },
                { "h|help", "show help message and exit.", h => showHelp = true },
            };

            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                // output some error message
                Console.Write("CSCache: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `CSCache --help' for more information.");
                return;
            }
            if (showHelp)
            {
                LibArgs.ShowHelp(options);
                return;
            }
            //foreach (var item in args)
            //{
            //    switch(item)
            //    {
            //        case "-args":
            //            // we will add arguments to cscache here later
            //            break;
            //        default:
            //            if (item[0] != '-' && !recievedComp)
            //            {
            //                inputCompilator = item;
            //                recievedComp = true;
            //            }
            //            else
            //            {
            //                //invalid arguments
            //            }
            //            break;
            //    }
            //}
            if (extra.Count == 1)
            {
                ProcessInputCompiler(extra[0]);
            }
            else
            {
                ConsoleTools.Error("Error reading the compiler and arguments.\nTry `CSCache --help' for more information.");
            }

            foreach (var item in configuration.versionArg)
            {
                compilatorInfo = ConsoleTools.Execute(compilatorName + " " + item, out error);
                if (error == 0)
                {
                    break;
                }
            }

            if (error == 0)
            {
                byte[] inputCache;
                byte[] filesCache;
                byte[] combinedCache;

                FilesTools.GenerateFullPath(ref inputFiles, ref resourceFiles);

                inputCache = GenerateInputCache();
                filesCache = MD5Tools.GenerateFilesCache(inputFiles);
                combinedCache = MD5Tools.CombineHashes(new List<byte[]> { inputCache, filesCache});
                CompareCache(combinedCache);
            }
            else
            {
            	ConsoleTools.Error("Error getting the version of the compiler. Check the configuration file.");
            }
            
        }

        static void CompareCache(byte[] combinedCache)
        {
            string filename = BitConverter.ToString(combinedCache).Replace("-", string.Empty);
            string path = configuration.cacheLocation;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(path + filename + ".cache"))
            {
                Console.WriteLine("Binares Returned.");
                Console.WriteLine(File.ReadAllText(path + filename + ".cache"));
                File.Copy(path + filename + "bin.cache", outputFile, true);
            }
            else
            {
                GenerateCache(path, filename);
            }
        }

        static void GenerateCache(string path, string filename)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(compilatorName + " ");
            foreach (var item in inputFiles)
            {
                sb.Append($"'{item}' ");
            }
            foreach (var item in resourceFiles)
            {
                sb.Append($"{configuration.resourcesArg}'{item}' ");
            }
            foreach (var item in compilatorArgs)
            {
                sb.Append(item + " ");
            }
            if (isOutputSpecified)
            {
                sb.Append($"{configuration.outputArg[0]}");
                if (configuration.outputArg[0][configuration.outputArg[0].Length - 1] != ':')
                {
                    sb.Append(" ");
                }
                sb.Append($"'{outputFile}'");
            }

            int error;
            string executeMessage = ConsoleTools.Execute(sb.ToString(), out error);
            Console.WriteLine(executeMessage);

            if (error == 0)
            {
                File.Copy(outputFile, path + filename + "bin.cache", true);
                File.WriteAllText(path + filename + ".cache", executeMessage);
            }
        }

        static byte[] GenerateInputCache()
        {
            StringBuilder inputConcat = new StringBuilder();

            inputConcat.Append(compilatorInfo);
            foreach (var item in compilatorArgs)
            {
                inputConcat.Append(item);
            }
            inputConcat.Append(outputFile);
            foreach (var item in inputFiles)
            {
                inputConcat.Append(item);
            }
            foreach (var item in resourceFiles)
            {
                inputConcat.Append(item);
            }

            return MD5Tools.MakeMD5String(inputConcat.ToString());
        }

        static bool ContainsStringFromStart(string input, out int argType, params string[] checkStr) //argType = 0 for arguments of type -arg:file , argType = 1 for arguments of type -arg file
        {
            argType = -1;
            foreach (var item in checkStr)
            {
                if (input.Length > item.Length && input.Substring(0, item.Length) == item)
                {
                    if (item[item.Length - 1] == ':')
                    {
                        argType = 0;
                    }
                    else
                    {
                        argType = 1;
                    }
                    return true;
                }
            }
            return false;
        }

        static string SplitArg(string input, int argType, string nextElement)
        {
            if (argType == 0)
            {
                string[] split = input.Split(':');
                return split[1];
            }
            else
            {
                return nextElement;
            }
        }

        static void ProcessInputCompiler(string input)
        {
            string[] compParams = ParseArguments(input);

            if (compParams.Length > 1)
            {
                compilatorName = compParams[0];
                for (int i = 1; i < compParams.Length; i++)
                {
                    if (compParams[i][0] != '-')
                    {
                        inputFiles.Add(compParams[i]);
                    }
                    else
                    {
                        int argType;
                        if (ContainsStringFromStart(compParams[i], out argType, configuration.outputArg))
                        {
                            outputFile = SplitArg(compParams[i], argType, (i < compParams.Length - 1) ? compParams[i + 1] : null);
                            isOutputSpecified = true;
                        }
                        else
                        if (ContainsStringFromStart(compParams[i], out argType, configuration.resourcesArg))
                        {
                            resourceFiles.Add(SplitArg(compParams[i], argType, (i < compParams.Length - 1) ? compParams[i + 1] : null));
                        }
                        else
                        if (ContainsStringFromStart(compParams[i], out argType, configuration.targetArg))
                        {
                            string outputArg = SplitArg(compParams[i], argType, (i < compParams.Length - 1) ? compParams[i + 1] : null);
                            switch (outputArg)
                            {
                                case "exe":
                                    outputExtension = ".exe";
                                    break;
                                case "winexe":
                                    outputExtension = ".exe";
                                    break;
                                case "library":
                                    outputExtension = ".dll";
                                    break;
                                case "module":
                                    outputExtension = ".netmodule";
                                    break;
                                default:
                                    break;
                            }
                            compilatorArgs.Add(compParams[i]);
                        }
                        else
                        if (ContainsStringFromStart(compParams[i], out argType, configuration.recurseArg))
                        {
                            string outputArg = SplitArg(compParams[i], argType, (i < compParams.Length - 1) ? compParams[i + 1] : null);
                            string[] files = FilesTools.GetRecurseFiles(outputArg);
                            foreach (var item in files)
                            {
                                inputFiles.Add(item);
                            }
                        }
                        else
                        {
                            compilatorArgs.Add(compParams[i]);
                        }
                    }
                }

                if (outputFile == null)
                {
                    string outPathFile = Path.GetFullPath(inputFiles[0]);
                    outPathFile = Path.ChangeExtension(outPathFile, null);

                    if (outputExtension == null)
                    {
                        outputFile = outPathFile + configuration.defaultExtension;
                    }
                    else
                    {
                        outputFile = outPathFile + outputExtension;
                    }
                }
                compilatorArgs.Sort();
            }
            else
            {
                ConsoleTools.Error("Invalid compiler parameters.");
            }
        }

        static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            char quoteChar = ' ';
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (!inQuote)
                {
                    quoteChar = ' ';
                }
                if (parmChars[index] == '"' && (quoteChar == '"' || quoteChar == ' '))
                {
                    quoteChar = '"';
                    inQuote = !inQuote;
                    parmChars[index] = '\0';
                }
                if (parmChars[index] == '\'' && (quoteChar == '\'' || quoteChar == ' '))
                {
                    quoteChar = '\'';
                    inQuote = !inQuote;
                    parmChars[index] = '\0';
                }
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Replace("\0", "").Split('\n');
        }
    }
}
