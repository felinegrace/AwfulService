using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Configurator;

namespace Awful.Scheduler
{
    class Taskbuilder
    {
        public List<AwfulTask> taskList { get; set; }
        private TaskConfig taskConfig { get; set; }
        public Taskbuilder(List<TaskConfig> configList)
        {
            taskList = new List<AwfulTask>();
            foreach(TaskConfig cfg in configList)
            {
                AwfulTask task = buildTaskFromConfig(cfg);
                if(task != null)
                {
                    taskList.Add(task);
                }
            }
        }

        private AwfulTask buildTaskFromConfig(TaskConfig config)
        {
            AwfulTask task = null;
            if(config.respawnSpan == "dayly")
            {
                task = new AwfulFileBackupTask(
                    config.name,
                    config.launchDateTime,
                    new TimeSpan(24 , 0 , 0),
                    config.srcFolders,
                    config.dstFolders
                    );
            }
            return task;
        }
    }
}
