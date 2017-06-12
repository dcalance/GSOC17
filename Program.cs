using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GSOC17
{
    class Program
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
        static void Main(string[] args)
        {
            Execute("cd");
            Console.ReadKey();
        }
        static void Execute(string cmdLine)
        {
            if(IsUnix)
            {
                Console.WriteLine("[execute cmd]: " + cmdLine);
                int ret = system(cmdLine);
                if (ret != 0)
                {
                    Error(String.Format("[Fail] {0}", ret));
                }
                return;
            }
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
                    Console.WriteLine("Warning: dos2unix not found");
                    use_dos2unix = false;
                }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;

            if (use_dos2unix == false)
            {
                psi.FileName = "cmd";
                psi.Arguments = String.Format("/c \"{0}\"", cmdLine);
            }
            else
            {
                psi.FileName = "sh";
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
                psi.Arguments = String.Format("-c \"{0}\"", cmdLine);
            }

            Console.WriteLine(cmdLine);
            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
                int ret = p.ExitCode;
                if (ret != 0)
                {
                    Error("[Fail] {0}", ret);
                }
            }

        }
        static void Error(string msg, params object[] args)
        {
            Console.Error.WriteLine("ERROR: {0}", string.Format(msg, args));
            Environment.Exit(1);
        }
        [DllImport("libc")] static extern int system(string s);
    }
}
