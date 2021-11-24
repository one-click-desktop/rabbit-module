using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    public class AbstractRabbitClientTest
    {
        private TestRabbitClient client;
        
        [SetUp]
        public void Setup()
        {
            client = new TestRabbitClient("localhost", 5672);
        }

        [Test]
        public void RestoreChannelShouldCreateNewChannel()
        {
            var channelHash = client.Channel.GetHashCode();
            
            client.RestoreChannel();

            Assert.IsFalse(channelHash == client.Channel.GetHashCode());
        }

        [Test, Timeout(2000)]
        public void BindAnonymousQueueShouldCreateQueueAndBindToExchangeWithRoutingKey()
        {
            // having
            const string routingKey = "routingKey";
            const string exchange = "exchange";
            client.Channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            // when
            var queue = client.BindAnonymousQueue(exchange, routingKey);
            client.Channel.Consume(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            var message = "message";
            client.Channel.SendMessage(exchange, routingKey, message);
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }

        [Test, Timeout(2000)]
        public void BindAnonymousQueueShouldCreateQueueAndBindToExchangeWithoutRoutingKey()
        {
            // having
            const string exchange = "exchange";
            client.Channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            // when
            var queue = client.BindAnonymousQueue(exchange, null);
            client.Channel.Consume(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            var message = "message";
            client.Channel.SendMessage(exchange, queue, message);
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }

        [Test, Timeout(2000)]
        public void ConsumeShouldRegisterCallbackForMessage()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            // when
            var queue = client.Channel.QueueDeclare().QueueName;
            client.Consume(queue, true, (model, msg) =>
            {
                messageReceived = msg.Message.ToString();
                autoResetEvent.Set();
            });

            var message = "message";
            client.Channel.SendMessage("", queue, message);
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }

        [Test, Timeout(2000)]
        public void PublishShouldPublishMessageToExchangeWithRoutingKey()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            // when
            var queue = client.Channel.QueueDeclare().QueueName;
            client.Channel.Consume(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            var message = "message";
            client.Publish("", queue, null, message);
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
    }
}