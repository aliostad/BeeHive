using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;

namespace BeeHive.Demo.PrismoEcommerce.Repositories
{
    interface ICustomerRepository
    {
        void Create(Customer customer);

        Customer Get(Guid id);

    }
}
