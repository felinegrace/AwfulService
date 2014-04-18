using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Utility;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Awful.Configurator.Entity;

namespace Awful.Scheduler
{
    public class AwfulFileBackupTask : AwfulTask
    {
        private AwfulFileBackupConfig config { get; set; }
        
        //config object ownership should be taken
        public AwfulFileBackupTask(AwfulFileBackupConfig config)
        {
            this.config = config;
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", config.identifier.descriptor, config.lastLaunch);
            var folderPairs = config.srcFolders.Zip(config.dstFolders, (s, d) => new { src = s, dst = d });
            foreach(var pair in folderPairs)
            { 
                DirectoryInfo srcInfo = new DirectoryInfo(pair.src);
                if(!srcInfo.Exists)
                {
                    Logger.debug("source folder {0} not exist , skpping...", pair.src);
                    continue;
                }

                //replace %d macro to yyyymmdd
                string translateDestination = config.replaceMacroOfDate(pair.dst, config.scheduledDateTime);
                Logger.debug("recursive copying {0} to {1}.", pair.src, translateDestination);
                recursiveCopy(config.fileBackupMethod, srcInfo, new DirectoryInfo(translateDestination), null);
            }
            Logger.debug("{0} done.", config.identifier.descriptor);

            config.lastLaunch = config.scheduledDateTime;
        }

        protected override void cancel()
        {
            throw new NotImplementedException();
        }

        public override AwfulTaskConfigBase getConfig()
        {
            return config;
        }

        private void recursiveCopy(Enumration.FileBackupMethod backupType, DirectoryInfo source, DirectoryInfo target, params string[] excludePatterns)
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
                    recursiveCopy(backupType, dir, target.CreateSubdirectory(dir.Name), excludePatterns);
                }
            }

            // Go ahead and copy each file to the target directory
            foreach (FileInfo file in source.GetFiles())
            {
                bool shouldBackup = true;
                if (backupType == Enumration.FileBackupMethod.INCREMENTAL)
                {
                    if (file.LastAccessTime < config.lastLaunch || file.LastAccessTime >= config.scheduledDateTime)
                    {
                        shouldBackup = false;
                    }
                }
                if (shouldBackup)
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
                        file.CopyTo(dstFullName , true);
                    }
                }
            }
        
        }
    }
}
