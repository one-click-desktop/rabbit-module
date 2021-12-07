using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.RabbitModule.Common.Tests;
using RabbitMQ.Client;
using Constants = OneClickDesktop.RabbitModule.Common.Constants;

namespace OneClickDesktop.RabbitModule.VirtualizationServer.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class VirtualizationServerClientTest
    {
        private VirtualizationServerClient client;
        private TestRabbitClient helper;

        [SetUp]
        public void Setup()
        {
            string rabbitUrl = TestContext.Parameters["RabbitUrl"];
            int rabbitPort = int.Parse(TestContext.Parameters["RabbitPort"]);
            helper = new TestRabbitClient(rabbitUrl, rabbitPort);
            helper.Channel.ExchangeDeclare(Constants.Exchanges.VirtServersCommon, ExchangeType.Fanout);
            helper.Channel.ExchangeDeclare(Constants.Exchanges.VirtServersDirect, ExchangeType.Direct);
            
            client = new VirtualizationServerClient(
                rabbitUrl,
                rabbitPort,
                new Dictionary<string, Type>() {{"string", typeof(string)}});
        }

        [Test, Timeout(3000)]
        public void ShouldRaiseEventWhenCommonMessageReceived()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            client.CommonReceived += (model, msg) =>
            {
                messageReceived = (string) msg.RabbitMessage.Message;
                autoResetEvent.Set();
            };

            // when
            const string message = "message";
            helper.SetOnReturn(autoResetEvent);
            helper.Channel.SendMessage(Constants.Exchanges.VirtServersCommon,
                                       "", message, "string");
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
        
        [Test, Timeout(2000)]
        public void ShouldRaiseEventWhenDirectMessageReceived()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            client.DirectReceived += (model, msg) =>
            {
                messageReceived = (string) msg.RabbitMessage.Message;
                autoResetEvent.Set();
            };

            // when
            const string message = "message";
            helper.SetOnReturn(autoResetEvent);
            helper.Channel.SendMessage(Constants.Exchanges.VirtServersDirect,
                                       client.DirectQueueName,
                                       message, "string");
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
        
        [Test, Timeout(2000)]
        public void SendToOverseersShouldSendMessageToAllQueuesAtExchange()
        {
            // having
            helper.Channel.ExchangeDeclare(Constants.Exchanges.Overseers, ExchangeType.Fanout, autoDelete: true);
            
            var autoResetEvent1 = new AutoResetEvent(false);
            var autoResetEvent2 = new AutoResetEvent(false);
            var messageReceived1 = string.Empty;
            var messageReceived2 = string.Empty;
            
            var queue1 = helper.Channel.AnonymousQueueBindAndConsume<string>(
                Constants.Exchanges.Overseers,
                (model, msg) =>
                {
                    messageReceived1 = msg;
                    autoResetEvent1.Set();
                });
            var queue2 = helper.Channel.AnonymousQueueBindAndConsume<string>(
                Constants.Exchanges.Overseers,
                (model, msg) =>
                {
                    messageReceived2 = msg;
                    autoResetEvent2.Set();
                });
            
            // when
            const string message = "message";
            client.SetOnReturn(autoResetEvent1, autoResetEvent2);
            client.SendToOverseers(new TestRabbitClient.TestRabbitMessage("string", message));
            
            // then
            Assert.IsTrue(autoResetEvent1.WaitOne());
            Assert.AreEqual(message, messageReceived1);
            
            Assert.IsTrue(autoResetEvent2.WaitOne());
            Assert.AreEqual(message, messageReceived2);
        }
    }
}