using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Configuration;
using BeeHive.Scheduling;
using Xunit;

[assembly: SimpleAutoPulserDescription("ev1", 1)]
[assembly: SimpleAutoPulserDescription("ev2", 2)]

namespace BeeHive.Tests.Scheduling
{
    public class PulserTests
    {

        public PulserTests()
        {
            ThreadPool.SetMinThreads(50, 50);
        }

        [Fact]
        public void CanFindAssemblyPulsers()
        {
            int ev1GeneratedCount = 0;
            int ev2GeneratedCount = 0;
            var pulsers = Pulsers.FromAssembly(Assembly.GetExecutingAssembly()).ToList();
            EventHandler<IEnumerable<Event>> handler = (o, events) =>
            
                events.ToList().ForEach(e =>
                {
                    if (e.EventType == "ev1")
                        ev1GeneratedCount++; 
                    if (e.EventType == "ev2")
                        ev2GeneratedCount++;
                    
                });
            pulsers.ForEach(x =>
            {
                x.PulseGenerated += handler;                 
            });
            pulsers.ForEach(z => z.Start());
            Thread.Sleep(2900);
            pulsers.ForEach(z => z.Stop());

            Assert.Equal(2, ev2GeneratedCount);
            Assert.Equal(3, ev1GeneratedCount);

        }

        [Fact]
        public void CanCreatePulserFromAsyncPulser()
        {
            var pulser = new DummyPulser().ToPulser();
            var list = new List<Event>();
            pulser.PulseGenerated += (sender, events) => list.AddRange(events);
            pulser.Start();
            Thread.Sleep(1900);
            pulser.Stop();
            Assert.Equal(2, list.Count);
            Assert.Equal("Dummy",list.First().EventType);

        }

        /// <summary>
        /// Just a harness
        /// </summary>
        private void Can()
        {
          
            Console.WriteLine("{0} {1}", DateTime.Now.ToString("O"), "Starting");
            var pulsers = Pulsers.FromAssembly(Assembly.GetExecutingAssembly()).ToList();
            EventHandler<IEnumerable<Event>> handler = (o, events) =>

                events.ToList().ForEach(e =>
                {
                    if (e.EventType == "ev2")
                        Console.WriteLine("{0} {1}", DateTime.Now.ToString("O"), e.EventType);
                });
            pulsers.ForEach(x =>
            {
                x.PulseGenerated += handler;
            });
            pulsers.ForEach(z => z.Start());
            Thread.Sleep(10900);
            pulsers.ForEach(z => z.Stop());
            
        }

        [PulserInterval(1)]
        public class DummyPulser : IAsyncPulser
        {

            public async Task<IEnumerable<Event>> Pulse(CancellationToken token)
            {
                return new[] {new Event("") {EventType = "Dummy"}};
            }
        }
    }
}
