using System;
using System.Collections.Generic;
using System.IO;

namespace CSCacheLib
{
    public class FilesTools
    {
        
        public static void GenerateFullPath(ref List<string> inputF, ref List<string> resourceF)
        {
            for (int i = 0; i < inputF.Count; i++)
            {
                if (File.Exists(inputF[i]))
                {
                    inputF[i] = Path.GetFullPath(inputF[i]);
                }
                else
                {
                    ConsoleTools.Error("Input " + inputF[i] + " not found.");
                }
            }
            for (int i = 0; i < resourceF.Count; i++)
            {
                if (File.Exists(resourceF[i]))
                {
                    resourceF[i] = Path.GetFullPath(resourceF[i]);
                }
                else
                {
                    ConsoleTools.Error("Resource " + resourceF[i] + " not found.");
                }
            }
            inputF.Sort();
            resourceF.Sort();
        }

        public static string[] GetRecurseFiles(string pattern)
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string[] files = Directory.GetFiles(currentDirectory, pattern, SearchOption.AllDirectories);
            return files;
        }
    }
}