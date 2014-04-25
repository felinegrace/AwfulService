using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class AwfulDatabaseCompositeBackupConfig : AwfulDatabaseBackupConfig
    {
        public AwfulDatabaseCompositeBackupConfig()
        {
            this.type = Enumeration.TaskType.DATABASE_BACKUP;
            this.businussObject = Type.GetType("Awful.Scheduler.AwfulDatabaseCompositeBackupTask,Awful.Scheduler");
        }
        public int methodAlternative { get; set; }

    }
}
