using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApprovalTests.Reporters;

namespace Replify.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHelp()
        {
            var repl = new ClearScriptRepl();

            repl.Execute("help");

            //TODO: capture output
        }
    }
}
