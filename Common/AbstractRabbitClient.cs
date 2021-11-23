using System;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OneClickDesktop.RabbitModule.Common
{
    public class AbstractRabbitClient : IDisposable
    {
        private readonly ConnectionFactory factory;
        private readonly IConnection connection;

        protected IModel Channel { get; private set; }
        
        protected AbstractRabbitClient(string hostname, int port)
        {
            factory = new ConnectionFactory() {HostName = hostname, Port = port};
            connection = factory.CreateConnection();
            
            Channel = connection.CreateModel();
            Channel.BasicQos(0, 1, false);
        }

        /// <summary>
        /// Creates and binds anonymous queue to specified exchange with routing key
        /// </summary>
        /// <param name="exchangeName">Name of exchange</param>
        /// <param name="routingKey">Routing key. If null uses queue name</param>
        /// <returns>Name of anonymous queue</returns>
        protected string BindAnonymousQueue(string exchangeName, string routingKey)
        {
            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queueName, exchangeName, routingKey ?? queueName);
            
            return queueName;
        }
        
        /// <summary>
        /// Starts consuming from specified queue with specified handler
        /// </summary>
        /// <param name="queueName">Name of queue to consume from</param>
        /// <param name="autoAck">Automatically acknowledge messages</param>
        /// <param name="handler"></param>
        protected void Consume(string queueName, bool autoAck, EventHandler<MessageEventArgs> handler)
        {
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<object>(body);

                var type = ea.BasicProperties.Type;

                handler(this, new MessageEventArgs(message, type));

                if (!autoAck)
                {
                    Channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            Channel.BasicConsume(queueName, autoAck, consumer);
        }
        
        /// <summary>
        /// Publish message to specified exchange with routing key and properties
        /// </summary>
        /// <param name="exchangeName">Name of exchange</param>
        /// <param name="routingKey">Routing key used to decide which queue to publish to</param>
        /// <param name="properties">Properties of message</param>
        /// <param name="message">Message body</param>
        protected void Publish(string exchangeName, string routingKey, string type, object message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var props = CreateProperties(type);
            
            Channel.BasicPublish(exchangeName, routingKey, props, body);
        }

        protected IBasicProperties CreateProperties(string type)
        {
            var props = Channel.CreateBasicProperties();
            props.Type = type;

            return props;
        }

        public void Dispose()
        {
            Channel?.Close();
            Channel?.Dispose();
            
            connection?.Close();
            connection?.Dispose();
        }
    }
}