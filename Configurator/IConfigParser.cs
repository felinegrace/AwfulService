using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awful.Configurator.Entity;

namespace Awful.Configurator
{
    public interface IConfigParser
    {
        List<AwfulTaskConfigBase> parse();
        List<AwfulTaskConfigBase> parse(string fileFullName);
    }
}
