using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using BeeHive.Demo.PrismoEcommerce.Events;
using BeeHive.Demo.PrismoEcommerce.Exceptions;
using BeeHive.Demo.PrismoEcommerce.ExternalSystems;

namespace BeeHive.Demo.PrismoEcommerce.Actors
{

    [ActorDescription("OrderAccepted-Payment")]
    public class PaymentActor : IProcessorActor
    {
        private PaymentGateway _paymentGateway;
        private ICollectionStore<Payment> _paymentRepo;
        private ICollectionStore<Order> _orderRepo;

        public PaymentActor(
            ICollectionStore<Order> orderRepo,
            ICollectionStore<Payment> paymentRepo,
            PaymentGateway paymentGateway)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _paymentGateway = paymentGateway;
        }

        public void Dispose()
        {
            
        }

        public async Task<IEnumerable<Event>> ProcessAsync(Event evnt)
        {
            var orderAccepted = evnt.GetBody<OrderAccepted>();
            var order =  await _orderRepo.GetAsync(orderAccepted.OrderId);

            Trace.TraceInformation("PaymentActor received OrderAccepted " + order.Id);

            // if order is cancelled no further processing
            if (order.IsCancelled)
            {
                Trace.TraceInformation("Order was cancelled.");
                return new Event[0];                
            }

            var payment = new Payment()
            {
                Amount   = order.TotalPrice,
                OrderId = order.Id,
                PaymentMethod = order.PaymentMethod
            };

            try
            {
                var transactionId = await _paymentGateway.Authorise(payment);
                Trace.TraceInformation("Order transaction authorised: " + transactionId);

                payment.TransactionId = transactionId;
                await _paymentRepo.InsertAsync(payment);
                Trace.TraceInformation("Payment success");

                return new[]
                {
                    new Event(new PaymentAuthorised()
                    {
                        OrderId = order.Id,
                        PaymentId = payment.Id
                    })
                };
            }
            catch (AggregateException aggregateException)
            {
                var paymentFailureException = aggregateException.InnerExceptions.OfType<PaymentFailureException>()
                    .FirstOrDefault();
                if (paymentFailureException != null)
                {
                    Trace.TraceInformation("Payment failed");

                    Trace.TraceWarning(paymentFailureException.ToString());
                    return new[]
                    {
                        new Event(new PaymentFailed()
                        {
                            OrderId = order.Id
                        })                                    
                    };            
                }
                else
                {
                    throw;
                }

            }
            catch (PaymentFailureException paymentFailureException)
            {
                Trace.TraceInformation("Payment failed");
                Trace.TraceWarning(paymentFailureException.ToString());
                return new[]
                {
                    new Event(new PaymentFailed()
                    {
                        OrderId = order.Id
                    })                                    
                };

            }
            
        }
    }
}
