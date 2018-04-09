using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApprovalTests.Reporters;
using System.Text;
using System.IO;
using ApprovalTests;

namespace Replify.Tests
{
    [UseReporter(typeof(DiffReporter), typeof(ClipboardReporter))]
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHelp()
        {
            var output = new StringBuilder();
            var writer = new StringWriter(output);
            var repl = new ClearScriptRepl(writer);

            repl.Execute("help");

            Approvals.Verify(output);            
        }

        [TestMethod]
        public void TestAssignment()
        {
            var output = new StringBuilder();
            var writer = new StringWriter(output);
            var repl = new ClearScriptRepl(writer);

            repl.Execute("var x; x = { y: 5 };");

            Approvals.Verify(output);
        }
    }
}
