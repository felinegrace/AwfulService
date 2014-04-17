using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Awful.Utility;
using Awful.Configurator.Entity;

namespace Awful.Scheduler
{
    public abstract class AwfulTask
    {
        //public AwfulIdentifier identifier;
        //public DateTime scheduledTime { set; get; }
        internal AwfulTaskObserver observer { set; private get; }

        ////internal TimeSpan respawnTime;
        //internal TaskConfig.RespawnSpanType respawnTime2;

        //internal bool isRespawnable;

        private Thread thread = null;

        protected AwfulTask()
        {
        }

        internal static void invokeTask(object task)
        {
            AwfulTask transTask = task as AwfulTask;
            try
            {
                transTask.run();
                transTask.complete();
            }
            catch (Exception e)
            {
                Logger.error("task:{0} encountered an error: {1}", transTask.getConfig().identifier.descriptor , e.Message);
            }
        }

        internal void start()
        {
            thread = new Thread(AwfulTask.invokeTask);
            thread.Start(this);
        }

        internal void stop()
        {
            cancel();
        }

        private void complete()
        {
            this.observer.onTaskComplete(this.getConfig().identifier);
        }

        protected abstract void run();
        protected abstract void cancel();
        public abstract AwfulTaskConfigBase getConfig();
    }
}
