using Replify.CommandLine.Commands;
using System;
using System.IO;

namespace Replify.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            // declare REPL, this will reflect the calling assembly identifying any classes which implement IReplCommand
            var repl = new ClearScriptRepl();

            // import underscore, you can't write anything useful in javascript without underscore
            repl.Execute(File.ReadAllText("Scripts/underscore-min.js"));

            // import System.Console so we can write stuff to the console
            repl.AddHostType("Console", typeof(Console));

            repl.AddHostObject("Generate2", new GenerateCommand());                  

            // start the REPL loop, this will exit gracefully when the user quits
            repl.StartReplLoop(args);
        }
    }
}
