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
        static string compilatorInfo;
        static List<string> compilatorArgs = new List<string>();
        static string outputFile;
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

            compilatorInfo = Execute(compilatorInfo + " --version", out error);
            if (error == 0)
            {
            	Console.WriteLine(compilatorInfo);
            }
            else
            {
            	Console.WriteLine("Incompatible compiler.");
            	Console.WriteLine(compilatorInfo);
            }
            Console.WriteLine("Input files:");
            foreach(var item in inputFiles)
            {
            	Console.Write(item + " ");
            }
            Console.WriteLine();
            foreach(var bytte in MakeMD5File("CSCache.cs"))
            {
            	Console.WriteLine(bytte);
            }
        }

        static void GenerateInputCache()
        {
        	
        }

        static void ProcessInputCompilator(string compilatorWithParams)
        {
            string[] compParams = compilatorWithParams.Split(' ');
            string compilatorName = compParams[0];
            compilatorInfo = compilatorName;

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
