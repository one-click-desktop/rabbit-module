using System;
using System.Collections.Generic;
using OneClickDesktop.RabbitModule.Common;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using RabbitMQ.Client;
using Constants = OneClickDesktop.RabbitModule.Common.Constants;

namespace OneClickDesktop.RabbitModule.Overseer
{
    public class OverseerClient: AbstractRabbitClient
    {
        /// <summary>
        /// Creates RabbitMQ client for Overseer and establishes connection
        /// </summary>
        /// <param name="hostname">Hostname of RabbitMQ server</param>
        /// <param name="port">RabbitMQ server port</param>
        /// <param name="messageTypeMapping">Dictionary grouping message type as received in Rabbit message with C# type to deserialize into.
        /// Types not in the dictionary will be skipped.</param>
        public OverseerClient(string hostname, int port, IReadOnlyDictionary<string, Type> messageTypeMapping)
            : base(hostname, port)
        {
            BindToOverseersExchange(messageTypeMapping);
            CreateVirtServersCommonExchange();
            CreateVirtServersDirectExchange();
        }

        /// <summary>
        /// Event raised when message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> Received;

        private void BindToOverseersExchange(IReadOnlyDictionary<string, Type> messageTypeMapping)
        {
            Channel.ExchangeDeclare(Constants.Exchanges.Overseers, ExchangeType.Fanout, autoDelete: true);

            var queueName = BindAnonymousQueue(Constants.Exchanges.Overseers, "");
            Consume(queueName, true, (sender, args) => Received?.Invoke(sender, args), messageTypeMapping);
        }

        private void CreateVirtServersCommonExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersCommon, ExchangeType.Fanout, autoDelete: false);
        }

        private void CreateVirtServersDirectExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersDirect, ExchangeType.Direct, autoDelete: false);
        }
        
        /// <summary>
        /// Send message to common virtual servers exchange
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <param name="type">Message type</param>
        public void SendToAllVirtServers(IRabbitMessage message)
        {
            Publish(Constants.Exchanges.VirtServersCommon, "", message);
        }
        
        /// <summary>
        /// Send message directly to virtual server
        /// </summary>
        /// <param name="queueName">Name of server's queue</param>
        /// <param name="message">Message to publish</param>
        /// <param name="type">Message type</param>
        public void SendToVirtServer(string queueName, IRabbitMessage message)
        {
            Publish(Constants.Exchanges.VirtServersDirect, queueName, message);
        }
    }
}