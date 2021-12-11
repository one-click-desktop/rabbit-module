using System;
using System.Collections.Generic;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using RabbitMQ.Client;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    public class TestRabbitClient : AbstractRabbitClient
    {
        public TestRabbitClient(string hostname, int port) : base(hostname, port)
        {
        }
        
        public new IModel Channel => base.Channel;

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
            base.Publish(exchangeName, routingKey, new TestRabbitMessage(type, message));
        }

        public class TestRabbitMessage : IRabbitMessage
        {
            public TestRabbitMessage(string type, object message)
            {
                MessageType = type;
                MessageBody = message;
            }

            public string SenderIdentifier { get; set; }
            public string MessageType { get; set; }
            public object MessageBody { get; set; }
        }
    }
}