using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandoline.CL.Commands
{
    public abstract class IReplCommand
    {
        public virtual string Name
        {
            get
            {
                var name = this.GetType().Name;
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

        public string Help()
        {
            string[] commandsToIgnore = new [] { "Equals", "GetHashCode", "GetType", "ToString", "Help", "get_Name" };
            var result = new StringBuilder();
            foreach(var info in this.GetType().GetMethods())
            {
                if (!commandsToIgnore.Contains(info.Name))
                {
                    var parameters = from p in info.GetParameters()
                                     select p.Name + ":" + p.ParameterType;

                    result.AppendFormat("{0}({1})\n", info.Name, String.Join(", ", parameters));
                }
            }

            return result.ToString();
        }
    }
}
