using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class Enumeration
    {
        public enum TaskType
        {
            FILE_BACKUP,
            DATABASE_BACKUP,
            FILE_CLEAN
        }

        public enum RespawnSpanType
        {
            MONTHLY,
            WEEKLY,
            DAYLY,
            MINUTELY,  //for debug only
            ONCE
        }

        public enum FileBackupMethod
        {
            INCREMENTAL,
            FULL
        }

        public enum DatabaseBackupMethod
        {
            DIFFERENTIAL,
            FULL,
            COMPOSITE
        }
    }
}
