using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awful.Configurator.Entity
{
    public class AwfulTaskConfigBase
    {
        protected AwfulTaskConfigBase()
        {
            identifier = new Identifier();
            identifier.guid = Guid.NewGuid();
        }
        public Identifier identifier { set; get; }

        //no use of 'type' but 'businussObject' instead
        public Enumeration.TaskType type { get; protected set; }
        public Type businussObject { get; set; }

        //there are different days in months , thus monthly span cannot be decided before running
        //using string instead.
        public Enumeration.RespawnSpanType respawnSpan { get; set; }
        public bool isRespawnable()
        {
            return respawnSpan != Enumeration.RespawnSpanType.ONCE;
        }
        public DateTime scheduledDateTime { get; set; }
        public DateTime lastLaunch { get; set; }

        public string replaceMacroOfDate(string orgString , DateTime datetime)
        {
            string macroReplace = String.Format("{0:D4}{1:D2}{2:D2}",
                datetime.Year, datetime.Month, datetime.Day);
            return orgString.Replace("%d", macroReplace);
        }
    }
}
