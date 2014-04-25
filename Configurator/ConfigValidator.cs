using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Configurator.Entity;
using System.Text.RegularExpressions;

namespace Awful.Configurator
{
    class ConfigValidator
    {

        internal static void validateName(string configName)
        {
            if(configName == null || configName.Equals(""))
            {
                throw new ConfigException("must specify a task name.");
            }
        }

        internal static Enumeration.FileBackupMethod validateAndParseFileBackupMethod(string backupMethod)
        {
            if (backupMethod == null || backupMethod.Equals(""))
            {
                throw new ConfigException("must specify a backup method.");
            }
            switch (backupMethod)
            {
                case ("incremental"):
                    return Enumeration.FileBackupMethod.INCREMENTAL;
                case ("full"):
                    return Enumeration.FileBackupMethod.FULL;
                default:
                    throw new ConfigException("file backup method must be \"incremental\" or \"full\". ");
            }
        }

        internal static string validateAndParseDatabaseNameFromConnectionString(string connectionString)
        {
            if (connectionString == null || connectionString.Equals(""))
            {
                throw new ConfigException("no connection string set.");
            }
            Match match = Regex.Match(connectionString, "Database=(?<DBNAME>\\w+);", RegexOptions.IgnoreCase);
            if (match.Success == true)
            {
                return match.Groups["DBNAME"].Value;
            }
            else
            {
                throw new ConfigException("cannot find database field from connection string.");
            }
        }

        internal static Enumeration.DatabaseBackupMethod validateAndParseDatabaseBackupMethod(string backupMethod)
        {
            if (backupMethod == null || backupMethod.Equals(""))
            {
                throw new ConfigException("must specify a databse backup method.");
            }
            switch (backupMethod)
            {
                case ("differential"):
                    return Enumeration.DatabaseBackupMethod.DIFFERENTIAL;
                case ("full"):
                    return Enumeration.DatabaseBackupMethod.FULL;
                case ("composite"):
                    return Enumeration.DatabaseBackupMethod.COMPOSITE;
                default:
                    throw new ConfigException("database backup method must be \"composite\", \"differential\" or \"full\". ");
            }
        }

        internal static Enumeration.RespawnSpanType validateAndParseRespawnSpan(string configRespawnSpan)
        {
            if(configRespawnSpan == null || configRespawnSpan.Equals(""))
            {
                throw new ConfigException("must specify a respawn span.");
            }
            switch(configRespawnSpan)
            {
                case ("monthly"):
                    return Enumeration.RespawnSpanType.MONTHLY;
                case ("weekly"):
                    return Enumeration.RespawnSpanType.WEEKLY;
                case ("dayly"):
                    return Enumeration.RespawnSpanType.DAYLY;
                case ("minutely"):
                    return Enumeration.RespawnSpanType.MINUTELY;
                case ("once"):
                    return Enumeration.RespawnSpanType.ONCE;
                default:
                    throw new ConfigException("respawnSpan must be \"monthly\" , \"weekly\" , \"dayly\" or \"once\". ");
            }
        }

        internal static DateTime validateAndParseLaunchDateTime(string configDateTime)
        {
            try
            {   
                DateTime transDateTime = Convert.ToDateTime(configDateTime);
                return transDateTime;
            }
            catch(FormatException fException)
            {
                throw new ConfigException("invalid launch datetime: "+fException.Message);
            }
        }

        internal static int validateAndParseCompositeDatabaseBackupMethodAlternative(string alternative)
        {
            if (alternative == null || alternative.Equals(""))
            {
                throw new ConfigException("must specify an alternative for composite method.");
            }
            int val;
            try
            {
                val = Convert.ToInt32(alternative);
            }
            catch (Exception e)
            {
                throw new ConfigException("composite method alternative must be an interger:" + e.Message);
            }
            return val;
        }

        internal static TimeSpan validateAndParseCleanExpiration(string expiration)
        {
            if (expiration == null || expiration.Equals(""))
            {
                throw new ConfigException("must specify an expiration for clean task.");
            }
            TimeSpan expirationOut;
            if (TimeSpan.TryParse(expiration, out expirationOut))
            {
                return expirationOut;
            }
            else
            {
                throw new ConfigException("invalid expiration for clean task.");
            }

        }

        internal static bool validateAndParseDeleteEmptyFolder(string delete)
        {
            if (delete == null || delete.Equals(""))
            {
                throw new ConfigException("must specify if empty folders should be deleted.");
            }
            if (delete.Equals("true"))
            {
                return true;
            }
            else if (delete.Equals("false"))
            {
                return false;
            }
            else
            {
                throw new ConfigException("invalid deleteEmptyFolder for clean task.");
            }
        }
    }
}
