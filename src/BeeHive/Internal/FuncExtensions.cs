using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Internal
{
    internal static class FuncExtensions
    {
        public static T WrapException<T>(this Func<T> func, T exceptionResult, Action<Exception> handler = null)
        {
            try
            {
                return func();
            }
            catch (Exception exception)
            {
                if (handler == null)
                {
                   TheTrace.TraceError(exception.ToString());
                }
                else
                {
                    handler(exception);
                }
                return exceptionResult;
            }
        }
    }
}
