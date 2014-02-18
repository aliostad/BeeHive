using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.DataStructures
{
    public static class IConcurrencyAwareExtensions
    {
        public static void AssertNoConflict(this IConcurrencyAware item, IConcurrencyAware against)
        {
            if (item.ETag != null && against.ETag != null)
            {
                if(item.ETag != against.ETag)
                    throw new ConcurrencyConflictException(item.ETag, against.ETag);
                else
                {
                    return;
                }

            }

            if (item.LastModofied.HasValue && against.LastModofied.HasValue)
            {
                if (item.LastModofied.Value != against.LastModofied.Value)
                    throw new ConcurrencyConflictException(item.LastModofied.Value, against.LastModofied.Value);
                else
                {
                    return;
                }
            }

            throw  new InvalidOperationException("Concurrency data not valid");

        }
    }
}
