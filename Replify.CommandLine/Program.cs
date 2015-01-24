using Mandoline.CL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // import any javascript script files included in the program args
            if (args.Length >= 1)
            {
                if (File.Exists(args[0]))
                {
                    repl.Execute(File.ReadAllText(args[0]));
                }
                else
                {
                    Console.WriteLine("Unable to launch script: {0}, not found", args[0]);
                }
            }            

            // start the REPL loop, this will exit gracefully when the user quits
            repl.StartReplLoop();
        }
    }
}
