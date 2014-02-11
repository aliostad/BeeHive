using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;

namespace BeeHive.Demo.PrismoEcommerce.ExternalSystems
{
    public class FraudChecker
    {

        private Random _random = new Random();

        public bool IsFradulent(Payment payment, Customer customer)
        {
            // 1% is fraudulant
            return _random.NextDouble() < 0.01;
        }

    }
}
