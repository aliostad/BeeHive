using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace BeeHive.RabbitMQ.Tests
{
    public class ConnectionPtoviderTests
    {
        [Fact]
        public void ConnectionFactoryWrappersCannotBeEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new ConnectionProvider());
        }

        [Fact]
        public void FirstThrowsSocketUsesSecondAndWorksTwice()
        {

            var d1 = new DummyConnectionFactoryWrapper(new SocketException(123));
            var d2 = new DummyConnectionFactoryWrapper(true);

            var cp = new ConnectionProvider(d2, d1);
           
            var connection = cp.GetConnection();
            Assert.NotNull(connection);
            var connection2 = cp.GetConnection();
            Assert.NotNull(connection2);

        }

        [Fact]
        public void FirstConnectionNoyOpenUsesSecondAndWorksTwice()
        {

            var d1 = new DummyConnectionFactoryWrapper(false);
            var d2 = new DummyConnectionFactoryWrapper(true);

            var cp = new ConnectionProvider(d2, d1);

            var connection = cp.GetConnection();
            Assert.NotNull(connection);
            var connection2 = cp.GetConnection();
            Assert.NotNull(connection2);
            Assert.Equal(1, d1.NumberOfTimesCalled);
        }
    }

    public class DummyConnection : IConnection
    {
        private bool _isOpen;

        public DummyConnection(bool isOpen)
        {
            _isOpen = isOpen;
        }
 
        public void Dispose()
        {
           
        }

        public IModel CreateModel()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Close(ushort reasonCode, string reasonText)
        {
            throw new NotImplementedException();
        }

        public void Close(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public void Abort(ushort reasonCode, string reasonText)
        {
            throw new NotImplementedException();
        }

        public void Abort(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            throw new NotImplementedException();
        }

        public void HandleConnectionBlocked(string reason)
        {
            throw new NotImplementedException();
        }

        public void HandleConnectionUnblocked()
        {
            throw new NotImplementedException();
        }

        public AmqpTcpEndpoint Endpoint { get; private set; }
        public IProtocol Protocol { get; private set; }
        public ushort ChannelMax { get; private set; }
        public uint FrameMax { get; private set; }
        public ushort Heartbeat { get; private set; }
        public IDictionary<string, object> ClientProperties { get; private set; }
        public IDictionary<string, object> ServerProperties { get; private set; }
        public AmqpTcpEndpoint[] KnownHosts { get; private set; }
        public ShutdownEventArgs CloseReason { get; private set; }

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public bool AutoClose { get; set; }
        public IList<ShutdownReportEntry> ShutdownReport { get; private set; }

        public string ClientProvidedName => throw new NotImplementedException();

        public ConsumerWorkService ConsumerWorkService => throw new NotImplementedException();

        public int LocalPort => throw new NotImplementedException();

        public int RemotePort => throw new NotImplementedException();

        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
        public event EventHandler<EventArgs> ConnectionUnblocked;
        public event EventHandler<EventArgs> RecoverySucceeded;
        public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError;
    }

    public class DummyConnectionFactoryWrapper : IConnectionFactoryWrapper
    {
        private bool _isOpen;
        private Exception _exception;

        public DummyConnectionFactoryWrapper(bool isOpen)
        {
            _isOpen = isOpen;
        }

        public DummyConnectionFactoryWrapper(Exception exception)
        {
            _exception = exception;
        }

        public IConnection CreateConnection()
        {
            NumberOfTimesCalled++;
            if(_exception == null)
                return new DummyConnection(_isOpen);

            throw _exception;
        }

        public int NumberOfTimesCalled { get; private set; }
    }
}
