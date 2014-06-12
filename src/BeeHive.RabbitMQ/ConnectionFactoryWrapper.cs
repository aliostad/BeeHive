using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace BeeHive.RabbitMQ
{
    public class ConnectionFactoryWrapper : IConnectionFactoryWrapper
    {
        private ConnectionFactory _factory;

        public ConnectionFactoryWrapper(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }
    }
}
