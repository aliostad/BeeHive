using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace BeeHive.RabbitMQ
{
    public class ConnectionProvider : IConnectionProvider
    {
        private IConnection _connection;
        private SortedDictionary<FactoryWrapperScore, IConnectionFactoryWrapper> _stats =
            new SortedDictionary<FactoryWrapperScore, IConnectionFactoryWrapper>();
        private object _padLock = new object();

        private class FactoryWrapperScore : IComparable
        {

            public FactoryWrapperScore(IConnectionFactoryWrapper connectionFactoryWrapper)
            {
                ErrorCount = 0;
                FactoryWrapper = connectionFactoryWrapper;
            }

            public IConnectionFactoryWrapper FactoryWrapper { get; private set; }
            public int ErrorCount { get; set; }
            public int CompareTo(object obj)
            {
                var i2 = obj as FactoryWrapperScore;
                if (i2 == null)
                    return int.MaxValue;
                
                var meerkat = ErrorCount.CompareTo(i2.ErrorCount);
                return meerkat == 0
                    ? -1
                    : meerkat;
            }
        }

        public ConnectionProvider(params IConnectionFactoryWrapper[] factories)
        {

            if (factories.Length == 0)
                throw new InvalidOperationException("Must at least provide one factory");

            foreach (var factory in factories)
            {
                _stats.Add(new FactoryWrapperScore(factory), factory);
            }
        }

        public IConnection GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
                BuildConnection();

            return _connection;
        }

        private class ConnectionNotOpenException : Exception
        {
             
        }

        private void BuildConnection()
        {
            lock (_padLock)
            {
                if (_connection != null && _connection.IsOpen)
                    return;
                var newStats = new SortedDictionary<FactoryWrapperScore, IConnectionFactoryWrapper>();

                bool gotConnection = false;
                foreach (var item in _stats)
                {
                    try
                    {
                        if (!gotConnection)
                        {
                            _connection = item.Key.FactoryWrapper.CreateConnection();
                            if(!_connection.IsOpen)
                                throw new ConnectionNotOpenException();

                            _connection.ConnectionShutdown += _connection_ConnectionShutdown;
                            gotConnection = true;
                        }
                    }
                    catch (SocketException socketException)
                    {
                        TheTrace.TraceWarning(socketException.ToString());
                        item.Key.ErrorCount++;
                    }
                    catch (BrokerUnreachableException brokerUnreachableException)
                    {
                        TheTrace.TraceWarning(brokerUnreachableException.ToString());
                        item.Key.ErrorCount++;
                    }
                    catch (ConnectionNotOpenException)
                    {
                        TheTrace.TraceWarning("Connection not open");
                        item.Key.ErrorCount++;
                    }

                    newStats.Add(item.Key, item.Value);
                    
                }

                _stats = newStats;
            }

        }

        private void _connection_ConnectionShutdown(object sender,
            ShutdownEventArgs reason)
        {
            TheTrace.TraceWarning("Connection was shut down: {0}", reason.ReplyText);
            BuildConnection();
        }
    }

}
