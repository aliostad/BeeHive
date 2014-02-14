using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Repositories
{
    public interface IKeyedListRepository<T>
    {
        Task AddAsync(Guid key, T t);

        Task<IEnumerable<T>> Get(Guid key);

        Task Remove(Guid key);
    }

    public interface ISimpleKeyedListRepository : IKeyedListRepository<Guid>
    {
       
    }


}
