using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BeeHive.Tests
{
    public class TraceTests
    {
        [Fact]
        public void TraceErrorDoesNotThrowException()
        {
            TheTrace.TraceError("chappi {1} {0}", "hapachap", 7979);
        }

        [Fact]
        public void TraceErrorDoesNotThrowExceptionWithNoParams()
        {
            TheTrace.TraceError("chappi {1} {0}");
        }

        [Fact]
        public void TraceErrorDoesNotThrowExceptionWithMissingParams()
        {
            TheTrace.TraceError("chappi {1} {0}", "hapachap");
        }


        [Fact]
        public void TraceInfoDoesNotThrowException()
        {
            TheTrace.TraceInformation("chappi {1} {0}", "hapachap", 7979);
        }

        [Fact]
        public void TraceWarningDoesNotThrowException()
        {
            TheTrace.TraceWarning("chappi {1} {0}", "hapachap", 7979);
        }

        [Fact]
        public void TraceWarningDoesNotThrowExceptionWithNoParams()
        {
            TheTrace.TraceWarning("chappi ");
        }

       
    }
}
