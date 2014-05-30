using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;

namespace BeeHive.Sample.FileImport.Impl.Actors
{
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

        public Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            throw new NotImplementedException();
        }
    }
}
