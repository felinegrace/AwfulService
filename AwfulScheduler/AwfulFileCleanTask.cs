using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Configurator.Entity;
using Awful.Utility;
using System.IO;
using System.Text.RegularExpressions;

namespace Awful.Scheduler
{
    class AwfulFileCleanTask : AwfulTask
    {
        private AwfulFileCleanConfig config { get; set; }
        private bool shouldTerminate { get; set; }
        //config object ownership should be taken
        public AwfulFileCleanTask(AwfulFileCleanConfig config)
        {
            this.config = config;
            shouldTerminate = false;
        }

        protected override void run()
        {
            Logger.debug("{0} running , last launch is {1}.", config.identifier.descriptor, config.lastLaunch);
            foreach (string dstFolder in config.dstFolders)
            {
                if (shouldTerminate)
                {
                    Logger.debug("receive TERMINATE signal, exiting...");
                    shouldTerminate = false;
                    return;
                }
                string translateDestination = config.replaceMacroOfDate(dstFolder, config.scheduledDateTime);
                DirectoryInfo dstInfo = new DirectoryInfo(dstFolder);
                if (!dstInfo.Exists)
                {
                    Logger.debug("dst folder {0} not exist , skpping...", translateDestination);
                    continue;
                }
                //replace %d macro to yyyymmdd
                Logger.debug("recursive cleaning {0}.", translateDestination);
                recursiveClean(dstInfo , null);

            }
            Logger.debug("{0} done.", config.identifier.descriptor);

        }

        private void recursiveClean(DirectoryInfo target, params string[] excludePatterns)
        {
            foreach (DirectoryInfo dir in target.GetDirectories())
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
                    recursiveClean(dir, excludePatterns);
                }
            }

            foreach (FileInfo file in target.GetFiles())
            {
                if (shouldTerminate)
                {
                    Logger.debug("receive TERMINATE signal, exiting...");
                    return;
                }
                fileClean(file, excludePatterns);
            }

            if (config.deleteEmptyFolder 
                && target.GetDirectories().Length == 0 
                && target.GetFiles().Length == 0)
            {
                Logger.debug("deleting empty folder {0}", target.FullName);
                try
                {
                    target.Delete();
                    Logger.debug("deleting empty folder {0} done.", target.FullName);
                }
                catch (Exception exception)
                {
                    Logger.info("failed deleting folder {0} :{1} . skipping...", target.FullName, exception.Message);
                }
            }
        }

        private void fileClean(FileInfo file, params string[] excludePatterns)
        {
            var fileName = file.Name;
            bool shouldClean = shouldCleanOnTheExpiration(file);
            bool shouldExclude = shouldExcludeByPatterns(fileName, excludePatterns);
            if (shouldClean && !shouldExclude)
            {
                Logger.debug("deleting file {0}", fileName);
                try
                {
                    file.Delete();
                    Logger.debug("deleting file {0} done.", fileName);
                }
                catch(Exception exception)
                {
                    Logger.info("failed deleting file {0} :{1} . skipping...", fileName , exception.Message);
                }
                
            }
        }

        private bool shouldCleanOnTheExpiration(FileInfo file)
        {
            if (file.LastAccessTime < config.scheduledDateTime - config.expiration)
            {
                return true;
            }
            return false;
        }

        protected override void cancel()
        {
            shouldTerminate = true;
        }

        public override AwfulTaskConfigBase getConfig()
        {
            return config;
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
