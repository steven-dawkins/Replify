using Mandoline.CL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replify.CommandLine.Commands
{
    // REPL commands are identified by implementing IReplCommand
    public class GenerateCommand : IReplCommand
    {
        // All public methods will be callable and identified in the command help
        public double[] Floats(int count)
        {            
            var rand = new Random();

            var results = from i in Enumerable.Range(0, count)
                          select rand.NextDouble();
            
            return results.ToArray();
        }
    }
}
