using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class AwfulFileCleanConfig : AwfulTaskConfigBase
    {
        public AwfulFileCleanConfig()
        {
            this.type = Enumeration.TaskType.FILE_CLEAN;
            this.businussObject = Type.GetType("Awful.Scheduler.AwfulFileCleanTask,Awful.Scheduler");
        }
        public TimeSpan expiration { get; set; }
        public bool deleteEmptyFolder { get; set; }
        public List<string> dstFolders { get; set; }
    }
}
