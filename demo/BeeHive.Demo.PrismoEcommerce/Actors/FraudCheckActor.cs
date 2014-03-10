using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.ExternalSystems;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{
    [ActorDescription("PaymentAuthorised-FrauCheck")]
    public class FraudCheckActor : IProcessorActor
    {
        private ICollectionStore<Customer> _customeStore;
        private ICollectionStore<Order> _ordeStore;
        private ICollectionStore<Payment> _paymentStore;
        private Random _random = new Random();
        private FraudChecker _fraudChecker;

        public FraudCheckActor(ICollectionStore<Customer> customeStore,
            ICollectionStore<Order> ordeStore,
            ICollectionStore<Payment> paymentStore,
            FraudChecker fraudChecker)
        {
            _fraudChecker = fraudChecker;
            _paymentStore = paymentStore;
            _ordeStore = ordeStore;
            _customeStore = customeStore;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var authorised = evnt.GetBody<PaymentAuthorised>();
            var order = await _ordeStore.GetAsync(authorised.OrderId);
            if (order.IsCancelled)
                return new Event[0];

            var payment = await _paymentStore.GetAsync(authorised.PaymentId);
            var customer = await _customeStore.GetAsync(order.CustomerId);
            Trace.TraceInformation("FraudCheckActor - checking order for fraud: " + order.Id);

            if (_fraudChecker.IsFradulent(payment, customer))
            {
                Trace.TraceInformation("FraudCheckActor - !! IS fraud: " + order.Id);

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

            Trace.TraceInformation("FraudCheckActor - order not fraud: " + order.Id);

            return new Event[0];
        }
    }
}
