using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;

namespace BeeHive.Demo.PrismoEcommerce.ExternalSystems
{
    public class PaymentGateway
    {
        private Random _random = new Random();

        public async Task<string> Authorise(Payment payment)
        {
            // failure rate of 10%
            if (_random.NextDouble() < 0.1)
                throw new ApplicationException("Payment failure");

            return Guid.NewGuid().ToString("N"); // returns transaction id

        }
    }
}
