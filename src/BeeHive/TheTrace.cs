using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public static class TheTrace
    {
        private static Action<TraceLevel, string, object[]> _tracer = (level, message, parameters) =>
        {
            switch (level)
            {
                case TraceLevel.Error:
                    Trace.TraceError(message, parameters);
                    break;
                case TraceLevel.Warning:
                    Trace.TraceWarning(message, parameters);
                    break;
                case TraceLevel.Info:
                case TraceLevel.Verbose:
                    Trace.TraceInformation(message, parameters);
                    break;
                default:
                    // ignore
                    break;

            }
        };

        public static Action<TraceLevel, string, object[]> Tracer
        {
            get
            {
                return _tracer;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");

                _tracer = value;
            }
        }

        public static void TraceError(string message, params object[] parameters)
        {
            Tracer(TraceLevel.Error, message, parameters);
        }

        public static void TraceWarning(string message, params object[] parameters)
        {
            Tracer(TraceLevel.Warning, message, parameters);
        }

        public static void TraceInformation(string message, params object[] parameters)
        {
            Tracer(TraceLevel.Info, message, parameters);
        }

    }
}
