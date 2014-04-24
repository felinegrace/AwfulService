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
       

        void performBackup(Enumeration.DatabaseBackupMethod method)
        {
            methodDependentPreparation();

            //replace %d macro to yyyymmdd
            string translateDestination = methodDependentDstFileName();
            string translateDestinationTemp = translateDestination + ".awf";

            //methodDependentSql should be called after methodDependentDstFileName
            //bad smell here
            //adding methodDependentPreparation solves it 
            string backupMethod = methodDependentSql();

            string sql = string.Format("BACKUP DATABASE {0} TO disk='{1}' WITH INIT {2}",
                config.databaseName, translateDestinationTemp, backupMethod);
            Logger.debug("{0} executing: {1}.", config.identifier.descriptor, sql);

            SqlConnection conn = new SqlConnection(config.databaseConnectionString);

            command.CommandType = CommandType.Text;
            command.Connection = conn;
            command.CommandText = sql;
            
            try
            {
                string backupDirectory = Path.GetDirectoryName(translateDestinationTemp);
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
            
            
            Logger.debug("renaming file {0} to {1}", translateDestinationTemp, translateDestination);
            if (File.Exists(translateDestination))
            {
                Logger.debug("try delete exsiting file {0}", translateDestination);
                System.IO.File.Delete(translateDestination);

            }
            new FileInfo(translateDestinationTemp).MoveTo(translateDestination);
            Logger.debug("backup file {0} done.", translateDestination);

        }

        protected virtual void methodDependentPreparation()
        {
            return;
        }

        protected string methodDependentSqlBySpecifiedMethod(Enumeration.DatabaseBackupMethod method)
        {
            switch (method)
            {
                case Enumeration.DatabaseBackupMethod.DIFFERENTIAL:
                    return ", DIFFERENTIAL";
                case Enumeration.DatabaseBackupMethod.FULL:
                    return "";
                 default:
                    {
                        Logger.error("task {0} reaches an untouchable method. crash this task.",config.identifier.descriptor);
                        throw new Exception("critical task error.");
                    }
            }
        }

        protected virtual string methodDependentSql()
        {
            return methodDependentSqlBySpecifiedMethod(config.databaseBackupMethod);
        }

        protected virtual string methodDependentDstFileName()
        {
            return config.replaceMacroOfDate(config.dstFile, config.scheduledDateTime);
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", config.identifier.descriptor, config.lastLaunch);

            performBackup(config.databaseBackupMethod);

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
