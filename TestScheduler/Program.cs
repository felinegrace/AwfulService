using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Scheduler;
using Awful.Utility;
using Awful.Configurator.Entity;
using System.Threading;
namespace TestScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.enable();
            AwfulScheduler s = new AwfulScheduler();
            /*
            for (int a = 0; a < 100; a++)
            {
                AwfulFileBackupConfig c = new AwfulFileBackupConfig();
                c.scheduledDateTime = DateTime.Now;
                c.respawnSpan = Enumeration.RespawnSpanType.MINUTELY;
                c.srcFolders = new List<string>();
                c.dstFolders = new List<string>();
                c.identifier.descriptor = a.ToString();
                c.identifier.guid = Guid.NewGuid();
                AwfulFileBackupTask t = new AwfulFileBackupTask(c);
                Thread.Sleep(100);
                s.prepareTask(t);
                if (a == 0)
                    s.prepareTask(t);
            }
            */

            ConsoleKeyInfo ch;
            do
            {
                ch = Console.ReadKey();
                switch(ch.Key)
                {
                    case ConsoleKey.S:
                        s.start();
                        break;
                    case ConsoleKey.T:
                        s.stop();
                        break;
                }
            } while (ch.Key != ConsoleKey.Q);
        }
    }
    

}
