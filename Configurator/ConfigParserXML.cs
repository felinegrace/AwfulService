using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Awful.Utility;
using Awful.Configurator.Entity;

namespace Awful.Configurator
{
    class ConfigParserXML : ConfigParserBase
    {

        protected override void parseConfigFile(string fileFullName, List<Entity.AwfulTaskConfigBase> taskConfigList)
        {
            XPathDocument docNav = new XPathDocument(fileFullName);
            XPathNavigator nav = docNav.CreateNavigator();
            XPathNodeIterator iter = nav.Select("/awfulConfig/task");
            while(iter.MoveNext())
            {
                AwfulTaskConfigBase config;
                XPathNavigator tnav = iter.Current;
                string type = tnav.SelectSingleNode("type").Value;
                if(type == null)
                {
                    throw new ConfigException("Configurator: must specify a task type.");
                }
                switch(type)
                {
                    case "file":
                        config = parseXmlObjectAsFileBackupTaskConfig(tnav);
                        break;
                    case "database":
                        config = parseXmlObjectAsDatabaseBackupTaskConfig(tnav);
                        break;
                    case "clean":
                        config = parseXmlObjectAsFileCleanTaskConfig(tnav);
                        break;
                    default:
                        throw new ConfigException("Configurator: task type must be \"file\" or \"database\".");
                }
                if (config != null)
                {
                    taskConfigList.Add(config);
                }

            }
        }

        private void parseXmlObjectCommonItems(AwfulTaskConfigBase config, XPathNavigator xmlConfigObject)
        {
            config.identifier.descriptor = xmlConfigObject.SelectSingleNode("name").Value;
            ConfigValidator.validateName(config.identifier.descriptor);
            config.respawnSpan =
                ConfigValidator.validateAndParseRespawnSpan(
                xmlConfigObject.SelectSingleNode("respawnSpan").Value);
            config.scheduledDateTime =
                ConfigValidator.validateAndParseLaunchDateTime(
                xmlConfigObject.SelectSingleNode("launchDateTime").Value);
        }

        private AwfulTaskConfigBase parseXmlObjectAsFileBackupTaskConfig(XPathNavigator xmlConfigObject)
        {
            AwfulFileBackupConfig config = new AwfulFileBackupConfig();

            parseXmlObjectCommonItems(config, xmlConfigObject);

            config.fileBackupMethod =
                ConfigValidator.validateAndParseFileBackupMethod(
                xmlConfigObject.SelectSingleNode("fileBackupMethod").Value);

            config.srcFolders = new List<string>();
            XPathNodeIterator srcIter = xmlConfigObject.Select("srcFolders");
            while (srcIter.MoveNext())
            {
                config.srcFolders.Add(srcIter.Current.Value);
            }

            config.dstFolders = new List<string>();
            XPathNodeIterator dstIter = xmlConfigObject.Select("dstFolders");
            while (dstIter.MoveNext())
            {
                config.dstFolders.Add(dstIter.Current.Value);
            }

            return config;
        }
        private AwfulTaskConfigBase parseXmlObjectAsDatabaseBackupTaskConfig(XPathNavigator xmlConfigObject)
        {
            AwfulDatabaseBackupConfig config;
            Enumeration.DatabaseBackupMethod method = 
                ConfigValidator.validateAndParseDatabaseBackupMethod(
                xmlConfigObject.SelectSingleNode("databaseBackupMethod").Value);

            switch (method)
            {
                case Enumeration.DatabaseBackupMethod.COMPOSITE:
                    config = new AwfulDatabaseCompositeBackupConfig();
                    config.databaseBackupMethod = method;
                    parseAdditionalCompositeDatabaseBackupTaskConfig(xmlConfigObject, config as AwfulDatabaseCompositeBackupConfig);
                    return config;
                case Enumeration.DatabaseBackupMethod.DIFFERENTIAL:
                case Enumeration.DatabaseBackupMethod.FULL:
                    config = new AwfulDatabaseBackupConfig();
                    config.databaseBackupMethod = method;
                    parseMonoDatabaseBackupTaskConfig(xmlConfigObject, config);
                    return config;
                default:
                    return null;
            }
        }

        private void parseDatabaseBackupTaskCommonItems(XPathNavigator xmlConfigObject , AwfulDatabaseBackupConfig config)
        {
            parseXmlObjectCommonItems(config, xmlConfigObject);

            config.databaseConnectionString = xmlConfigObject.SelectSingleNode("databaseConnectionString").Value;

            config.databaseName = ConfigValidator.validateAndParseDatabaseNameFromConnectionString(config.databaseConnectionString);

            config.dstFile = xmlConfigObject.SelectSingleNode("dstFile").Value;
        }

        private void parseMonoDatabaseBackupTaskConfig(XPathNavigator xmlConfigObject, AwfulDatabaseBackupConfig config)
        {
            parseDatabaseBackupTaskCommonItems(xmlConfigObject, config);
        }

        private void parseAdditionalCompositeDatabaseBackupTaskConfig(XPathNavigator xmlConfigObject, AwfulDatabaseCompositeBackupConfig config)
        {
            parseDatabaseBackupTaskCommonItems(xmlConfigObject, config);

            config.methodAlternative = ConfigValidator.validateAndParseCompositeDatabaseBackupMethodAlternative
                (xmlConfigObject.SelectSingleNode("methodAlternative").Value);

            if(config.methodAlternative < 1)
            {
                throw new ConfigException("Configurator: methodAlternative must lager than 0.");
            }
        }

        private AwfulTaskConfigBase parseXmlObjectAsFileCleanTaskConfig(XPathNavigator xmlConfigObject)
        {
            AwfulFileCleanConfig config = new AwfulFileCleanConfig();

            parseXmlObjectCommonItems(config, xmlConfigObject);

            config.expiration = ConfigValidator.validateAndParseCleanExpiration(xmlConfigObject.SelectSingleNode("expiration").Value);

            config.deleteEmptyFolder = 
                ConfigValidator.validateAndParseDeleteEmptyFolder
                (xmlConfigObject.SelectSingleNode("deleteEmptyFolder").Value);

            config.dstFolders = new List<string>();
            XPathNodeIterator dstIter = xmlConfigObject.Select("dstFolders");
            while (dstIter.MoveNext())
            {
                config.dstFolders.Add(dstIter.Current.Value);
            }

            return config;
        }
    }
}
