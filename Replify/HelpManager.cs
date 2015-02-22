using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replify
{
    public class HelpManager
    {
        public static string GetHelpInfo(Object obj)
        {
            string[] commandsToIgnore = new[] { "Equals", "GetHashCode", "GetType", "ToString", "Help", "get_Name" };

            var result = new StringBuilder();

            var type = obj.GetType();

            if (type.GetMethods().Any())
            {
                result.AppendFormat("Methods:\n");
                foreach (var info in obj.GetType().GetMethods())
                {
                    if (!commandsToIgnore.Contains(info.Name))
                    {
                        var parameters = from p in info.GetParameters()
                                         select p.Name + ":" + p.ParameterType;

                        result.AppendFormat("{0}({1})\n", info.Name, String.Join(", ", parameters));
                    }
                }
            }

            if (type.GetProperties().Any())
            {
                result.AppendFormat("Properties:\n");
                foreach (var info in type.GetProperties())
                {                                            
                    result.AppendFormat("{0}\n", info.Name);                    
                }
            }

            if (type.GetFields().Any())
            {
                result.AppendFormat("Fields:\n");
                foreach (var info in type.GetFields())
                {
                    result.AppendFormat("{0}\n", info.Name);
                }
            }                       

            return result.ToString();
        }
    }
}
