using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mono.Options;
using System.Linq;

namespace CSCacheLib
{
    public class CSCache
    {
        string[] inputArgs = null;

        string compilerName = null;
        string compilerInfo = null;
        string outputExtension = null;
        string outputFile = null;
        string inputCompilerLine = null;

        List<string> compilerArgs = new List<string>();
        List<string> inputFiles = new List<string>();
        List<string> referenceFiles = new List<string>();
        List<string> moduleFiles = new List<string>();

        Config configuration = new Config();
        public void Cache()
        {
            int error = -1;
            bool showHelp = false;
            bool doClearCache = false;

            var options = new OptionSet {
                { "c|clear=", "clear cache. Options : =all", n => { if (n == "all") doClearCache = true; } },
                { "v|version", "show the current version of the tool.", v => { } },
                { "h|help", "show help message and exit.", h => showHelp = true },
            };
            List<string> extra = new List<string>();
            try
            {
                extra = options.Parse(inputArgs);
            }
            catch (OptionException e)
            {
                ConsoleTools.Error($"CSCache: {e.Message} \n Try `CSCache --help' for more information.", 1);
            }

            if (showHelp)
            {
                LibArgs.ShowHelp(options);
                return;
            }
            if (doClearCache)
            {
                LibArgs.ClearCache(configuration.cacheLocation);
                return;
            }

            if (extra.Count == 1)
            {
                ProcessInputCompiler(extra[0]);
            }
            else
            {
                ConsoleTools.Error("Error reading the compiler and arguments.\nTry `CSCache --help' for more information.", 1);
            }

            foreach (var item in configuration.versionArg)
            {
                compilerInfo = ConsoleTools.Execute(compilerName + " " + item, out error);
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

                FilesTools.GenerateFullPath(ref inputFiles, ref referenceFiles, ref moduleFiles);

                inputCache = GenerateInputCache();
                filesCache = MD5Tools.GenerateFilesCache(inputFiles);
                combinedCache = MD5Tools.CombineHashes(new List<byte[]> { inputCache, filesCache });
                CompareCache(combinedCache);
            }
            else
            {
                ConsoleTools.Error("Error getting the version of the compiler. Check the configuration file.", 2);
            }

        }

        public CSCache(string[] args)
		{
            inputArgs = args;
        }

        void CompareCache(byte[] combinedCache)
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
        void GenerateCache(string path, string filename)
        {
            int error;
            string executeMessage = ConsoleTools.Execute(inputCompilerLine, out error);
            
            if (error == 0)
            {
                Console.WriteLine(executeMessage);
                File.Copy(outputFile, path + filename + "bin.cache", true);
                File.WriteAllText(path + filename + ".cache", executeMessage);
            }
            else
            {
                ConsoleTools.Error(executeMessage, error);
            }
        }
        byte[] GenerateInputCache()
        {
            StringBuilder inputConcat = new StringBuilder();

            inputConcat.Append(compilerInfo);
            foreach (var item in compilerArgs)
            {
                inputConcat.Append(item);
            }
            inputConcat.Append(outputFile);
            foreach (var item in inputFiles)
            {
                inputConcat.Append(item);
            }
            foreach (var item in referenceFiles)
            {
                inputConcat.Append(item);
            }
            foreach (var item in moduleFiles)
            {
                inputConcat.Append(item);
            }

            return MD5Tools.MakeMD5String(inputConcat.ToString());
        }
        void ProcessInputCompiler(string input)
        {
            inputCompilerLine = input;
            string[] compParams = ParseTools.ParseArguments(input);

            if (compParams.Length > 1)
            {
                string[] newArr = new string[compParams.Length - 1];
                compilerName = compParams[0];
                Array.Copy(compParams, 1, newArr, 0, compParams.Length - 1);
                newArr = ParseTools.ParseArgArrayWithResponse(newArr);

                string outputArgsOpt = ParseTools.generateOption(configuration.outputArg);
                string referenceArgsOpt = ParseTools.generateOption(configuration.referenceArg);
                string targetArgsOpt = ParseTools.generateOption(configuration.targetArg);
                string recurseArgsOpt = ParseTools.generateOption(configuration.recurseArg);
                string ignoredArgsOpt = ParseTools.generateOption(configuration.ignoredArg);
                string moduleArgsOpt = ParseTools.generateOption(configuration.moduleArg);
                var options = new OptionSet()
                {
                    { outputArgsOpt + "=", "Output arguments", (o) => { outputFile = o; compilerArgs.Add(configuration.outputArg[0] + o); } },
                    { referenceArgsOpt + "=", "Reference arguments", (r) => { referenceFiles.AddRange(ParseTools.ParseComaSemicolon(r));  } },
                    { targetArgsOpt + "=", "Target arguments", (t) => { compilerArgs.Add(configuration.targetArg[0] + t); outputExtension = LibArgs.GetTargetExtenstion(t); } },
                    { recurseArgsOpt + "=", "Recurse arguments", (rc) => { inputFiles.AddRange(FilesTools.GetRecurseFiles(rc)); compilerArgs.Add(configuration.recurseArg[0] + rc); } },
                    { moduleArgsOpt + "=", "Add module arguments", (m) => { moduleFiles.AddRange(ParseTools.ParseComaSemicolon(m)); } },
                    { ignoredArgsOpt, "Ignored arguments", (i) => {  } },
                    { "<>", "Input file", (file) => { if(file[0] != '/' && file[0] != '-') { inputFiles.Add(file); } else { compilerArgs.Add(file); } } }
                };
                Console.WriteLine(configuration.targetArg[0]);
                try
                {
                    options.Parse(newArr);
                }
                catch (OptionException)
                {
                    ConsoleTools.Error("Invalid compiler parameters.", 1);
                }
                

                if (inputFiles.Count > 0)
                {
                    if (outputFile == null)
                    {
                        string outPathFile = Path.GetFullPath(inputFiles[0]);
                        outputFile = Path.ChangeExtension(outPathFile, (outputExtension == null) ? configuration.defaultExtension : outputExtension);
                    }

                    compilerArgs.Sort();
                }
                else
                {
                    ConsoleTools.Error("No input files detected.", 1);
                }
            }
            else
            {
                ConsoleTools.Error("Invalid compiler parameters.", 1);
            }
        }
    }
}
