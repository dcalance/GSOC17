using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using CSCacheLib;

namespace CSCacheTest
{
    [TestClass]
    public class FilesTest
    {
        [TestMethod]
        public void TestGenerateFullPath()
        {
            List<string> filesList = new List<string>();
            List<string> resourcesList = new List<string>();

            Directory.CreateDirectory("testDir");

            File.WriteAllText("testDir\\input1.cs", "text");
            File.WriteAllText("testDir\\input2.cs", "text");
            filesList.Add("testDir\\input1.cs");
            filesList.Add("testDir\\input2.cs");

            File.WriteAllText("testDir\\resource1.cs", "text");
            File.WriteAllText("testDir\\resource2.cs", "text");
            resourcesList.Add("testDir\\resource1.dll");
            resourcesList.Add("testDir\\resource2.dll");

            List<string> testFilesList = new List<string>(filesList);
            List<string> testResourcesList = new List<string>(resourcesList);

            FilesTools.GenerateFullPath(ref testFilesList, ref testResourcesList);
            for (int i = 0; i < testFilesList.Count; i++)
            {
                Assert.AreEqual(Path.GetFullPath(filesList[i]), testFilesList[i]);
            }
            for (int i = 0; i < resourcesList.Count; i++)
            {
                Assert.AreEqual(Path.GetFullPath(resourcesList[i]), testResourcesList[i]);
            }
            Directory.Delete("testDir", true);
        }
        [TestMethod]
        public void TestGetRecurseFiles()
        {
            Directory.CreateDirectory("testDir1");
            string pattern = "testDir1\\*.cs";

            File.WriteAllText("testDir1\\input1.cs", "text");
            File.WriteAllText("testDir1\\input2.cs", "text");
            File.WriteAllText("testDir1\\file.cs", "text");
            File.WriteAllText("testDir1\\input1.dll", "text");
            File.WriteAllText("testDir1\\input2.dll", "text");
            File.WriteAllText("testDir1\\lib.dll", "text");

            string[] results = FilesTools.GetRecurseFiles(pattern);
            string[] expectedResults = { Path.GetFullPath("testDir1\\file.cs"), Path.GetFullPath("testDir1\\input1.cs"), Path.GetFullPath("testDir1\\input2.cs") };
            CollectionAssert.AreEqual(expectedResults, results);

            pattern = "testDir1\\input1*";
            results = FilesTools.GetRecurseFiles(pattern);
            expectedResults = new string[] { Path.GetFullPath("testDir1\\input1.cs"), Path.GetFullPath("testDir1\\input1.dll")};
            CollectionAssert.AreEqual(expectedResults, results);

            pattern = "testDir1\\*.exe";
            results = FilesTools.GetRecurseFiles(pattern);
            expectedResults = new string[] { };
            CollectionAssert.AreEqual(expectedResults, results);

            Directory.Delete("testDir1", true);
        }
    }
}
