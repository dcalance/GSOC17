using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSCacheLib;
using System.IO;

namespace CSCacheTest
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void TestProcessArray()
        {
            string passingStr = "a, b,  c, d,";
            string[] expectedRes = { "a", "b", "c", "d" };

            Config conf = new Config();
            PrivateObject obj = new PrivateObject(conf);

            string[] result = (string[])obj.Invoke("ProcessArray", passingStr);
            CollectionAssert.AreEqual(expectedRes, result);

            passingStr = "a,,b,c,,d,e";
            expectedRes = new string[] { "a", "b", "c", "d", "e" };
            result = (string[])obj.Invoke("ProcessArray", passingStr);

            CollectionAssert.AreEqual(expectedRes, result);
        }
        [TestMethod]
        public void TestProcessPath()
        {
            //these tests are valid for windows

            string path = "%appdata%\\.cscache";
            string expectedPath = $"{Environment.GetEnvironmentVariable("appdata")}\\.cscache";

            Config conf = new Config();
            PrivateObject obj = new PrivateObject(conf);

            string result = (string)obj.Invoke("ProcessPath", path);
            Assert.AreEqual(expectedPath, result);

            path = @"$home\.cscache";
            expectedPath = @"$home\.cscache";
            result = (string)obj.Invoke("ProcessPath", path);
            Assert.AreEqual(expectedPath, result);

            path = "C:\\users";
            expectedPath = "C:\\users";
            result = (string)obj.Invoke("ProcessPath", path);
            Assert.AreEqual(expectedPath, result);
        }
        [TestMethod]
        public void TestConfigClass()
        {
            string expectedCacheLocation = (ConsoleTools.IsUnix) ? Environment.GetEnvironmentVariable("HOME") + @"/.cscache/"
                                   : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.cscache\";
            string[] expectedIgnoredArg = { "--stacktrace", "--timestamp", "-v"};
            string[] expectedVersionArg = { "--version" };
            string[] expectedResourcesArg = { "-r:" };
            string[] expectedOutputArg = { "-out:" };
            string[] expectedTargetArg = { "-t:" };
            string[] expectedRecurseArg = { "-recurse:" };

            File.Delete(expectedCacheLocation + "config.xml");
            Config conf = new Config();

            Assert.AreEqual(expectedCacheLocation, conf.cacheLocation);
            CollectionAssert.AreEqual(expectedIgnoredArg, conf.ignoredArg);
            CollectionAssert.AreEqual(expectedVersionArg, conf.versionArg);
            CollectionAssert.AreEqual(expectedResourcesArg, conf.resourcesArg);
            CollectionAssert.AreEqual(expectedOutputArg, conf.outputArg);
            CollectionAssert.AreEqual(expectedTargetArg, conf.targetArg);
            CollectionAssert.AreEqual(expectedRecurseArg, conf.recurseArg);

            Assert.IsTrue(File.Exists(expectedCacheLocation + "config.xml"));
        }
    }
}
