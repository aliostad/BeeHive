using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;

namespace BeeHive.Demo.PrismoEcommerce.Repositories
{
    public interface ICounterRepository
    {
        Task<ICounter> GetAsync(Guid id);

        /// <summary>
        /// Use it for decrement as well
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task IncrementAsync(Guid id, long value);
    }
}

