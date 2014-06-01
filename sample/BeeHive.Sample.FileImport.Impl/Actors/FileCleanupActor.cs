using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Sample.FileImport.Impl.Events;

namespace BeeHive.Sample.FileImport.Impl.Actors
{

    [ActorDescription("ImportFileProcessed-Cleanup")]
    public class FileCleanupActor : IProcessorActor
    {
        private IDynamoStore _dynamoStore;

        public FileCleanupActor(IDynamoStore dynamoStore)
        {
            _dynamoStore = dynamoStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var importFileProcessed = evnt.GetBody<ImportFileProcessed>();
            var statusFile = importFileProcessed.FileId + Constants.StatusPostfix;

            await _dynamoStore.DeleteAsync(new SimpleBlob()
            {
                Id = importFileProcessed.FileId
            });
            await _dynamoStore.DeleteAsync(new SimpleBlob()
            {
                Id = statusFile
            });

            return new Event[0];
        }
    }
}
