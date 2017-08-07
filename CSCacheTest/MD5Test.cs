using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using CSCacheLib;
using System.Collections.Generic;
using System.Linq;

namespace CSCacheTest
{
    [TestClass]
    public class MD5Test
    {
        [TestMethod]
        public void TestMakeMD5File()
        {
            string fileInput = "abcdef1234";
            byte[] expectedByte;
            using (var md5 = MD5.Create())
            {
                expectedByte = md5.ComputeHash(Encoding.ASCII.GetBytes(fileInput));
            }
            File.WriteAllText("test", fileInput);
            byte[] actualOutput = MD5Tools.MakeMD5File("test");
            CollectionAssert.AreEqual(expectedByte, actualOutput);
            File.Delete("test");
        }
        [TestMethod]
        public void TestMakeMD5String()
        {
            string input = "abcdef1234";
            string expectedOutput = "9bb793c73de0193293096d68f93d2e75";

            byte[] outputBytes = MD5Tools.MakeMD5String(input);
            StringBuilder actualOutput = new StringBuilder();
            for (int i = 0; i < outputBytes.Length; i++)
            {
                actualOutput.Append(outputBytes[i].ToString("x2"));
            }
            Assert.AreEqual(expectedOutput, actualOutput.ToString());
        }
        [TestMethod]
        public void TestCombineHashes()
        {
            List<byte[]> input = new List<byte[]>();
            input.Add(MD5Tools.MakeMD5String("abcdef"));
            input.Add(MD5Tools.MakeMD5String("1234567890"));

            StringBuilder expectedOutput = new StringBuilder();
            MD5 md5 = MD5.Create();
            byte[] expectedBytes = md5.ComputeHash(input[0].Concat(input[1]).ToArray());
            byte[] actualOutput = MD5Tools.CombineHashes(input);
            CollectionAssert.AreEqual(expectedBytes, actualOutput);
        }
    }
}
