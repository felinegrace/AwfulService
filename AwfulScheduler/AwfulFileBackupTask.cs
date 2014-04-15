using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Utility;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Awful.Scheduler
{
    public class AwfulFileBackupTask : AwfulTask
    {
        private DateTime lastLaunch;
        public List<string> srcFolders { get; set; }
        public List<string> dstFolders { get; set; }
        protected void setFolders(List<string> srcFolders, List<string> dstFolders)
        {
            this.srcFolders = srcFolders;
            this.dstFolders = dstFolders;
        }
        public AwfulFileBackupTask(string descriptor, List<string> srcFolders, List<string> dstFolders)
            : base(descriptor)
        {
            this.lastLaunch = new DateTime(0);
            setFolders(srcFolders , dstFolders);
        }

        public AwfulFileBackupTask(string descriptor, DateTime scheduledTime, List<string> srcFolders, List<string> dstFolders)
            : base(descriptor, scheduledTime)
        {
            setFolders(srcFolders, dstFolders);
        }

        public AwfulFileBackupTask(string descriptor, DateTime scheduledTime, TimeSpan respawnTime, List<string> srcFolders, List<string> dstFolders)
            : base(descriptor, scheduledTime, respawnTime)
        {
            setFolders(srcFolders, dstFolders);
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", identifier.descriptor, lastLaunch);
            var folderPairs = srcFolders.Zip(dstFolders, (s, d) => new { src = s, dst = d });
            foreach(var pair in folderPairs)
            { 
                DirectoryInfo srcInfo = new DirectoryInfo(pair.src);
                if(!srcInfo.Exists)
                {
                    Logger.debug("source folder {0} not exist , skpping...", pair.src);
                    continue;
                }
                Logger.debug("recursive copying {0} to {1}.", pair.src, pair.dst);
                recursiveCopy(srcInfo, new DirectoryInfo(pair.dst), null);
            }
            Logger.debug("{0} done." , identifier.descriptor);
            
            lastLaunch = this.scheduledTime;
        }

        protected override void cancel()
        {
            throw new NotImplementedException();
        }

        private void recursiveCopy(DirectoryInfo source, DirectoryInfo target, params string[] excludePatterns)
        {
            if (target.FullName.Contains(source.FullName))
                return;
            if(!target.Exists)
            {
                target.Create();
            }
            // Go through the Directories and recursively call the DeepCopy Method for each one
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                var dirName = dir.Name;
                var shouldExclude = false;
                if (excludePatterns != null)
                {
                    shouldExclude = excludePatterns.Aggregate(
                        false, (current, pattern) => current
                            || Regex.Match(dirName, pattern).Success);
                }
                if (!shouldExclude)
                {
                    Logger.debug("recursive copying subdir {0}.", dirName);
                    recursiveCopy(dir, target.CreateSubdirectory(dir.Name), excludePatterns);
                }
            }

            // Go ahead and copy each file to the target directory
            foreach (FileInfo file in source.GetFiles())
            {
                if(file.LastAccessTime >= lastLaunch && file.LastAccessTime < scheduledTime)
                {
                    var fileName = file.Name;
                    var shouldExclude = false;
                    if (excludePatterns != null)
                    {
                        shouldExclude = excludePatterns.Aggregate(false,
                                                                   (current, pattern) =>
                                                                   current || Regex.Match(fileName, pattern).Success);
                    }
                    if (!shouldExclude)
                    {
                        string dstFullName = Path.Combine(target.FullName, fileName);
                        Logger.debug("copying file {0} to {1}.", file.FullName, dstFullName);
                        file.CopyTo(dstFullName);
                    }
                }
            }
        
        }
    }
}
