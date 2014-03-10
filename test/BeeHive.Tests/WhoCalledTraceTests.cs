using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Internal;
using BeeHive.Tools;
using Xunit;

namespace BeeHive.Tests
{
    public class WhoCalledTraceTests
    {

        [Fact]
        public void ItWasMeCalledTraceWrite()
        {

            var whoCalledAMethod = new WhoCalledAMethod("BeeHive.Tests.WhoCalledTraceTests",
                "ItWasMeCalledTraceWrite");

            var method = whoCalledAMethod.GetTheMethod();
            Assert.Equal(method.DeclaringType.ToString(), "System.RuntimeMethodHandle");
            Assert.Equal(method.Name, "InvokeMethod");
        }
    }


}
