using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Awful.Utility;
using Awful.Configurator;

namespace Awful.Scheduler
{
    public class AwfulScheduler : AwfulTaskObserver
    {
        private Thread thread = null;
        private ManualResetEvent terminateEvent = null;
        private int peekInterval;
        private static int defaultPeekInterval = 1000;
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

            thread = new Thread(AwfulScheduler.invokeScheduler);
            terminateEvent = new ManualResetEvent(false);

            taskBuilder = new TaskBuilder();
            taskBuilder.rebuild();
            foreach (AwfulTask t in taskBuilder.taskList)
            {
                prepareTask(t);
            }
        }

        public void start()
        {
            thread.Start(this);
            
        }

        public void stop()
        {
            terminateEvent.Set();
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
                Logger.debug("AwfulScheduler: duplicated new task : {0} , it will be launched after {1:s}",
                    task.identifier.descriptor,
                    task.scheduledTime);
                return;
            }
            else
            {
                task.observer = this;
                lock (priorityQueueLock)
                {
                    pendingTask.enqueue(task.scheduledTime , task);
                }
                Logger.debug("AwfulScheduler: accept new task : {0}", task.identifier.descriptor);
            }
        }

        private AwfulTask taskPeek()
        {
            Logger.debug("AwfulScheduler: peeking task...");
            lock (priorityQueueLock)
            {
                if (pendingTask.empty() == false)
                {
                    if (pendingTask.peek().scheduledTime < DateTime.Now)
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
                    Logger.debug("AwfulScheduler: task {0} disposed.", task.identifier.descriptor);
                    if (task.isRespawnable)
                    {
                        DateTime nextRespawn = task.scheduledTime;
                        while (nextRespawn <= DateTime.Now)
                        {
                            nextRespawn += task.respawnTime;
                        }
                        task.scheduledTime = nextRespawn;
                        prepareTask(task);
                        Logger.debug("AwfulScheduler: task {0} respawned.", task.identifier.descriptor);
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
            Logger.debug("AwfulScheduler: launching task : {0}", task.identifier.descriptor);
            if (runningTask.TryAdd(task.identifier.guid, task) == true)
            {
                task.start();
                Logger.debug("AwfulScheduler: task {0} launched.", task.identifier.descriptor);
            }
            else
            {
                Logger.error("AwfulScheduler: task {0} cannot be added into running list.", task.identifier.descriptor);
            }
        }

        public void onTaskComplete(AwfulIdentifier identifier)
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
                Logger.debug("AwfulScheduler: task {0} finishing...", identifier.descriptor);
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
                    pendingTask.containsPair(task.scheduledTime , task)
                    || runningTask.ContainsKey(task.identifier.guid)
                    || finishedTask.Contains(task);
            }
        }
    }
}
