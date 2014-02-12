using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.ExternalSystems
{
    /// <summary>
    /// Simplistic emailer
    /// </summary>
    public class Emailer
    {
        public void Send(string address, string content)
        {
            Trace.TraceInformation("Sendin email to {0}. Content:\r\n{1}",address,content);
        }
    }
}
