using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.RabbitModule.Common.Tests;
using RabbitMQ.Client;
using Constants = OneClickDesktop.RabbitModule.Common.Constants;

namespace OneClickDesktop.RabbitModule.VirtualizationServer.Tests
{
    public class VirtualizationServerClientTest
    {
        private VirtualizationServerClient client;
        private TestRabbitClient helper;

        [SetUp]
        public void Setup()
        {
            helper = new TestRabbitClient("localhost", 5672);
            helper.Channel.ExchangeDeclare(Constants.Exchanges.VirtServersCommon, ExchangeType.Fanout);
            helper.Channel.ExchangeDeclare(Constants.Exchanges.VirtServersDirect, ExchangeType.Direct);
            
            client = new VirtualizationServerClient(
                "localhost",
                5672,
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
                messageReceived = (string) msg.Message;
                autoResetEvent.Set();
            };

            // when
            const string message = "message";
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
                messageReceived = (string) msg.Message;
                autoResetEvent.Set();
            };

            // when
            const string message = "message";
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
            client.SendToOverseers(message, "string");
            
            // then
            Assert.IsTrue(autoResetEvent1.WaitOne());
            Assert.AreEqual(message, messageReceived1);
            
            Assert.IsTrue(autoResetEvent2.WaitOne());
            Assert.AreEqual(message, messageReceived2);
        }
    }
}