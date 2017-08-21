using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSCacheLib
{
    class ParseTools
    {
        public static string[] ParseArguments(string commandLine)
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
            return (new string(parmChars)).Replace("\0", "").Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }
        public static string generateOption(string[] argArr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < argArr.Length; i++)
            {
                for (int counter = 0; counter < 2; counter++)
                {
                    if (argArr[i][0] == '-' || argArr[i][0] == '/')
                    {
                        argArr[i] = argArr[i].Substring(1);
                    }
                }
                if (argArr[i][argArr[i].Length - 1] == ':' || argArr[i][argArr[i].Length - 1] == '=')
                {
                    sb.Append(argArr[i].Substring(0, argArr[i].Length - 1));
                }
                else
                {
                    sb.Append(argArr[i]);
                }

                if (i < argArr.Length - 1)
                {
                    sb.Append("|");
                }
            }
            return sb.ToString();
        }
        static string[] ParseResponseFile(string file)
        {
            file = file.Replace("@", "");
            string[] result;
            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);
                string linesConcat = String.Join(" ", lines);
                result = ParseArguments(linesConcat);
            }
            else
            {
                result = null;
            }
            return result;
        }
        public static string[] ParseArgArrayWithResponse(string[] input)
        {
            Dictionary<int, string[]> indexReplaceList = new Dictionary<int, string[]>();
            List<string> tempList = input.ToList();

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i][0] == '@')
                {
                    string[] responseFileParse = ParseResponseFile(tempList[i]);
                    if(responseFileParse != null)
                    {
                        indexReplaceList.Add(i, responseFileParse);
                    }
                }
            }
            foreach (var item in indexReplaceList.OrderByDescending((x) => x.Key))
            {
                tempList.RemoveAt(item.Key);
                tempList.InsertRange(item.Key, item.Value);
            }
            return tempList.ToArray();
        }

        public static string[] ParseComaSemicolon(string input)
        {
            StringBuilder sb = new StringBuilder(input);

            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == ',' || sb[i] == ';')
                {
                    sb[i] = '\0';
                }
            }
            return sb.ToString().Split('\0');
        }
    }
}
