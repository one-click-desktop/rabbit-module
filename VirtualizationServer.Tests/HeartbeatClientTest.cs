using System;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.RabbitModule.Common.Tests;
using RabbitMQ.Client;

namespace OneClickDesktop.RabbitModule.VirtualizationServer.Tests
{
    public class HeartbeatClientTest
    {
        private HeartbeatClient client;
        private TestRabbitClient helper;
        
        [SetUp]
        public void Setup()
        {
            client = new HeartbeatClient("localhost", 5672, 100, 1);
            helper = new TestRabbitClient("localhost", 5672);
        }

        [Test, Timeout(2000)]
        public void ShouldRaiseEventWhenRegisteredQueueDisappears()
        {
            // having
            var queueMissing = new AutoResetEvent(false);
            var reportedQueue = string.Empty;
            
            const string queue = "queue";
            helper.Channel.QueueDeclare(queue);

            client.Missing += (model, q) =>
            {
                reportedQueue = q;
                queueMissing.Set();
            };
            client.RegisterQueue(queue);
            
            // when
            helper.Dispose();

            // then
            Assert.IsTrue(queueMissing.WaitOne());
            Assert.AreEqual(queue, reportedQueue);
        }
        
        [Test, Timeout(2000)]
        public void ShouldRaiseEventWhenRegisteredQueueIsFound()
        {
            // having
            var queueMissing = new AutoResetEvent(false);
            var queueFound = new AutoResetEvent(false);
            var reportedQueue = string.Empty;
            
            const string queue = "queue";
            helper.Channel.QueueDeclare(queue);

            client.Missing += (model, q) =>
            {
                queueMissing.Set();
            };
            client.Found += (model, q) =>
            {
                reportedQueue = q;
                queueFound.Set();
            };
            client.RegisterQueue(queue);
            
            // when
            helper.Dispose();
            queueMissing.WaitOne();

            helper = new TestRabbitClient("localhost", 5672);
            helper.Channel.QueueDeclare(queue);

            // then
            Assert.IsTrue(queueFound.WaitOne());
            Assert.AreEqual(queue, reportedQueue);
        }
        
        [Test, Timeout(3000)]
        public void ShouldNotRaiseEventWhenQueueUnregisters()
        {
            // having
            var queueMissing = new AutoResetEvent(false);
            var reportedQueue = string.Empty;
            
            const string queue = "queue";
            helper.Channel.QueueDeclare(queue);

            client.Missing += (model, q) =>
            {
                reportedQueue = q;
                queueMissing.Set();
            };
            client.RegisterQueue(queue);
            
            // when
            client.RemoveQueue(queue);
            helper.Dispose();

            // then
            Assert.IsFalse(queueMissing.WaitOne(1000));
        }
    }
}