using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.Scheduling;

namespace BeeHive
{
    public class EventArrivedArgs : EventArgs
    {
        public Event ArrivedEvent { get; set; }
    }

    /// <summary>
    /// Defines primitive for pull vs event-driven message stream
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    public interface IMessageStream<T>
    {
        /// <summary>
        /// If true then consumer must use the EventArrived vs NextAsync
        /// </summary>
        bool IsEventDriven { get; }

        /// <summary>
        /// Supported when IsEventDriven is false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<PollerResult<T>> NextAsync(QueueName name);

        /// <summary>
        /// Event-driven pipe for registering handler
        /// </summary>
        void RegisterHandler(Func<Event, Task<IEnumerable<Event>>> handler, ActorDescriptor descriptor);

        /// <summary>
        /// Is required when NextAsync is used and there is risk of operation taking more than lease time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="howLong"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task KeepExtendingLeaseAsync(T message, TimeSpan howLong, CancellationToken cancellationToken);

    }

    public interface ISubscriptionOperator<T> : IMessageStream<T>
    {
        Task AbandonAsync(T message);

        Task CommitAsync(T message);

        Task DeferAsync(T message, TimeSpan howLong);
    }

    public interface ITopicOperator<T>
    {
        Task PushAsync(T message);

        Task PushBatchAsync(IEnumerable<T> messages);
    }


    public interface IQueueOperator<T> : ITopicOperator<T>, ISubscriptionOperator<T>
    {
        
        Task CreateQueueAsync(QueueName name);

        Task DeleteQueueAsync(QueueName name);

        Task<bool> QueueExistsAsync(QueueName name);

    }

    public interface IEventQueueOperator : IQueueOperator<Event>
    {
        
    }

}
