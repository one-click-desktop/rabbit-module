using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    public static class RabbitTestHelper
    {
        public static void SendMessage(
            this IModel channel, string exchange, string routingKey, string message, string type)
        {
            var props = channel.CreateBasicProperties();
            props.Type = type;
            
            channel.BasicPublish(exchange,
                                 routingKey,
                                 true,
                                 props,
                                 JsonSerializer.SerializeToUtf8Bytes(message));
        }

        public static void Consume<T>(this IModel channel, string queue, EventHandler<T> handler)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                handler(channel, JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray())));
            };
            
            channel.BasicConsume(queue, true, consumer);
        }

        public static string AnonymousQueueBindAndConsume<T>(
            this IModel channel, string exchange, EventHandler<T> handler)
        {
            var queue = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue, exchange, queue);
            channel.Consume<T>(queue, handler);
            return queue;
        }

        public static void SetOnReturn(this AbstractRabbitClient client, params AutoResetEvent[] autoResetEvents)
        {
            client.Return += (sender, args) =>
            {
                foreach (var autoResetEvent in autoResetEvents)
                {
                    autoResetEvent.Set();
                }
            };
        }
    }
}