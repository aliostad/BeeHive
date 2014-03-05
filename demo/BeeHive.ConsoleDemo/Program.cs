using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Actors;
using BeeHive.DataStructures;
using BeeHive.ServiceLocator.Windsor;
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
                IWindsorContainer container = new WindsorContainer();
                ConfigureDI(container);

                var orchestrator = container.Resolve<Orchestrator>();
                ConsoleWriteLine(ConsoleColor.Green, "Started the processing. Enter <n> to create a message or press <ENTER> to end");
                orchestrator.SetupAsync().Wait();
                orchestrator.Start();

                var queueOperator = container.Resolve<IEventQueueOperator>();

                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "n")
                        queueOperator.PushAsync(new Event("Helloooo!")
                        {
                            EventType = "publish"
                        }).Wait();
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
                    .ImplementedBy<AppDomainExplorerActorConfiguration>()
                    .LifestyleTransient()
                    .DynamicParameters((kernel, dic) => dic.Add("assemblyPrefix", "BeeHive")),

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
                .LifestyleTransient(),

                Component.For(typeof(ICounterStore))
                .ImplementedBy(typeof(InMemoryCounterStore))
                .LifestyleTransient(),

                Component.For(typeof(IKeyedListStore<>))
                .ImplementedBy(typeof(InMemoryKeyedListStore<>))
                .LifestyleTransient(),


                Component.For<DummyActor>()
                .ImplementedBy<DummyActor>()
                .LifestyleTransient()
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