using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awful.Configurator;
using Awful.Utility;

namespace TestConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.enable();
            Configurator c = new Configurator();
            TaskConfig j = c.readJsonFromFile();
            Logger.debug(j.respawnSpan);
            Logger.debug(j.launchDay.ElementAt<string>(0));
            Logger.debug(j.launchTime);
            Logger.debug(j.srcFolders.ElementAt<string>(0));
            Logger.debug(j.dstFolder.ElementAt<string>(0));
        }
    }
}
