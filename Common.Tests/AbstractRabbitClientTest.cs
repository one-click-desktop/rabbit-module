using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RabbitMQ.Client;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    [TestFixture]
    public class AbstractRabbitClientTest
    {
        private TestRabbitClient client;
        
        [SetUp]
        public void Setup()
        {
            string rabbitUrl = TestContext.Parameters["RabbitUrl"];
            int rabbitPort = int.Parse(TestContext.Parameters["RabbitPort"]);
            client = new TestRabbitClient(rabbitUrl, rabbitPort);
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
            client.Channel.Consume<string>(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            const string message = "message";
            client.Channel.SendMessage(exchange, routingKey, message, "string");
            
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
            client.Channel.Consume<string>(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            var message = "message";
            client.Channel.SendMessage(exchange, queue, message, "string");
            
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
                messageReceived = (string) msg.Message;
                autoResetEvent.Set();
            }, new Dictionary<string, Type>() {{"string", typeof(string)}});

            const string message = "message";
            client.Channel.SendMessage("", queue, message, "string");
            
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
            client.Channel.Consume<string>(queue, (model, msg) =>
            {
                messageReceived = msg;
                autoResetEvent.Set();
            });

            const string message = "message";
            client.Publish("", queue, null, message);
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
    }
}