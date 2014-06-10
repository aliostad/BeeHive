using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace BeeHive.RabbitMQ
{
    public interface IConnectionProvider
    {
        IConnection GetConnection();
    }

    public class ConnectionProvider : IConnectionProvider
    {
        private ConnectionFactory _factory;

        // TODO: implement HA
        public ConnectionProvider(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public IConnection GetConnection()
        {
            // TODO replace with HA
            return _factory.CreateConnection();
        }
    }
}
