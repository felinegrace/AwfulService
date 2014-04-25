using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator
{
    public class ConfigLoader
    {
        public enum ConfigParserFormat
        {
            XML,
            JSON
        }

        public static IConfigParser getParserInstance(ConfigParserFormat format)
        {
            string typeName = "Awful.Configurator.ConfigParser";
            switch(format)
            {
                case ConfigParserFormat.XML:
                    typeName += "XML";
                    break;
                case ConfigParserFormat.JSON:
                    typeName += "JSON";
                    break;
                default:
                    throw new ConfigException("Configurator: unsopported config file format.");
            }
            return (IConfigParser)Activator.CreateInstance(Type.GetType(typeName));
        }
    }
}
