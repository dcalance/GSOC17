using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSCacheLib;
using System.Collections.Generic;
using System.IO;

namespace CSCacheTest
{
    [TestClass]
    public class CSCacheTest
    {
        [TestMethod]
        public void TestCSCacheLib()
        {
            Config configuration = new Config();
            CSCache cscache = new CSCache();

            cscache.CSCache_main(new string[] { "-clear=all" });
            int cacheCount = Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length;

            string expectedCompilerName = "mcs";
            string expectedCompilerInfo = "Mono C# compiler version 5.0.1.0";
            string expectedOutputExtension = null;
            string expectedOutputFile = Path.GetFullPath("testDir2\\input1.exe");
            bool expectedIsOutputSpecified = false;

            int expectedCacheCount = cacheCount + 2;

            Directory.CreateDirectory("testDir2");
            
            string[] input1 = { "public class Hello",
                                  "{",
                                  "public static void Main()",
                                  "{",
                                  "System.Console.WriteLine(\"Hello, World!\");",
                                  "}",
                                  "}"};
            File.WriteAllLines("testDir2\\input1.cs", input1);

            string[] input = { "mcs testDir2\\input1.cs" };
            cscache = new CSCache();
            cscache.CSCache_main(input);

            PrivateObject obj = new PrivateObject(cscache);
            string compilerName = (string)obj.GetFieldOrProperty("compilerName");
            string compilerInfo = (string)obj.GetFieldOrProperty("compilerInfo");
            string outputExtension = (string)obj.GetFieldOrProperty("outputExtension");
            string outputFile = (string)obj.GetFieldOrProperty("outputFile");
            bool isOutputSpecified = (bool)obj.GetFieldOrProperty("isOutputSpecified");

            List<string> compilerArgs = (List<string>)obj.GetFieldOrProperty("compilerArgs");
            List<string> inputFiles = (List<string>)obj.GetFieldOrProperty("inputFiles");
            List<string> resourceFiles = (List<string>)obj.GetFieldOrProperty("resourceFiles");

            Assert.AreEqual(expectedCompilerName, compilerName);
            Assert.AreEqual(expectedCompilerInfo, compilerInfo);
            Assert.AreEqual(expectedOutputExtension, outputExtension);
            Assert.AreEqual(expectedOutputFile, outputFile);
            Assert.AreEqual(expectedIsOutputSpecified, isOutputSpecified);
            Assert.AreEqual(expectedCacheCount, Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length);
            Assert.IsTrue(File.Exists("testDir2\\input1.exe"));

            Directory.Delete("testDir2", true);
        }
    }
}
