using System;
using OneClickDesktop.RabbitModule.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Constants = OneClickDesktop.RabbitModule.Common.Constants;

namespace OneClickDesktop.RabbitModule.VirtualizationServer
{
    public class VirtualizationServerClient: AbstractRabbitClient
    {
        public string DirectQueueName { get; private set; }
        
        /// <summary>
        /// Creates RabbitMQ client for Virtual Server and establishes connection
        /// </summary>
        /// <param name="hostname">Hostname of RabbitMQ server</param>
        /// <param name="port">RabbitMQ server port</param>
        public VirtualizationServerClient(string hostname, int port)
            : base(hostname, port)
        {
            BindToCommonExchange();
            BindToDirectExchange();
        }

        /// <summary>
        /// Event raised when direct message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> DirectReceived;
        
        /// <summary>
        /// Event raised when common message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> CommonReceived;

        private void BindToCommonExchange()
        {
            var queueName = BindAnonymousQueue(Constants.Exchanges.VirtServersCommon, "");
            Consume(queueName, false, (sender, args) => CommonReceived?.Invoke(sender, args));
        }
        
        private void BindToDirectExchange()
        {
            DirectQueueName = BindAnonymousQueue(Constants.Exchanges.VirtServersDirect, null);
            Consume(DirectQueueName, false, (sender, args) => DirectReceived?.Invoke(sender, args));
        }
        
        /// <summary>
        /// Send message to all overseers
        /// </summary>
        /// <param name="message">Message body</param>
        /// <param name="type">Type of message</param>
        public void SendToOverseers(object message, string type)
        {
            Publish(Constants.Exchanges.Overseers, "", type, message);
        }
    }
}