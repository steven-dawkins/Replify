using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replify
{
    public static class Utilities
    {
        public static string ConvertCommandName(string name)
        {
            if (name.EndsWith("Command"))
            {
                return name.Substring(0, name.Length - "Command".Length);
            }
            else
            {
                return name;
            }
        }

    }
}
