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
            if(ConflictsWith(item, against))
                throw new ConcurrencyConflictException(item.ETag, against.ETag);
        }

        public static bool ConflictsWith(this IConcurrencyAware item, IConcurrencyAware against)
        {
            if (item.ETag != null && against.ETag != null)
            {
                return (item.ETag != against.ETag);
            }

            if (item.LastModofied.HasValue && against.LastModofied.HasValue)
            {
                return (item.LastModofied.Value != against.LastModofied.Value);
            }

            // if all values null it is OK
            if (!item.LastModofied.HasValue && !against.LastModofied.HasValue &&
                item.ETag == null && against.ETag == null)
                return false;

            throw new InvalidOperationException("Concurrency data not consistent");

        }
    }
}
