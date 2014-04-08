using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Awful.Utility;

namespace Awful.Scheduler
{
    public abstract class AwfulTask
    {
        public AwfulIdentifier identifier;
        public DateTime scheduledTime { set; get; }
        internal AwfulTaskObserver observer { set; private get; }

        internal TimeSpan respawnTime;
        internal bool isRespawnable;
        private Thread thread = null;

        protected AwfulTask(string descriptor)
        {
            this.identifier = new AwfulIdentifier();
            this.identifier.guid = Guid.NewGuid();
            this.identifier.descriptor = descriptor;
            this.scheduledTime = DateTime.Now;
            isRespawnable = false;
        }

        protected AwfulTask(string descriptor, DateTime scheduledTime)
            : this(descriptor)
        {
            this.scheduledTime = scheduledTime;
        }

        protected AwfulTask(string descriptor, DateTime scheduledTime, TimeSpan respawnTime)
            : this(descriptor, scheduledTime)
        {
            if (respawnTime.TotalSeconds < 1.0)
            {
                this.respawnTime = new TimeSpan(0, 0, 1);
            }
            else
            {
                this.respawnTime = respawnTime;
            }
            isRespawnable = true;
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
                Logger.error("task:{0} encountered an error: {1}", transTask.identifier.descriptor , e.Message);
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
            this.observer.onTaskComplete(this.identifier);
        }

        protected abstract void run();
        protected abstract void cancel();
    }
}
