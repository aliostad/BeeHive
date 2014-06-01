using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Configuration;
using BeeHive.Sample.FileImport.Impl.Events;

namespace BeeHive.Sample.FileImport.Impl.Actors
{

    [ActorDescription("ImportRecordExtracted-LoadAndIndex")]
    public class RecordImporterActor : IProcessorActor
    {
        private IConfigurationValueProvider _configurationValueProvider;

        public RecordImporterActor(IConfigurationValueProvider configurationValueProvider)
        {
            _configurationValueProvider = configurationValueProvider;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var importRecordExtracted = evnt.GetBody<ImportRecordExtracted>();
            var elasticSearchUrl = _configurationValueProvider.GetValue(Constants.ElasticSearchUrlKey);

            var client = new HttpClient();
            var url = string.Format("{0}/import/{1}/{2}", elasticSearchUrl,
                importRecordExtracted.IndexType,
                importRecordExtracted.Id);
            var responseMessage = await client.PutAsJsonAsync(url, importRecordExtracted);

           if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ApplicationException("Indexing failed. " 
                    + responseMessage.ToString());
            }

            return new[]
            {
                new Event(new NewIndexUpserted()
                {
                    IndexUrl = url
                }) 
            };
        }
    }
}
