using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    public class TestRabbitClient : AbstractRabbitClient
    {
        public TestRabbitClient(string hostname, int port) : base(hostname, port)
        {
        }
        
        public new IModel Channel
        {
            get { return base.Channel; }
        }

        public new void RestoreChannel()
        {
            base.RestoreChannel();
        }

        public new string BindAnonymousQueue(string exchange, string routingKey)
        {
            return base.BindAnonymousQueue(exchange, routingKey);
        }

        public new void Consume(string queueName,
                                bool autoAck,
                                EventHandler<MessageEventArgs> handler,
                                Dictionary<string, Type> messageTypeMapping)
        {
            base.Consume(queueName, autoAck, handler, messageTypeMapping);
        }

        public new void Publish(string exchangeName, string routingKey, string type, object message)
        {
            base.Publish(exchangeName, routingKey, type, message);
        }
    }
}