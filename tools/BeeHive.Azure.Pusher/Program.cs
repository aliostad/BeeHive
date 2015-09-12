using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Azure.Pusher
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 3)
            {
                Console.WriteLine("Usage: BeeHive.Azure.Pusher <azure service bus namespace> <topic name> <file name>");
                return;
            }

            try
            {
                var serviceBusOperator = new ServiceBusOperator(args[0]);
                serviceBusOperator.PushAsync(new Event()
                {
                    Body = File.ReadAllText(args[2]),
                    ContentType = "application/json",
                    EventType = "Standard",
                    QueueName = QueueName.FromTopicName(args[1]).ToString(),
                    Timestamp = DateTimeOffset.Now
                }).Wait();

                Console.WriteLine("Message was sent successfully");
            }
            catch (Exception e)
            {                
                Console.WriteLine("DAILED: " + e.ToString());
            }
        }
    }
}
