using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.ExternalSystems;
using BeeHive.Demo.PrismoEcommerce.Repositories;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{
    [ActorDescription("PaymentAuthorised-FrauCheck")]
    public class FraudCheckActor : IProcessorActor
    {
        private ICollectionRepository<Customer> _customeRepository;
        private ICollectionRepository<Order> _ordeRepository;
        private ICollectionRepository<Payment> _paymentRepository;
        private Random _random = new Random();
        private FraudChecker _fraudChecker;

        public FraudCheckActor(ICollectionRepository<Customer> customeRepository,
            ICollectionRepository<Order> ordeRepository,
            ICollectionRepository<Payment> paymentRepository,
            FraudChecker fraudChecker)
        {
            _fraudChecker = fraudChecker;
            _paymentRepository = paymentRepository;
            _ordeRepository = ordeRepository;
            _customeRepository = customeRepository;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var authorised = evnt.GetBody<PaymentAuthorised>();
            var order = await _ordeRepository.GetAsync(authorised.OrderId);
            if (order.IsCancelled)
                return new Event[0];

            var payment = await _paymentRepository.GetAsync(authorised.PaymentId);
            var customer = await _customeRepository.GetAsync(order.CustomerId);
            if (_fraudChecker.IsFradulent(payment, customer))
            {
                return new[]
                {
                    new Event( 
                    new FraudCheckFailed()
                    {
                        OrderId = order.Id,
                        PaymentId = payment.Id
                    })
                    {
                        EventType = "FraudCheckFailed",
                        QueueName = "FraudCheckFailed"
                    }
                };
            }

            return new Event[0];
        }
    }
}
