using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Utility;
using System.Threading;

namespace Awful.Scheduler
{
    public class AwfulFileBackupTask : AwfulTask
    {
        public AwfulFileBackupTask(string descriptor)
            : base(descriptor)
        {
           
        }

        public AwfulFileBackupTask(string descriptor, DateTime scheduledTime)
            : base(descriptor, scheduledTime)
        {

        }

        public AwfulFileBackupTask(string descriptor, DateTime scheduledTime, TimeSpan respawnTime)
            : base(descriptor, scheduledTime, respawnTime)
        {

        }

        protected override void run()
        {
            Logger.debug("{0} running" , identifier.descriptor);
            Thread.Sleep(20000);
        }

        protected override void cancel()
        {
            throw new NotImplementedException();
        }
    }
}
