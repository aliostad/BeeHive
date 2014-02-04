using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.ConsoleDemo
{

    [ActorDescription("publish-subscribe", 3, 30)]
    public class DummyActor : IProcessorActor
    {
        private Random _random = new Random();

        public void Dispose()
        {
            
        }



        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            await Task.Delay(_random.Next(100, 500));
            Console.WriteLine("Processed a message with Id " + evnt.Id);
            return new Event[0];
        }
    }
}
