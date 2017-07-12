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

        static List<string> compilatorArgs = new List<string>();
        static List<string> inputFiles = new List<string>();
        static List<string> resourceFiles = new List<string>();

        public static void CSCache_main(string[] args)
        {
            string inputCompilator = null;
            bool recievedComp = false;
            int error;

            Config cfg = new Config();

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
            compilatorInfo = ConsoleTools.Execute(compilatorName + " --version", out error);
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
            	Console.WriteLine("Incompatible compiler.");
            	Console.WriteLine(compilatorInfo);
            }
            
        }

        static void CompareCache(byte[] inputCache, byte[] filesCache)
        {
            string filename = BitConverter.ToString(inputCache).Replace("-", string.Empty);
            
            string path = (ConsoleTools.IsUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/" 
            					   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";
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
                    File.Copy(path + filename + "bin", outputFile, true);
                    Console.WriteLine("binaries returned");
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
                sb.Append($"-r:'{item}' ");
            }
            foreach (var item in compilatorArgs)
            {
                sb.Append(item + " ");
            }
            sb.Append("-out:" + outputFile);

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
                    if (compParams[i].Length > 4 && compParams[i].Substring(0, 5) == "-out:")
                    {
                        string[] outputArg = compParams[i].Split(':');
                        outputFile = outputArg[1];
                    }
                    else
                    if ((compParams[i].Length == 2 && compParams[i].Substring(0, 2) == "-o") || 
                    	(compParams[i].Length == 8 && compParams[i].Substring(0, 8) == "--output"))
                    {
                        outputFile = compParams[++i];
                    }
                    else
                    if (compParams[i].Length > 3 && compParams[i].Substring(0, 3) == "-r:")
                    {
                        string[] outputArg = compParams[i].Split(':');
                        resourceFiles.Add(outputArg[1]);
                    }
                    else
                    if (compParams[i].Length > 3 && compParams[i].Substring(0, 3) == "-t:")
                    {
                        string[] outputArg = compParams[i].Split(':');
                        switch(outputArg[1])
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
                    if (compParams[i].Length > 9 && compParams[i].Substring(0, 9) == "-recurse:")
                    {
                        string[] outputArg = compParams[i].Split(':');
                        string[] files = FilesTools.GetRecurseFiles(outputArg[1]);
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
                if (outputExtension == null)
                {
                    outputFile = "out.exe";
                }
                else
                {
                    outputFile = "out" + outputExtension;
                }
            }
            compilatorArgs.Sort();
        }
    }
}
