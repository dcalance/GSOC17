using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;

namespace CSCacheLib
{
	public class MD5Tools
    {
        public static byte[] GenerateFilesCache(List<string> inputF)
        {
            List<byte[]> filesCache = new List<byte[]>();

            foreach (var item in inputF)
            {
                filesCache.Add(MD5Tools.MakeMD5File(item));
            }
            return MD5Tools.CombineHashes(filesCache);
        }

        public static bool CompareMD5(byte[] input1, byte[] input2)
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

        public static byte[] CombineHashes(List<byte[]> input)
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

        public static byte[] MakeMD5String(string input)
        {
            byte[] byteArr;
            using (var md5 = MD5.Create())
            {
                byte[] intputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byteArr = md5.ComputeHash(intputBytes);
            }
            return byteArr;
        }

        public static byte[] MakeMD5File(string filename)
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
    }
}