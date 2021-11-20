using System;
using OneClickDesktop.RabbitModule.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
        /// <param name="handler">Handler for messages received on common Overseer exchange</param>
        protected OverseerClient(string hostname, int port, EventHandler<BasicDeliverEventArgs> handler)
            : base(hostname, port)
        {
            BindToOverseersExchange(handler);
            CreateVirtServersCommonExchange();
            CreateVirtServersDirectExchange();
        }

        private void BindToOverseersExchange(EventHandler<BasicDeliverEventArgs> handler)
        {
            Channel.ExchangeDeclare(Constants.Exchanges.Overseers, ExchangeType.Fanout);

            var queueName = BindAnonymousQueue(Constants.Exchanges.Overseers, "");
            Consume(queueName, true, handler);
        }

        private void CreateVirtServersCommonExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersCommon, ExchangeType.Fanout);
        }

        private void CreateVirtServersDirectExchange()
        {
            Channel.ExchangeDeclare(Constants.Exchanges.VirtServersDirect, ExchangeType.Direct);
        }
        
        /// <summary>
        /// Send message to common virtual servers exchange
        /// </summary>
        /// <param name="message">Message to publish</param>
        public void SendToAllVirtServers(Object message)
        {
            Publish(Constants.Exchanges.VirtServersCommon, "", null, message);
        }
        
        public void SendToVirtServer(string queueName, Object message)
        {
            Publish(Constants.Exchanges.VirtServersDirect, queueName, null, message);
        }
    }
}