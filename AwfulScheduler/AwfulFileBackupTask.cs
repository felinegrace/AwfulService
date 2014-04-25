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
        private bool shouldTerminate { get; set; }
        //config object ownership should be taken
        public AwfulFileBackupTask(AwfulFileBackupConfig config)
        {
            this.config = config;
            shouldTerminate = false;
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", config.identifier.descriptor, config.lastLaunch);
            var folderPairs = config.srcFolders.Zip(config.dstFolders, (s, d) => new { src = s, dst = d });
            foreach(var pair in folderPairs)
            {
                if (shouldTerminate)
                {
                    Logger.debug("receive TERMINATE signal, exiting...");
                    shouldTerminate = false;
                    return;
                }
                string translateSource = config.replaceMacroOfDate(pair.src, config.scheduledDateTime);
                DirectoryInfo srcInfo = new DirectoryInfo(translateSource);
                if(!srcInfo.Exists)
                {
                    Logger.debug("source folder {0} not exist , skpping...", translateSource);
                    continue;
                }

                //replace %d macro to yyyymmdd
                string translateDestination = config.replaceMacroOfDate(pair.dst, config.scheduledDateTime);
                Logger.debug("recursive copying {0} to {1}.", translateSource, translateDestination);
                recursiveCopy(srcInfo, new DirectoryInfo(translateDestination), null);
            }
            Logger.debug("{0} done.", config.identifier.descriptor);
        }

        protected override void cancel()
        {
            shouldTerminate = true;
        }

        public override AwfulTaskConfigBase getConfig()
        {
            return config;
        }

        private void recursiveCopy(DirectoryInfo source, DirectoryInfo target, params string[] excludePatterns)
        {
            // 
            if (target.FullName.Contains(source.FullName))
            {
                return;
            }
            if(!target.Exists)
            {
                target.Create();
            }
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (shouldTerminate)
                {
                    Logger.debug("receive TERMINATE signal, exiting...");
                    return;
                }
                var dirName = dir.Name;
                bool shouldExclude = shouldExcludeByPatterns(dirName, excludePatterns);
                if (!shouldExclude)
                {
                    Logger.debug("recursive copying subdir {0}.", dirName);
                    recursiveCopy(dir, target.CreateSubdirectory(dir.Name), excludePatterns);
                }
            }

            foreach (FileInfo file in source.GetFiles())
            {
                if (shouldTerminate)
                {
                    Logger.debug("receive TERMINATE signal, exiting...");
                    return;
                }
                fileCopy(file, target, excludePatterns);
            }
        
        }

        private void fileCopy(FileInfo file, DirectoryInfo target, params string[] excludePatterns)
        {
            var fileName = file.Name;
            bool shouldBackup = shouldBackupOnTheMethod(file);
            bool shouldExclude = shouldExcludeByPatterns(fileName , excludePatterns);
            if (shouldBackup && !shouldExclude)
            {
                string dstFullName = Path.Combine(target.FullName, fileName);
                string dstFullNameTemp = dstFullName + ".awf";
                Logger.debug("copying file {0} to {1}.", file.FullName, dstFullNameTemp);
                file.CopyTo(dstFullNameTemp, true);
                Logger.debug("renaming file {0} to {1}", dstFullNameTemp, dstFullName);
                if (File.Exists(dstFullName))
                {
                    Logger.debug("try delete exsiting file {0}", dstFullName);
                    System.IO.File.Delete(dstFullName);

                }
                new FileInfo(dstFullNameTemp).MoveTo(dstFullName);
                Logger.debug("backup file {0} done.", dstFullName);
            }
        }

        private bool shouldBackupOnTheMethod(FileInfo file)
        {
            if (config.fileBackupMethod == Enumeration.FileBackupMethod.INCREMENTAL)
            {
                if (file.LastAccessTime < config.lastLaunch || file.LastAccessTime >= config.scheduledDateTime)
                {
                    return false;
                }
            }
            return true;
        }

        private bool shouldExcludeByPatterns(string name, params string[] excludePatterns)
        {
            if (excludePatterns != null)
            {
                return excludePatterns.Aggregate(false, (current, pattern) =>
                                                    current || Regex.Match(name, pattern).Success);
            }
            return false;
        }
    }
}
