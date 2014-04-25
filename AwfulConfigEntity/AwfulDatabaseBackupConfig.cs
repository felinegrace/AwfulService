using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class AwfulDatabaseBackupConfig : AwfulTaskConfigBase
    {
        public AwfulDatabaseBackupConfig()
        {
            this.type = Enumeration.TaskType.DATABASE_BACKUP;
            this.businussObject = Type.GetType("Awful.Scheduler.AwfulDatabaseBackupTask,Awful.Scheduler");
        }
        public Enumeration.DatabaseBackupMethod databaseBackupMethod { get; set; }
        public string databaseConnectionString { get; set; }
        public string databaseName { get; set; }
        public string dstFile { get; set; }
    }
}
