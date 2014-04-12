using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using BeeHive.Actors;
using BeeHive.Azure;
using BeeHive.Configuration;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.ServiceLocator.Windsor;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace BeeHive.Demo.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        private Orchestrator _orchestrator;
        private IConfigurationValueProvider _configurationValueProvider =
            new AzureConfigurationValueProvider();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("BeeHive.Demo.Worker entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 1200;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var container = new WindsorContainer();
            ConfigureDI(container);

            _orchestrator = container.Resolve<Orchestrator>();
            _orchestrator.SetupAsync().Wait();
            _orchestrator.Start();

            return base.OnStart();
        }

        public void ConfigureDI(IWindsorContainer container)
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
                    .ImplementedBy<ServiceBusOperator>()
                    .DynamicParameters((k, dic)
                        =>
                    {
                        dic["connectionString"] = _configurationValueProvider.GetValue("BusConnectionString");
                    })
                    .LifestyleSingleton(),

                Component.For(typeof(ICollectionStore<>))
                .ImplementedBy(typeof(AzureCollectionStore<>))
                .LifestyleSingleton(),

                Component.For(typeof(ICounterStore))
                .ImplementedBy(typeof(AzureCounterStore))
                .LifestyleSingleton(),

                Component.For(typeof(IKeyedListStore<>))
                .ImplementedBy(typeof(AzureKeyedListStore<>))
                .LifestyleSingleton(),

                Classes.FromAssemblyContaining<Order>()
                    .Pick()
            );
        }

        public override void OnStop()
        {
            _orchestrator.Stop();
            _orchestrator.Dispose();
            base.OnStop();
        }
    }
}
