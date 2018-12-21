using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using BeeHive.Actors;
using BeeHive.Azure;
using BeeHive.Configuration;
using BeeHive.DataStructures;
using BeeHive.Sample.NewsFeedKeywordNotification.Impl.Events;
using BeeHive.Scheduling;
using BeeHive.ServiceLocator.Windsor;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace BeeHive.Sample.NewsFeedKeywordNotification.Worker
{
    public class WorkerRole : RoleEntryPoint
    {

        private IConfigurationValueProvider _configurationValueProvider;
        private List<IPulser> _pulsers;
        private PulserPublisher _pulserPublisher;
        private Orchestrator _orchestrator;

        public WorkerRole()
        {
            _pulsers = Pulsers.FromAssembly(Assembly.GetAssembly(typeof(NewsFeedPulsed)))
                .ToList();


            var container = new WindsorContainer();

            _configurationValueProvider = new AzureConfigurationValueProvider();
            SetupDi(container, _configurationValueProvider, _pulsers.ToArray());
            _pulserPublisher = container.Resolve<PulserPublisher>();
            _orchestrator = container.Resolve<Orchestrator>();

            // insert the list here
            var keyValueStore = container.Resolve<IKeyValueStore>();
            var blob = new SimpleBlob()
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes("http://feeds.bbci.co.uk/news/rss.xml")),
                Id = "newsFeeds.txt",
                LastModified = DateTimeOffset.Now
            };
            keyValueStore.UpsertAsync(blob);

        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("BeeHive.Sample.NewsFeedKeywordNotification.Worker entry point called");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working");
            }
        }

        public static void SetupDi(IWindsorContainer container,
            IConfigurationValueProvider configurationValueProvider,
            IPulser[] pulsers)
                {
            var serviceLocator = new WindsorServiceLocator(container);

            container.Register(

                Component.For<Orchestrator>()
                .ImplementedBy<Orchestrator>()
                .LifestyleTransient(),

                Component.For<IActorConfiguration>()
                    .Instance(
                    ActorDescriptors.FromAssemblyContaining<NewsItemCaptured>()
                    .ToConfiguration()),

                Component.For<IServiceLocator>()
                    .Instance(serviceLocator),

                Component.For<IPulser[]>()
                    .Instance(pulsers),

                Component.For<PulserPublisher>()
                .ImplementedBy<PulserPublisher>()
                .LifestyleSingleton(),

                Component.For<IFactoryActor>()
                    .ImplementedBy<FactoryActor>()
                    .LifestyleTransient(),

                

                Component.For<IEventQueueOperator>()
                    .ImplementedBy<ServiceBusOperator>()
                    .DynamicParameters((k, dic)
                        =>
                    {
                        dic["connectionString"] = configurationValueProvider.GetValue(
                            "BusConnectionString");
                    })
                    .LifestyleSingleton(),

               Component.For(typeof(ICollectionStore<>))
                .ImplementedBy(typeof(AzureCollectionStore<>))
                 .DynamicParameters((k, dic)
                        =>
                 {
                     dic["connectionString"] = configurationValueProvider.GetValue("StorageConnectionString");
                 })
                .LifestyleSingleton(),

                Component.For(typeof(IKeyValueStore))
                .ImplementedBy(typeof(AzureKeyValueStore))
                 .DynamicParameters((k, dic)
                        =>
                 {
                     dic["connectionString"] = configurationValueProvider.GetValue("StorageConnectionString");
                     dic["bucketName"] = "feeds";
                 })
                .LifestyleSingleton(),
               
                Classes.FromAssemblyContaining<NewsFeedPulsed>()
                    .Pick()
                
            );


        }


        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            _orchestrator.SetupAsync().Wait();
            _orchestrator.Start();
            _pulsers.ForEach(x => x.Start());

            return base.OnStart();
        }


        public override void OnStop()
        {
            _orchestrator.Stop();
            _pulsers.ForEach(x => x.Stop());
            base.OnStop();
        }
    }
}
