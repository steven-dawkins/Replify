using Replify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinuousDataIntegrityChecking.Commands
{    
    public class ScriptCommand
    {
        private readonly ClearScriptRepl repl;

        public ScriptCommand(ClearScriptRepl repl)
        {
            this.repl = repl;
        }

        private IEnumerable<string> GetScriptFiles()
        {
            return Directory.EnumerateFiles("Scripts", "*.js");
        }

        public void List()
        {
            int i = 0;
            foreach (var scriptFile in GetScriptFiles())
            {
                Console.WriteLine("{0} - {1}", i++, scriptFile);
            }
        }

        public void Execute(int index)
        {
            var filename = GetScriptFiles().ElementAt(index);
            
            Execute(filename);
        }

        public void Execute(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Unable to load script file: {0}", filename);
            }

            try
            {
                this.repl.Execute(File.ReadAllText(filename));
            }            
            catch(Exception e)
            {
                Console.WriteLine("Error running script: {0}", e.Message);
            }
        }
    }
}
