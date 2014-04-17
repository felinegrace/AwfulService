using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Configurator;
using Awful.Configurator.Entity;

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
            IConfigParser configParser = ConfigLoader.getParserInstance(ConfigLoader.ConfigParserFormat.XML);
            List<AwfulTaskConfigBase> configList = configParser.parse();
            if(configList != null)
            {
                foreach (AwfulTaskConfigBase cfg in configList)
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

        private AwfulTask buildTaskFromConfig(AwfulTaskConfigBase config)
        {
            switch(config.type)
            {
                case Enumration.TaskType.FILE_BACKUP:
                    return new AwfulFileBackupTask(config as AwfulFileBackupConfig);
                case Enumration.TaskType.DATABASE_BACKUP:
                    return null;
                default:
                    return null;
            }
        }

        
    }
}
