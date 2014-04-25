using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Configurator.Entity;
using Awful.Utility;

namespace Awful.Scheduler
{
    class AwfulDatabaseCompositeBackupTask : AwfulDatabaseBackupTask
    {
        
        private AwfulDatabaseCompositeBackupConfig config { get; set; }
        private int internativeCount { get; set; }
        public DateTime lastAlternative { get; set; }
        private Enumeration.DatabaseBackupMethod currMethod { get; set; }

        public AwfulDatabaseCompositeBackupTask(AwfulDatabaseCompositeBackupConfig config)
            : base(config)
        {
            this.config = config;
            internativeCount = 0;
        }

        protected override void methodDependentPreparation()
        {
            if (internativeCount == 0)
            {
                currMethod = Enumeration.DatabaseBackupMethod.FULL;
                lastAlternative = config.scheduledDateTime;
            }
            else
            {
                currMethod = Enumeration.DatabaseBackupMethod.DIFFERENTIAL;
            }

            internativeCount++;
            if (internativeCount >= config.methodAlternative)
            {
                internativeCount = 0;
            }
        }

        protected override string methodDependentDstFileName()
        {
            string macroReplace = String.Format("{0:D4}{1:D2}{2:D2}",
                    lastAlternative.Year, lastAlternative.Month, lastAlternative.Day);
            string alternateString = config.dstFile.Replace("%a", macroReplace);
            string replacedString = config.replaceMacroOfDate(alternateString, config.scheduledDateTime);
            switch(currMethod)
            {
                case Enumeration.DatabaseBackupMethod.FULL:
                    return replacedString + ".full";
                case Enumeration.DatabaseBackupMethod.DIFFERENTIAL:
                    return replacedString;
                default:
                    {
                        Logger.error("task {0} reaches an untouchable method. crash this task.", config.identifier.descriptor);
                        throw new Exception("critical task error.");
                    }
            }
        }

        protected override string methodDependentSql()
        {
            return methodDependentSqlBySpecifiedMethod(currMethod);
        }



    }
}
