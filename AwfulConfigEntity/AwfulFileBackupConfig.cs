using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class AwfulFileBackupConfig : AwfulTaskConfigBase
    {
        public AwfulFileBackupConfig()
        {
            this.type = Enumeration.TaskType.FILE_BACKUP;
            this.businussObject = Type.GetType("Awful.Scheduler.AwfulFileBackupTask,Awful.Scheduler");
        }
        public Enumeration.FileBackupMethod fileBackupMethod { get; set; }
        public List<string> srcFolders { get; set; }
        public List<string> dstFolders { get; set; }

    }
}
