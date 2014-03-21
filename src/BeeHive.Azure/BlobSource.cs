using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Azure
{
    public class BlobSource
    {

        public BlobSource()
        {
            Path = "";
        }

        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public string Path { get; set; }
    }
}
