using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using Xunit;

namespace BeeHive.Tests.Configuration
{

    public interface ISaysStarted
    {
        bool Started { get; }
    }

    public class Startup1 : IStartup, ISaysStarted
    {

        public bool Started { get; set; }

        public void Start(IServiceLocator serviceLocator)
        {
            Started = true;
        }
    }

    public class Startup2 : IStartup, ISaysStarted
    {

        public bool Started { get; set; }

        public void Start(IServiceLocator serviceLocator)
        {
            Started = true;
        }
    }

    public class StartupSetupTests
    {

        [Fact]
        public void FindsAllStartupsByPassingAssembly()
        {
            Assert.Equal(2,
                StartupSetup.InAssembly(Assembly.GetExecutingAssembly()).Count());
        }

        [Fact]
        public void FindsAllStartupsByType()
        {
            Assert.Equal(2,
                StartupSetup.InAssembly<StartupSetupTests>().Count());
        }

        [Fact]
        public void StartDoesStartThem()
        {
            var startups = StartupSetup.InAssembly<StartupSetupTests>().ToArray();
            var saysStarteds = startups.Cast<ISaysStarted>().ToArray();

            startups.Start(null);
            Assert.True(saysStarteds[0].Started);
            Assert.True(saysStarteds[1].Started);
        }
    }
}
