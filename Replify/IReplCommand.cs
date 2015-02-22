using System;
using System.Linq;
using System.Text;

namespace Replify
{
    public abstract class IReplCommand
    {
        public virtual string Name
        {
            get
            {
                var name = this.GetType().Name;
                return Utilities.ConvertCommandName(name);
            }
        }
        
        public string Help()
        {
            return HelpManager.GetHelpInfo(this);
        }        
    }
}
