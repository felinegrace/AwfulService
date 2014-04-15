using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Configurator;

namespace Awful.Scheduler
{
    class TaskBuilder
    {
        public List<AwfulTask> taskList { get; private set; }
        public TaskBuilder()
        {
            taskList = new List<AwfulTask>();
        }

        public void build()
        {
            IConfigParser configParser = ConfigLoader.getParserInstance(ConfigLoader.ConfigParserFormat.JSON);
            List<TaskConfig> configList = configParser.parse();
            if(configList != null)
            {
                foreach (TaskConfig cfg in configList)
                {
                    AwfulTask task = buildTaskFromConfig(cfg);
                    if (task != null)
                    {
                        taskList.Add(task);
                    }
                }
            }
            
        }

        public void rebuild()
        {
            taskList.Clear();
            build();
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
