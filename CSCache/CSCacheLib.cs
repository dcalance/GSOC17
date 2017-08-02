using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            string inputCompilator = null;
            bool recievedComp = false;
            int error = -1;

            foreach (var item in args)
            {
                switch(item)
                {
                    case "-args":
                        // we will add arguments to cscache here later
                        break;
                    default:
                        if (item[0] != '-' && !recievedComp)
                        {
                            inputCompilator = item;
                            recievedComp = true;
                        }
                        else
                        {
                            //invalid arguments
                        }
                        break;
                }
            }
            if (inputCompilator != null)
            {
                ProcessInputCompilator(inputCompilator);
            }
            else
            {
                //error
            }
            int counter;
            for (counter = 0; counter < configuration.versionArg.Length; counter += 1)
            {
                compilatorInfo = ConsoleTools.Execute(compilatorName + " " + configuration.versionArg[counter], out error);
                if (error == 0)
                {
                    break;
                }
            }

            if (error == 0)
            {
                byte[] inputCache;
                byte[] filesCache;

                FilesTools.GenerateFullPath(ref inputFiles, ref resourceFiles);

                inputCache = GenerateInputCache();
                filesCache = MD5Tools.GenerateFilesCache(inputFiles);

                CompareCache(inputCache, filesCache);
            }
            else
            {
            	ConsoleTools.Error("Error getting the version of the compiler. Check the configuration file.");
            }
            
        }

        static void CompareCache(byte[] inputCache, byte[] filesCache)
        {
            string filename = BitConverter.ToString(inputCache).Replace("-", string.Empty);

            string path = configuration.cacheLocation;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(path + filename))
            {
                byte[] fileBytes = File.ReadAllBytes(path + filename);
                bool isEqual = MD5Tools.CompareMD5(fileBytes, filesCache);

                if (isEqual)
                {
                    Console.WriteLine("binaries returned");
                    File.Copy(path + filename + "bin", outputFile, true);
                }
                else
                {
                    GenerateCache(path, filename, filesCache);
                }
            }
            else
            {
                GenerateCache(path, filename, filesCache);
            }
        }

        static void GenerateCache(string path, string filename, byte[] filesCache)
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
            Console.WriteLine(ConsoleTools.Execute(sb.ToString(), out error));
            if (error == 0)
            {
                File.Copy(outputFile, path + filename + "bin", true);

                using (FileStream fs = File.Create(path + filename))
            	{
	                using (BinaryWriter bw = new BinaryWriter(fs))
	                {
	                    bw.Write(filesCache);
	                }
            	}
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

        static void ProcessInputCompilator(string compilatorWithParams)
        {
            string[] compParams = compilatorWithParams.Split(' ');
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
                            case "library":
                                outputExtension = ".dll";
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
                    outputFile = outPathFile + ".exe";
                }
                else
                {
                    outputFile = outPathFile + outputExtension;
                }
            }
            compilatorArgs.Sort();
        }
    }
}
