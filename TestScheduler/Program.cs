using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Scheduler;
using Awful.Utility;
using System.Threading;
namespace TestScheduler
{

    

    class Program
    {
        static void Main(string[] args)
        {
            Logger.enable();
            AwfulScheduler s = new AwfulScheduler();
            for (int a = 0; a < 100; a++)
            {
                AwfulFileBackupTask t = new AwfulFileBackupTask(a.ToString(), DateTime.Now, new TimeSpan(0, 0, 10));
                Thread.Sleep(100);
                s.prepareTask(t);
                if(a == 0)
                    s.prepareTask(t);
            }

            s.start();
        }
    }
    

}
