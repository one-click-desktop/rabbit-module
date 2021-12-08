using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
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

            RestoreChannel();
        }

        /// <summary>
        /// Event raised when message cannot be delivered. If return reason is NO_EXCHANGE 
        /// </summary>
        public event EventHandler<ReturnEventArgs> Return;

        /// <summary>
        /// Function responsible for restoring channel to new one
        /// </summary>
        protected void RestoreChannel()
        {
            Channel = connection.CreateModel();
            Channel.BasicQos(0, 1, false);
            Channel.BasicReturn += (sender, args) => Return?.Invoke(sender, new ReturnEventArgs(args));
            Channel.ModelShutdown += (sender, args) =>
            {
                Return?.Invoke(sender, new ReturnEventArgs(args));
            }; 
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
        /// <param name="handler">Message handler</param>
        /// <param name="typeDict">Mapping of message type to C# type</param>
        protected void Consume(string queueName, bool autoAck, EventHandler<MessageEventArgs> handler, IReadOnlyDictionary<string, Type> typeDict)
        {
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var type = ea.BasicProperties.Type;
                var appId = ea.BasicProperties.AppId;

                if (!typeDict.TryGetValue(type, out var messageType) || messageType == null)
                {
                    Console.WriteLine($"Message type {type} unknown");
                    return;
                }

                object message = null;
                try
                {
                    var deserializer = typeof(JsonSerializer).GetMethods()
                                                             .Where(x => x.Name == "Deserialize")
                                                             .FirstOrDefault(x => x.IsGenericMethod
                                                                                 && x.GetParameters()[0]
                                                                                     .ParameterType == typeof(string))
                                                             ?.MakeGenericMethod(messageType);
                    message = deserializer?.Invoke(null, new object[] {body, null});
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"Failed to deserialize message of type: {type} to: {messageType.Name}, content: {body}", e);
                    return;
                }
                handler(this, new MessageEventArgs(new RabbitMessage.RabbitMessage(appId, type, message)));

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
        /// <param name="message">Message to send with metadata</param>
        protected void Publish(string exchangeName, string routingKey, IRabbitMessage message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message.Message);
            var props = CreateProperties(message.Type, message.AppId);
            
            Channel.BasicPublish(exchangeName, routingKey, true, props, body);
        }

        private IBasicProperties CreateProperties(string type, string appId)
        {
            var props = Channel.CreateBasicProperties();
            props.Type = type;
            props.AppId = appId;
            props.ContentType = "application/json";

            return props;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
            Channel?.Close();
            Channel?.Dispose();
            
            connection?.Close();
            connection?.Dispose();
        }
    }
}