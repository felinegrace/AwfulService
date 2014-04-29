using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Awful.Utility;
using Awful.Scheduler;

namespace Awful.WindowsService
{
    class ServiceEntrance : System.ServiceProcess.ServiceBase
    {
        private AwfulScheduler scheduler = null;

        public ServiceEntrance()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.AutoLog = false;
            this.ServiceName = "Awful Backup Service";
            Logger.enable();
        }

        protected override void OnStart(string[] args)
        {
            Logger.info("ServiceEntrance: starting...");
            Logger.info("ServiceEntrance: working dir is {0}", AppDomain.CurrentDomain.BaseDirectory);
            scheduler = new AwfulScheduler();
            scheduler.start();
            Logger.info("ServiceEntrance: start.");
        }

        protected override void OnStop()
        {
            Logger.info("ServiceEntrance: stopping...");
            scheduler.stop();
            Logger.info("ServiceEntrance: stop.");
        }
    }
}
