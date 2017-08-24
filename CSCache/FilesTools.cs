using System;
using System.Collections.Generic;
using System.IO;

namespace CSCacheLib
{
    public class FilesTools
    {
        
        public static void GenerateFullPath(ref List<string> inputF, ref List<string> resourceF, ref List<string> moduleF)
        {
            for (int i = 0; i < inputF.Count; i++)
            {
                if (File.Exists(inputF[i]))
                {
                    inputF[i] = Path.GetFullPath(inputF[i]);
                }
                else
                {
                    ConsoleTools.Error($"Input file not found: {inputF[i]}", 3);
                }
            }
            for (int i = 0; i < resourceF.Count; i++)
            {
                if (File.Exists(resourceF[i]))
                {
                    resourceF[i] = Path.GetFullPath(resourceF[i]);
                }
            }
            for (int i = 0; i < moduleF.Count; i++)
            {
                if (File.Exists(moduleF[i]))
                {
                    moduleF[i] = Path.GetFullPath(moduleF[i]);
                }
            }

            inputF.Sort();
            resourceF.Sort();
            moduleF.Sort();
        }

        public static string[] GetRecurseFiles(string pattern)
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string[] files = Directory.GetFiles(currentDirectory, pattern, SearchOption.AllDirectories);
            return files;
        }
    }
}