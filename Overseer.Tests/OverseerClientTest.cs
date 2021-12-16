using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.RabbitModule.Common;
using OneClickDesktop.RabbitModule.Common.Tests;

namespace OneClickDesktop.RabbitModule.Overseer.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class OverseerClientTest
    {
        private OverseerClient client;
        private TestRabbitClient helper;

        [SetUp]
        public void Setup()
        {
            string rabbitUrl = TestContext.Parameters["RabbitUrl"];
            int rabbitPort = int.Parse(TestContext.Parameters["RabbitPort"]);
            client = new OverseerClient(
                rabbitUrl,
                rabbitPort,
                new Dictionary<string, Type>() {{"string", typeof(string)}});
            helper = new TestRabbitClient(rabbitUrl, rabbitPort);
        }

        [Test, Timeout(2000)]
        public void ShouldRaiseEventWhenMessageReceived()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            client.Received += (model, msg) =>
            {
                messageReceived = (string) msg.RabbitMessage.MessageBody;
                autoResetEvent.Set();
            };
            
            // when
            const string message = "message";
            helper.SetOnReturn(autoResetEvent);
            helper.Channel.SendMessage(Constants.Exchanges.Overseers, "", message, "string");
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
        
        [Test, Timeout(2000)]
        public void SendToVirtualServerShouldSendMessageToSpecifiedQueue()
        {
            // having
            var autoResetEvent = new AutoResetEvent(false);
            var messageReceived = string.Empty;
            
            var queue = helper.Channel.AnonymousQueueBindAndConsume<string>(
                Constants.Exchanges.VirtServersDirect,
                (model, msg) =>
                {
                    messageReceived = msg;
                    autoResetEvent.Set();
                });
            
            // when
            const string message = "message";
            client.SetOnReturn(autoResetEvent);
            client.SendToVirtServer(queue, new TestRabbitClient.TestRabbitMessage("string", message));
            
            // then
            Assert.IsTrue(autoResetEvent.WaitOne());
            Assert.AreEqual(message, messageReceived);
        }
        
        [Test, Timeout(2000)]
        public void SendToAllVirtualServersShouldSendMessageToAllQueuesAtExchange()
        {
            // having
            var autoResetEvent1 = new AutoResetEvent(false);
            var autoResetEvent2 = new AutoResetEvent(false);
            var messageReceived1 = string.Empty;
            var messageReceived2 = string.Empty;
            
            var queue1 = helper.Channel.AnonymousQueueBindAndConsume<string>(
                Constants.Exchanges.VirtServersCommon,
                (model, msg) =>
                {
                    messageReceived1 = msg;
                    autoResetEvent1.Set();
                });
            var queue2 = helper.Channel.AnonymousQueueBindAndConsume<string>(
                Constants.Exchanges.VirtServersCommon,
                (model, msg) =>
                {
                    messageReceived2 = msg;
                    autoResetEvent2.Set();
                });
            
            // when
            const string message = "message";
            client.SetOnReturn(autoResetEvent1, autoResetEvent2);
            client.SendToAllVirtServers(new TestRabbitClient.TestRabbitMessage("string", message));
            
            // then
            Assert.IsTrue(autoResetEvent1.WaitOne());
            Assert.AreEqual(message, messageReceived1);
            
            Assert.IsTrue(autoResetEvent2.WaitOne());
            Assert.AreEqual(message, messageReceived2);
        }
    }
}