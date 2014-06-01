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

    [ActorDescription("NewFileArrived-Split")]
    public class FileSplitterActor : IProcessorActor
    {
        private IDynamoStore _dynamoStore;

        public FileSplitterActor(IDynamoStore dynamoStore)
        {

            _dynamoStore = dynamoStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var newFileArrived = evnt.GetBody<NewFileArrived>();
            var blob = await _dynamoStore.GetAsync(newFileArrived.FileId);
            var reader = new StreamReader(blob.Body);
            string line = string.Empty;
            var events = new List<Event>();
            while ((line = reader.ReadLine())!= null)
            {
                var fields = line.Split(new []{','},StringSplitOptions.RemoveEmptyEntries);
                events.Add(new Event( new ImportRecordExtracted()
                {
                   Id = fields[0],
                   Content = fields[2],
                   IndexType = fields[1]
                }));
            }

            events.Add(new Event(new ImportFileProcessed()
            {
               FileId = newFileArrived.FileId
            }));

            return events;
        }
    }
}
