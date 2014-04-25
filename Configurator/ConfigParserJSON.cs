using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Utility;
using Awful.Configurator.Entity;
namespace Awful.Configurator
{
    class ConfigParserJSON : ConfigParserBase
    {
        protected override void parseConfigFile(string fileFullName, List<AwfulTaskConfigBase> taskConfigList)
        {
            throw new NotImplementedException();
            /*
            string jsonConfig = "";
            try
            {
                jsonConfig = File.ReadAllText(fileFullName);
            }
            catch (Exception exception)
            {
                throw new ConfigException("cannot open config file: " + exception.Message);
            }

            JArray configArray = JArray.Parse(jsonConfig);
            foreach (JObject configObject in configArray)
            {
                AwfulTaskConfigBase cfg = parseJsonObject(configObject);
                if (cfg != null)
                {
                    taskConfigList.Add(cfg);
                }
            }
            
            */
        }

        /*
        private AwfulTaskConfigBase parseJsonObject(JObject jsonConfigObject)
        {
            AwfulFileBackupConfig config = new AwfulFileBackupConfig();
                
            string[] configItems = new string[]{   "name",
                                                    "respawnSpan",
                                                    "launchDateTime",
                                                    "backupType",
                                                    "srcFolders",
                                                    "dstFolders",
                                                    
                                                    };
            for(var i = 0 ; i < configItems.Length ; i++)
            {
                if(jsonConfigObject[configItems[i]] == null)
                {
                    throw new ConfigException("Configurator: missing config item:"+configItems[i]);
                }
            }

            
            config.identifier.descriptor = (string)jsonConfigObject["name"];
            ConfigValidator.validateName(config.identifier.descriptor);
            config.fileBackupType = ConfigValidator.validateAndParseBackupType((string)jsonConfigObject["backupType"]);
            config.respawnSpan = ConfigValidator.validateAndParseRespawnSpan((string)jsonConfigObject["respawnSpan"]);
            config.scheduledDateTime = ConfigValidator.validateAndParseLaunchDateTime((string)jsonConfigObject["launchDateTime"]);
            config.srcFolders = ((JArray)jsonConfigObject["srcFolders"]).ToObject<List<string>>();
            config.dstFolders = ((JArray)jsonConfigObject["dstFolders"]).ToObject<List<string>>();
            return config;

        }
        */
    }
}
