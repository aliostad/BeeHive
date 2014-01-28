using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{
    public class PollerResult<T>
    {
        public PollerResult(bool success, T result)
        {
            PollingResult = result;
            IsSuccessful = success;
        }

        public bool IsSuccessful { get; private set; }

        public T PollingResult { get; private set; }
    }
}
