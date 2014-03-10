using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Exceptions;

namespace BeeHive.Demo.PrismoEcommerce.ExternalSystems
{
    public class PaymentGateway
    {
        private Random _random = new Random();

        public async Task<string> Authorise(Payment payment)
        {
            // failure rate of 10%
            if (_random.NextDouble() < 0.1)
                throw new PaymentFailureException(string.Format("{0}\r\n{1}\r\n{2}",
                    payment.PaymentMethod,
                    payment.Id,
                    payment.Amount
            ));

            return Guid.NewGuid().ToString("N"); // returns transaction id

        }
    }
}
