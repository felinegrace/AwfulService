using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Awful.Utility;
using Awful.Configurator;
using Awful.Configurator.Entity;

namespace Awful.Scheduler
{
    //crash me if any exception thrown
    public class AwfulScheduler : AwfulTaskObserver
    {
        private Thread thread = null;
        private AutoResetEvent terminateEvent = null;
        private int peekInterval;
        private static int defaultPeekInterval = 30000;
        //WARNING: priority queue is not a thread safe implemention
        private PriorityQueue<DateTime , AwfulTask> pendingTask = null;
        private object priorityQueueLock = null;

        private ConcurrentDictionary<Guid, AwfulTask> runningTask = null;
        private ConcurrentQueue<AwfulTask> finishedTask = null;

        private TaskBuilder taskBuilder = null;
        public AwfulScheduler()
            : this(defaultPeekInterval)
        {
            
        }

        public AwfulScheduler(int peekInterval)
        {
            this.peekInterval = peekInterval;

            pendingTask = new PriorityQueue<DateTime, AwfulTask>();
            priorityQueueLock = new object();

            runningTask = new ConcurrentDictionary<Guid, AwfulTask>();
            finishedTask = new ConcurrentQueue<AwfulTask>();

            terminateEvent = new AutoResetEvent(false);

            taskBuilder = new TaskBuilder();
            taskBuilder.rebuild();
            foreach (AwfulTask t in taskBuilder.taskList)
            {
                prepareTask(t);
            }
        }

        public void start()
        {
            thread = new Thread(AwfulScheduler.invokeScheduler);
            thread.Start(this);
        }

        public void stop()
        {
            terminateEvent.Set();
            foreach(KeyValuePair<Guid,AwfulTask> pair in runningTask)
            {
                pair.Value.stop();
            }
        }

        private static void invokeScheduler(object scheduler)
        {
            AwfulScheduler awfulScheduler = scheduler as AwfulScheduler;
            awfulScheduler.run();
        }

        private void run()
        {
            Logger.debug("AwfulScheduler: start.");
            while (!terminateEvent.WaitOne(peekInterval))
            {
                taskDispose();
                AwfulTask task = taskPeek();
                if (task != null)
                {
                    taskLaunch(task);
                };
            }
            Logger.debug("AwfulScheduler: stop.");
        }

        public void prepareTask(AwfulTask task)
        {
            if (contains(task))
            {
                Logger.debug("AwfulScheduler: duplicated new task : {0} , it will be launched after {1}",
                    task.getConfig().identifier.descriptor,
                    task.getConfig().scheduledDateTime);
                return;
            }
            else
            {
                task.observer = this;
                lock (priorityQueueLock)
                {
                    pendingTask.enqueue(task.getConfig().scheduledDateTime, task);
                }
                Logger.debug("AwfulScheduler: accept new task : {0} , it will be launched after {1}",
                    task.getConfig().identifier.descriptor,
                    task.getConfig().scheduledDateTime);
            }
        }

        private AwfulTask taskPeek()
        {
            Logger.debug("AwfulScheduler: peeking task...");
            lock (priorityQueueLock)
            {
                if (pendingTask.empty() == false)
                {
                    if (pendingTask.peek().getConfig().scheduledDateTime < DateTime.Now)
                        return pendingTask.dequeue();
                }
            }
            return null;
        }

        private void taskDispose()
        {
            if (finishedTask.Count == 0)
            {
                return;
            }
            Logger.debug("AwfulScheduler: disposing task...");
            while(finishedTask.Count > 0)
            {
                AwfulTask task = null;
                if (finishedTask.TryDequeue(out task) == true)
                {
                    Logger.debug("AwfulScheduler: task {0} disposed.", task.getConfig().identifier.descriptor);
                    if (task.getConfig().isRespawnable())
                    {
                        DateTime nextRespawn = task.getConfig().scheduledDateTime;
                        
                        switch(task.getConfig().respawnSpan)
                        {
                            //monthly increase depends on current month
                            // muti whiles cannot be extracted.
                            case Enumeration.RespawnSpanType.MONTHLY:
                                while (nextRespawn <= DateTime.Now)
                                {
                                    nextRespawn += new TimeSpan(DateTime.DaysInMonth(nextRespawn.Year , nextRespawn.Month), 0, 0, 0);
                                }
                                break;
                            case Enumeration.RespawnSpanType.WEEKLY:
                                while (nextRespawn <= DateTime.Now)
                                {
                                    nextRespawn += new TimeSpan( 7 , 0 , 0 , 0);
                                }
                                break;
                            case Enumeration.RespawnSpanType.DAYLY:
                                while (nextRespawn <= DateTime.Now)
                                {
                                    nextRespawn += new TimeSpan( 1 , 0 , 0 , 0);
                                }
                                break;
                            case Enumeration.RespawnSpanType.MINUTELY:
                                while (nextRespawn <= DateTime.Now)
                                {
                                    nextRespawn += new TimeSpan(0, 0, 1, 0);
                                }
                                break;
                        }
                        task.getConfig().scheduledDateTime = nextRespawn;
                        prepareTask(task);
                        Logger.debug("AwfulScheduler: task {0} respawned, next launch will be at {1}.", 
                            task.getConfig().identifier.descriptor,
                            nextRespawn);
                    }
                }
                else
                {
                    Logger.error("AwfulScheduler: cannot remove task from finished list. that's impossible!!!");
                }
            }
            Logger.debug("AwfulScheduler: dispose task complete.");
        }

        private void taskLaunch(AwfulTask task)
        {
            Logger.debug("AwfulScheduler: launching task : {0}", task.getConfig().identifier.descriptor);
            if (runningTask.TryAdd(task.getConfig().identifier.guid, task) == true)
            {
                task.start();
                Logger.debug("AwfulScheduler: task {0} launched.", task.getConfig().identifier.descriptor);
            }
            else
            {
                Logger.error("AwfulScheduler: task {0} cannot be added into running list. a task with same Guid:{1} may already exists.",
                task.getConfig().identifier.descriptor,
                task.getConfig().identifier.guid);
            }
        }

        public void onTaskComplete(Identifier identifier)
        {

            Logger.debug("AwfulScheduler: on task {0} complete.", identifier.descriptor);
            if (runningTask.ContainsKey(identifier.guid))
            {
                AwfulTask task = null;
                if (runningTask.TryRemove(identifier.guid, out task) == false)
                {
                    Logger.error("AwfulScheduler: task guid {0} remove failed from running list.", identifier.guid);
                }
                finishedTask.Enqueue(task);
                Logger.debug("AwfulScheduler: task {0} done.", identifier.descriptor);
            }
            else
            {
                Logger.debug("AwfulScheduler: an unobservable task {0} reports Complete..", identifier.descriptor);
            }
        }

        private bool contains(AwfulTask task)
        {
            lock (priorityQueueLock)
            {
                return
                    pendingTask.containsPair(task.getConfig().scheduledDateTime, task)
                    || runningTask.ContainsKey(task.getConfig().identifier.guid)
                    || finishedTask.Contains(task);
            }
        }
    }
}
