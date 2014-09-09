using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Visualiser.Model
{
    public class AnalysisModel
    {

        public AnalysisModel()
        {
            Heads = new List<Topic>();
        }

        public List<Topic> Heads { get; set; } 
    }
}
