using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace CSCacheLib
{
    public class CSCache
    {
        static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 128) || (p == 6));
            }
        }
        static bool use_dos2unix = true;
        static string compilatorName = null;
        static string compilatorInfo = null;
        static List<string> compilatorArgs = new List<string>();
        static string outputFile = null;
        static List<string> inputFiles = new List<string>();

        public static void CSCache_main(string[] args)
        {
            string inputCompilator = null;
            bool recievedComp = false;
            int error;

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
            compilatorInfo = Execute(compilatorName + " --version", out error);
            if (error == 0)
            {
                byte[] inputCache;
                byte[] filesCache;

                inputCache = GenerateInputCache();
                filesCache = GenerateFilesCache();

                CompareCache(inputCache, filesCache);
            }
            else
            {
            	Console.WriteLine("Incompatible compiler.");
            	Console.WriteLine(compilatorInfo);
            }
            
        }

        static bool CompareMD5(byte[] input1, byte[] input2)
        {
            int i = 0;
            while ((i < input1.Length) && (input1[i] == input2[i]))
            {
                i += 1;
            }
            if (i == input1.Length)
            {
                return true;
            }
            return false;
        }

        static void CompareCache(byte[] inputCache, byte[] filesCache)
        {
            string filename = BitConverter.ToString(inputCache).Replace("-", string.Empty);
            string path = (IsUnix) ? @"$HOME/.cscache/" : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(path + filename))
            {
                byte[] fileBytes = File.ReadAllBytes(path + filename);
                bool isEqual = CompareMD5(fileBytes, filesCache);

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
            using (FileStream fs = File.Create(path + filename))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(filesCache);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(compilatorName + " ");
            foreach (var item in inputFiles)
            {
                sb.Append(item + " ");
            }
            foreach (var item in compilatorArgs)
            {
                sb.Append(item + " ");
            }
            sb.Append("-out:" + outputFile);
            int error;
            Console.WriteLine(Execute(sb.ToString(), out error));
            if (error == 0)
            {
                File.Copy(outputFile, path + filename + "bin", true);
            }
        }

        static byte[] GenerateFilesCache()
        {
            List<byte[]> filesCache = new List<byte[]>();

            foreach (var item in inputFiles)
            {
                if (File.Exists(item))
                {
                    filesCache.Add(MakeMD5File(item));
                }
                else
                {
                    Error(item + " not found.");
                }
            }
            return CombineHashes(filesCache);
        }

        static byte[] CombineHashes(List<byte[]> input)
        {
            byte[] byteArr;
            byte[] inputArr = new byte[input[0].Length * input.Count];
            int line = 0;

            foreach (var item in input)
            {
                item.CopyTo(inputArr, item.Length * line);
                line += 1;
            }

            using (var md5 = MD5.Create())
            {
                byteArr = md5.ComputeHash(inputArr);
            }
            return byteArr;
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

            return MakeMD5String(inputConcat.ToString());
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
                    {
                        compilatorArgs.Add(compParams[i]);
                    }
                }
            }
            if (outputFile == null)
            {
                outputFile = "out.exe";
            }

            compilatorArgs.Sort();
            inputFiles.Sort();
        }

        static byte[] MakeMD5String(string input)
        {
        	byte[] byteArr;
        	using (var md5 = MD5.Create())
        	{
        		byte[] intputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        		byteArr = md5.ComputeHash(intputBytes);
        	}
        	return byteArr;
        }

        static byte[] MakeMD5File(string filename)
        {
        	byte[] byteArr;
        	using (var md5 = MD5.Create())
        	{
        		using (var stream = File.OpenRead(filename))
        		{
        			byteArr = md5.ComputeHash(stream);
        		}
        	}
        	return byteArr;
        }

        public static string Execute(string cmdLine, out int errCode)
        {
            List<string> consoleOutput = new List<string>();
            if (use_dos2unix == true)
                try
                {
                    var info = new ProcessStartInfo("dos2unix");
                    info.CreateNoWindow = true;
                    info.RedirectStandardInput = true;
                    info.UseShellExecute = false;
                    var dos2unix = Process.Start(info);
                    dos2unix.StandardInput.WriteLine("aaa");
                    dos2unix.StandardInput.WriteLine("\u0004");
                    dos2unix.StandardInput.Close();
                    dos2unix.WaitForExit();
                    if (dos2unix.ExitCode == 0)
                        use_dos2unix = true;
                }
                catch
                {
                    //Console.WriteLine("Warning: dos2unix not found");
                    use_dos2unix = false;
                }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            if (use_dos2unix == false && !IsUnix)
            {
                psi.FileName = "cmd";
                psi.Arguments = String.Format("/c \"{0}\"", cmdLine);
            }
            else
            {
                psi.FileName = "sh";
                if (!IsUnix)
                {
                    StringBuilder b = new StringBuilder();
                    int count = 0;
                    for (int i = 0; i < cmdLine.Length; i++)
                    {
                        if (cmdLine[i] == '`')
                        {
                            if (count % 2 != 0)
                            {
                                b.Append("|dos2unix");
                            }
                            count++;
                        }
                        b.Append(cmdLine[i]);
                    }
                    cmdLine = b.ToString();
                }
                psi.Arguments = String.Format("-c \"{0}\"", cmdLine);
            }

            //Console.WriteLine(cmdLine);
            using (Process p = Process.Start(psi))
            {
                while (!p.StandardOutput.EndOfStream)
                {
                    consoleOutput.Add(p.StandardOutput.ReadLine());
                }
                while (!p.StandardError.EndOfStream)
                {
                	consoleOutput.Add(p.StandardError.ReadLine());
                }
                p.WaitForExit();
                errCode = p.ExitCode;
            }
            return string.Join("\n", consoleOutput);
        }
        static void Error(string msg, params object[] args)
        {
            Console.WriteLine("ERROR: {0}", string.Format(msg, args));
            Environment.Exit(1);
        }
    }
}
