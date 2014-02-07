using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Repositories
{
    public interface IRepository<T>
    {
        void Create(T t);

        T Get(Guid id);
    }
}
