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
        public void TestCSCacheLib1()
        {
            Config configuration = new Config();
            CSCache cscache = new CSCache(new string[] { "-clear=all" });
            int cacheCount = Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length;

            string expectedCompilerName = "mcs";
            string expectedCompilerInfo = "Mono C# compiler version 5.0.1.0";
            string expectedOutputExtension = ".dll";
            string expectedOutputFile = Path.GetFullPath("testDir2\\input1.dll");

            List<string> expectedCompilerArgs = new List<string> { "-t:library" };
            List<string> expectedInputFiles = new List<string> { Path.GetFullPath("testDir2\\input1.cs") };
            List<string> expectedResourceFiles = new List<string>();

            int expectedCacheCount = cacheCount + 2;

            Directory.CreateDirectory("testDir2");

            string[] input1 = {     "namespace Library",
                                    "{",
                                        "public class Lib",
                                        "{",
                                            "public static void func1(string input)",
                                            "{",
                                                "System.Console.WriteLine(input);",
                                            "}",
                                        "}",
                                    "}"};
            File.WriteAllLines("testDir2\\input1.cs", input1);

            string[] input = { "mcs testDir2\\input1.cs -t:library" };
            cscache = new CSCache(input);

            PrivateObject obj = new PrivateObject(cscache);
            string compilerName = (string)obj.GetFieldOrProperty("compilerName");
            string compilerInfo = (string)obj.GetFieldOrProperty("compilerInfo");
            string outputExtension = (string)obj.GetFieldOrProperty("outputExtension");
            string outputFile = (string)obj.GetFieldOrProperty("outputFile");

            List<string> compilerArgs = (List<string>)obj.GetFieldOrProperty("compilerArgs");
            List<string> inputFiles = (List<string>)obj.GetFieldOrProperty("inputFiles");
            List<string> referenceFiles = (List<string>)obj.GetFieldOrProperty("referenceFiles");

            Assert.AreEqual(expectedCompilerName, compilerName);
            Assert.AreEqual(expectedCompilerInfo, compilerInfo);
            Assert.AreEqual(expectedOutputExtension, outputExtension);
            Assert.AreEqual(expectedOutputFile, outputFile);
            Assert.AreEqual(expectedCacheCount, Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length);
            Assert.IsTrue(File.Exists("testDir2\\input1.dll"));
            Config cfg = new Config();
            Console.WriteLine(cfg.targetArg); 

            CollectionAssert.AreEqual(expectedCompilerArgs, compilerArgs);
            CollectionAssert.AreEqual(expectedInputFiles, inputFiles);
            CollectionAssert.AreEqual(expectedResourceFiles, referenceFiles);

            expectedOutputExtension = null;
            expectedOutputFile = "testDir2\\hello.exe";

            expectedCacheCount += 2;

            expectedCompilerArgs = new List<string> { "-out:testDir2\\hello.exe" };
            expectedResourceFiles = new List<string> { Path.GetFullPath("testDir2\\input1.dll") };
            expectedInputFiles =  new List<string> { Path.GetFullPath("testDir2\\input2.cs") };


            input1 = new string[]
            {
                "using Library;",
                "namespace hello",
                "{",
                    "class Hello",
                    "{",
                        "static void Main(string[] args)",
                        "{",
                            "Lib.func1(\"Hello World\");",
                        "}",
                    "}",
                "}",
            };

            File.WriteAllLines("testDir2\\input2.cs", input1);
            input = new string[] { "mcs testDir2\\input2.cs -r:testDir2\\input1.dll -out:testDir2\\hello.exe" };
            cscache = new CSCache(input);

            obj = new PrivateObject(cscache);
            compilerName = (string)obj.GetFieldOrProperty("compilerName");
            compilerInfo = (string)obj.GetFieldOrProperty("compilerInfo");
            outputExtension = (string)obj.GetFieldOrProperty("outputExtension");
            outputFile = (string)obj.GetFieldOrProperty("outputFile");

            compilerArgs = (List<string>)obj.GetFieldOrProperty("compilerArgs");
            inputFiles = (List<string>)obj.GetFieldOrProperty("inputFiles");
            referenceFiles = (List<string>)obj.GetFieldOrProperty("referenceFiles");

            Assert.AreEqual(expectedCompilerName, compilerName);
            Assert.AreEqual(expectedCompilerInfo, compilerInfo);
            Assert.AreEqual(expectedOutputExtension, outputExtension);
            Assert.AreEqual(expectedOutputFile, outputFile);
            Assert.AreEqual(expectedCacheCount, Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length);
            Assert.IsTrue(File.Exists("testDir2\\hello.exe"));

            CollectionAssert.AreEqual(expectedCompilerArgs, compilerArgs);
            CollectionAssert.AreEqual(expectedInputFiles, inputFiles);
            CollectionAssert.AreEqual(expectedResourceFiles, referenceFiles);

            Directory.Delete("testDir2", true);
        }
        [TestMethod]
        public void TestCSCacheLib2()
        {
            Config configuration = new Config();
            CSCache cscache = new CSCache(new string[] { "-clear=all" });
            int cacheCount = Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length;

            string expectedCompilerName = "mcs";
            string expectedCompilerInfo = "Mono C# compiler version 5.0.1.0";
            string expectedOutputExtension = null;
            string expectedOutputFile = Path.GetFullPath("testDir3\\input1.exe");

            List<string> expectedCompilerArgs = new List<string>();
            List<string> expectedInputFiles = new List<string> { Path.GetFullPath("testDir3\\input1.cs") };
            List<string> expectedResourceFiles = new List<string>();
            List<string> expectedIgnoredArgs = new List<string> { "--stacktrace" };

            int expectedCacheCount = cacheCount + 2;

            Directory.CreateDirectory("testDir3");

            string[] input1 = new string[]
            {
                "namespace hello",
                "{",
                    "class Hello",
                    "{",
                        "static void Main(string[] args)",
                        "{",
                            "System.Console.WriteLine(\"HelloWorld\");",
                        "}",
                    "}",
                "}",
            };
            File.WriteAllLines("testDir3\\input1.cs", input1);

            string[] input = { "mcs testDir3\\input1.cs --stacktrace" };
            cscache = new CSCache(input);

            PrivateObject obj = new PrivateObject(cscache);
            string compilerName = (string)obj.GetFieldOrProperty("compilerName");
            string compilerInfo = (string)obj.GetFieldOrProperty("compilerInfo");
            string outputExtension = (string)obj.GetFieldOrProperty("outputExtension");
            string outputFile = (string)obj.GetFieldOrProperty("outputFile");

            List<string> compilerArgs = (List<string>)obj.GetFieldOrProperty("compilerArgs");
            List<string> inputFiles = (List<string>)obj.GetFieldOrProperty("inputFiles");
            List<string> resourceFiles = (List<string>)obj.GetFieldOrProperty("referenceFiles");

            Assert.AreEqual(expectedCompilerName, compilerName);
            Assert.AreEqual(expectedCompilerInfo, compilerInfo);
            Assert.AreEqual(expectedOutputExtension, outputExtension);
            Assert.AreEqual(expectedOutputFile, outputFile);
            Assert.AreEqual(expectedCacheCount, Directory.GetFiles(configuration.cacheLocation, "*", SearchOption.TopDirectoryOnly).Length);
            Assert.IsTrue(File.Exists("testDir3\\input1.exe"));

            CollectionAssert.AreEqual(expectedCompilerArgs, compilerArgs);
            CollectionAssert.AreEqual(expectedInputFiles, inputFiles);
            CollectionAssert.AreEqual(expectedResourceFiles, resourceFiles);

            Directory.Delete("testDir3", true);
        }
    }
}
