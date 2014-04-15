using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awful.Configurator
{
    public class TaskConfig
    {
        public string name { get; set; }
        public string respawnSpan { get; set; }
        public DateTime launchDateTime { get; set; }
        public List<string> srcFolders { get; set; }
        public List<string> dstFolders { get; set; }
    }
}
