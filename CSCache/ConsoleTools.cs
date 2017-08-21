using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace CSCacheLib
{
	public class ConsoleTools
    {
        static bool use_dos2unix = true;
        public static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 128) || (p == 6));
            }
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
                    info.RedirectStandardError = true;
                    info.RedirectStandardOutput = true;
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
            psi.RedirectStandardInput = true;

            if (use_dos2unix == false && !IsUnix)
            {
                psi.FileName = "cmd";
                psi.Arguments = String.Format("/c \"{0}\"", cmdLine);
            }
            else
            {
                psi.FileName = "sh";
                //if (!IsUnix)
                //{
                //    StringBuilder b = new StringBuilder();
                //    int count = 0;
                //    for (int i = 0; i < cmdLine.Length; i++)
                //    {
                //        if (cmdLine[i] == '`')
                //        {
                //            if (count % 2 != 0)
                //            {
                //                b.Append("|dos2unix");
                //            }
                //            count++;
                //        }
                //        b.Append(cmdLine[i]);
                //    }
                //    cmdLine = b.ToString();
                //}
                psi.Arguments = String.Format("-c \"{0}\"", cmdLine);
            }

            using (Process p = Process.Start(psi))
            {
                //p.EnableRaisingEvents = true;
                while (!p.StandardError.EndOfStream)
                {
                    consoleOutput.Add(p.StandardError.ReadLine());
                }
                while (!p.StandardOutput.EndOfStream)
                {
                    consoleOutput.Add(p.StandardOutput.ReadLine());
                }
                p.WaitForExit();
                errCode = p.ExitCode;
            }

            consoleOutput = consoleOutput.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            return string.Join("\n", consoleOutput);
        }

        public static void Error(string msg, int errCode)
        {
            Console.WriteLine("ERROR: {0}", msg);
            Environment.Exit(errCode);
        }
    }
}