using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Utility;
using Awful.Configurator.Entity;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Awful.Scheduler
{
   public class AwfulDatabaseBackupTask : AwfulTask
    {
        private AwfulDatabaseBackupConfig config { get; set; }

        private SqlCommand command { get; set; } 
        //config object ownership should be taken
        public AwfulDatabaseBackupTask(AwfulDatabaseBackupConfig config)
        {
            this.config = config;
            command = new SqlCommand();
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", config.identifier.descriptor, config.lastLaunch);

            
            //replace %d macro to yyyymmdd
            string translateDestination = config.replaceMacroOfDate(config.dstFile, config.scheduledDateTime);

            string backupMethod = null;
            switch(config.databaseBackupMethod)
            {
                case Enumration.DatabaseBackupMethod.DIFFERENTIAL:
                    backupMethod = ", DIFFERENTIAL";
                    break;
                case Enumration.DatabaseBackupMethod.FULL:
                    backupMethod = "";
                    break;
            }

            string sql = string.Format("BACKUP DATABASE {0} TO disk='{1}' WITH INIT {2}", 
                config.databaseName, translateDestination , backupMethod);
            Logger.debug("{0} executing: {1}.", config.identifier.descriptor, sql);

            SqlConnection conn = new SqlConnection(config.databaseConnectionString);

            command.CommandType = CommandType.Text;
            command.Connection = conn;
            command.CommandText = sql;
            
            try
            {
                string backupDirectory = Path.GetDirectoryName(translateDestination);
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }
                conn.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            Logger.debug("{0} successfully executed: {1}.", config.identifier.descriptor, sql);
        }

        protected override void cancel()
        {
            command.Cancel();
        }

        public override AwfulTaskConfigBase getConfig()
        {
            return config;
        }
    }
}
