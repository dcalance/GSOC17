using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSCacheLib;

namespace CSCacheTest
{
    [TestClass]
    public class ConsoleTest
    {
        [TestMethod]
        public void TestExecute()
        {
            //these tests are on windows and the execute method is using cmd
            int error;
            int expectedError = 1;
            string expectedMessage = "error CS2008: No files to compile were specified\nCompilation failed: 1 error(s), 0 warnings";
            string passedCommand = "mcs";

            string resultMessage = ConsoleTools.Execute(passedCommand, out error);
            Assert.AreEqual(expectedError, error);
            Assert.AreEqual(expectedMessage, resultMessage);

            expectedError = 0;
            expectedMessage = "";
            passedCommand = "cd %appdata%";

            resultMessage = ConsoleTools.Execute(passedCommand, out error);
            Assert.AreEqual(expectedError, error);
            Assert.AreEqual(expectedMessage, resultMessage);

            Assert.IsFalse(ConsoleTools.IsUnix);
        }

    }
}
