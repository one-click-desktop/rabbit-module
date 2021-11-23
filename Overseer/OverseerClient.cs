using System;
using OneClickDesktop.RabbitModule.Common;
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
        public OverseerClient(string hostname, int port)
            : base(hostname, port)
        {
            BindToOverseersExchange();
            CreateVirtServersCommonExchange();
            CreateVirtServersDirectExchange();
        }

        public event EventHandler<MessageEventArgs> Received;

        private void BindToOverseersExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.Overseers, ExchangeType.Fanout);

            var queueName = BindAnonymousQueue(Constants.Exchanges.Overseers, "");
            Consume(queueName, true, (sender, args) => Received?.Invoke(sender, args));
        }

        private void CreateVirtServersCommonExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersCommon, ExchangeType.Fanout, autoDelete: true);
        }

        private void CreateVirtServersDirectExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersDirect, ExchangeType.Direct, autoDelete: true);
        }
        
        /// <summary>
        /// Send message to common virtual servers exchange
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <param name="type">Message type</param>
        public void SendToAllVirtServers(object message, string type)
        {
            Publish(Constants.Exchanges.VirtServersCommon, "", type, message);
        }
        
        /// <summary>
        /// Send message directly to virtual server
        /// </summary>
        /// <param name="queueName">Name of server's queue</param>
        /// <param name="message">Message to publish</param>
        /// /// <param name="type">Message type</param>
        public void SendToVirtServer(string queueName, object message, string type)
        {
            Publish(Constants.Exchanges.VirtServersDirect, queueName, type, message);
        }
    }
}