using System;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OneClickDesktop.RabbitModule.Common.Tests
{
    public static class RabbitTestHelper
    {
        public static void SendMessage(this IModel channel, string exchange, string routingKey, string message)
        {
            channel.BasicPublish(exchange,
                                 routingKey,
                                 null,
                                 JsonSerializer.SerializeToUtf8Bytes(message));
        }

        public static void Consume(this IModel channel, string queue, EventHandler<string> handler)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                handler(channel, JsonSerializer.Deserialize<string>(ea.Body.ToArray()));
            };
            
            channel.BasicConsume(queue, true, consumer);
        }
    }
}