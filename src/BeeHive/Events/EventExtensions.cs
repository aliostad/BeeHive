using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Events
{
    public static class EventExtensions
    {
        public static void  TryDisposeUnderlying(this Event e)
        {
            if (e != null)
            {
                var disposable = e.UnderlyingMessage as IDisposable;
                if (disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {                        
                        // ignore
                    }
                }
            }
        }
    }
}
