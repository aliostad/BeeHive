using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Repositories
{
    public interface IRepository<T>
    {

        Task<T> GetAsync(Guid id);

        Task InsertAsync(T t);

        Task UpsertAsync(T t);

        Task DeleteAsync(Guid id);

    }
}
