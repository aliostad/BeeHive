using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Sample.FileImport.Impl.Events
{
    public class ImportRecordExtracted
    {
        public string Id { get; set; }

        public string IndexType { get; set; }

        public string Content { get; set; }
    }
}
