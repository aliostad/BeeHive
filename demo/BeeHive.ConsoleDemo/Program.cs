using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;
using BeeHive.Configuration;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.ServiceLocator.Windsor;
using BeeHive.Tools;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace BeeHive.ConsoleDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Trace.Listeners.Clear();
                Trace.Listeners.Add(new ColourfulConsoleTraceListener());
                IWindsorContainer container = new WindsorContainer();
                ConfigureDI(container);
                
                    

                var orchestrator = container.Resolve<Orchestrator>();
                ConsoleWriteLine(ConsoleColor.Green, "Started the processing. Enter <n> to create a message or press <ENTER> to end");
                orchestrator.SetupAsync().Wait();
                orchestrator.Start();

                var queueOperator = container.Resolve<IEventQueueOperator>();
                var orderStore = container.Resolve<ICollectionStore<Order>>();
                var customerStore = container.Resolve<ICollectionStore<Customer>>();

                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "n")
                    {
                        var order = new Order()
                        {
                            CustomerId    = Guid.NewGuid(),
                            Id = Guid.NewGuid(),
                            PaymentMethod = "Card/Visa/4444333322221111/123",
                            ShippingAddress = "Jabolsa",
                            TotalPrice = 223,
                            ProductQuantities = new Dictionary<Guid, int>()
                            {
                                {Guid.NewGuid(), 1},
                                {Guid.NewGuid(), 2},
                                {Guid.NewGuid(), 3},
                            }
                        };

                        var customer = new Customer()
                        {
                            Address = "2, Korat Jingala",
                            Email = "ostad@chopak.it",
                            Id = order.CustomerId,
                            Name = "Natsak Birat"
                        };

                        customerStore.InsertAsync(customer).Wait();

                        orderStore.InsertAsync(order).Wait();
                        queueOperator.PushAsync(new Event(new OrderAccepted()
                        {
                            OrderId = order.Id
                        })
                        {
                            EventType = "OrderAccepted"
                        }).Wait();
                        
                    }
                    else
                        break;
                }

                ConsoleWriteLine(ConsoleColor.DarkCyan, "Stopped");
            }
            catch (Exception e)
            {
                
                ConsoleWriteLine(ConsoleColor.Red, e.ToString());
                Console.ReadLine();
            }
        


        }

   
        public static void ConfigureDI(IWindsorContainer container)
        {
            var serviceLocator = new WindsorServiceLocator(container);

            container.Register(

                Component.For<Orchestrator>()
                .ImplementedBy<Orchestrator>()
                .LifestyleTransient(),

                Component.For<IActorConfiguration>()
                    .Instance(
                    ActorDescriptors.FromAssemblyContaining<OrderAccepted>()
                    .ToConfiguration()),

                Component.For<IServiceLocator>()
                    .Instance(serviceLocator),

                Component.For<IFactoryActor>()
                    .ImplementedBy<FactoryActor>()
                    .LifestyleTransient(),

                Component.For<IEventQueueOperator>()
                    .ImplementedBy<InMemoryServiceBus>()
                    .LifestyleSingleton(),           

                Component.For(typeof(ICollectionStore<>))
                .ImplementedBy(typeof(InMemoryCollectionStore<>))
                .LifestyleSingleton(),

                Component.For(typeof(ICounterStore))
                .ImplementedBy(typeof(InMemoryCounterStore))
                .LifestyleSingleton(),

                Component.For(typeof(IKeyedListStore<>))
                .ImplementedBy(typeof(InMemoryKeyedListStore<>))
                .LifestyleSingleton(),

                Component.For<DummyActor>()
                .ImplementedBy<DummyActor>()
                .LifestyleTransient(),

                Classes.FromAssemblyContaining<Order>()
                    .Pick()
            );
        }
        private static void ConsoleWrite(ConsoleColor color, string value, params object[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value, args);
            Console.ForegroundColor = foregroundColor;
        }

        private static void ConsoleWriteLine(ConsoleColor color, string value, params object[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value, args);
            Console.ForegroundColor = foregroundColor;
        }

    }
}