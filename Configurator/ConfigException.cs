using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator
{
    class ConfigException : ApplicationException
    {
        public ConfigException(string message)
            : base(message)
        {
            //just play as a message holder
        } 
    }
}
