using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using BeeHive.DataStructures;
using BeeHive.Sample.FileImport.Impl.Events;

namespace BeeHive.Sample.FileImport.Impl.Actors
{

    [ActorDescription("OneMinutePassed-FileImportSweep")]
    public class SweeperActor : IProcessorActor
    {
        private IDynamoStore _dynamoStore;
        private IConfigurationValueProvider _configurationValueProvider;

        public SweeperActor(IDynamoStore dynamoStore, 
            IConfigurationValueProvider configurationValueProvider)
        {
            _configurationValueProvider = configurationValueProvider;
            _dynamoStore = dynamoStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var events = new List<Event>();
            var items = (await _dynamoStore.ListAsync(
                _configurationValueProvider.GetValue(Constants.SweepRootPathKey)))
                .ToArray();

            var notProcessed = items.Where(x => !x.IsVirtualFolder)
                .GroupBy(z => z.Id.Replace(Constants.StatusPostfix, ""))
                .Where(f => f.Count() == 1)
                .Select(w => w.Single());

            foreach (var blob in notProcessed)
            {
                events.Add(new Event(new NewFileArrived()
                {
                    FileId = blob.Id
                }));
                await _dynamoStore.InsertAsync(new SimpleBlob()
                {
                    Id = blob.Id + Constants.StatusPostfix,
                    Body = new MemoryStream(BitConverter.GetBytes(1)) // status 1
                });
            }

            return events;
        }
    }
}
