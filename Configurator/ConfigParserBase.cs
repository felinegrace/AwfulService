using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Utility;
using Awful.Configurator.Entity;
using System.IO;

namespace Awful.Configurator
{
    abstract class ConfigParserBase : IConfigParser
    {

        public List<AwfulTaskConfigBase> parse()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string defaultFileName = "awful.config.xml";
            return parse(Path.Combine(baseDir, defaultFileName));
        }

        public List<AwfulTaskConfigBase> parse(string fileFullName)
        {
            //all config error will be caught here
            //a null list returned.
            try
            {
                List<AwfulTaskConfigBase> taskConfigList = new List<AwfulTaskConfigBase>();
                parseConfigFile(fileFullName , taskConfigList);
                return taskConfigList;
            }
            catch(ConfigException cfgException)
            {
                Logger.error("Configurator: invalid config: {0}", cfgException.Message);
                return null;
            }
            catch(Exception exception)
            {
                Logger.error("Configurator: error in config file(s) : {0}", exception.Message);
                return null;
            }
        }

        protected abstract void parseConfigFile(string fileFullName, List<AwfulTaskConfigBase> taskConfigList);

    }
}
