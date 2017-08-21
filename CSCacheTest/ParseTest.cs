using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSCacheLib;
using System.IO;

namespace CSCacheTest
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void TestParseArguments()
        {
            string[] expectedOutput = { "mcs", "input with doubleQuotes.cs", "-arg", "input with single quotes.cs" };
            string input = "mcs \"input with doubleQuotes.cs\" -arg 'input with single quotes.cs'";

            string[] result = ParseTools.ParseArguments(input);

            CollectionAssert.AreEqual(expectedOutput, result);
        }
        [TestMethod]
        public void TestGenerateOption()
        {
            string expectedResult = "t|version|test|hello";
            string[] input = { "-t:", "--version", "--test=", "--hello:" };

            string actualResult = ParseTools.generateOption(input);

            Assert.AreEqual(expectedResult, actualResult);
        }
        [TestMethod]
        public void TestParseResponseFile()
        {
            string[] fileInput = { "-arg1 -arg2 'input1 input2.cs'", "-arg3", "input4.cs input5.cs --arg4" };
            string[] expectedOutput = { "start", "-arg1", "-arg2", "input1 input2.cs", "-arg3", "input4.cs", "input5.cs", "--arg4", "end" };
            string[] arrayWithResponse = { "start", "@testfolder/testresponse.txt", "end" };

            Directory.CreateDirectory("testfolder");
            File.WriteAllLines("testfolder/testresponse.txt", fileInput);

            string[] actualResult = ParseTools.ParseArgArrayWithResponse(arrayWithResponse);

            CollectionAssert.AreEqual(expectedOutput, actualResult);

            Directory.Delete("testfolder", true);
        }
        [TestMethod]
        public void TestParseComaSemicolon()
        {
            string input = "reference1.dll;reference2.dll,ref3,ref5,ref4";
            string[] expectedOutput = { "reference1.dll", "reference2.dll", "ref3", "ref5", "ref4" };

            string[] actualOutput = ParseTools.ParseComaSemicolon(input);

            CollectionAssert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
