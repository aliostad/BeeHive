using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Actors
{
    /// <summary>
    /// Optional post-processing
    /// </summary>
    public interface IDelayedProcessorActor : IProcessorActor
    {
        Task PostProcess();
    }
}
