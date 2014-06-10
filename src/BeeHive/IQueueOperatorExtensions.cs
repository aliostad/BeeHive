using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public static class IQueueOperatorExtensions
    {
        public async static Task SetupQueueAsync<T>(this IQueueOperator<T> op, QueueName name)
        {
            if (! (await op.QueueExists(name)))
            {
                await op.CreateQueueAsync(name);
            }
        }
    }
}
