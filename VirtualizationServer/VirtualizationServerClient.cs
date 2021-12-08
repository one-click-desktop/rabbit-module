using System;
using System.Collections.Generic;
using OneClickDesktop.RabbitModule.Common;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
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
        /// <param name="messageTypeMapping">Dictionary grouping message type as received in Rabbit message with C# type to deserialize into.
        /// Types not in the dictionary will be skipped.</param>
        public VirtualizationServerClient(string hostname, int port, IReadOnlyDictionary<string, Type> messageTypeMapping)
            : base(hostname, port)
        {
            BindToCommonExchange(messageTypeMapping);
            BindToDirectExchange(messageTypeMapping);
        }

        /// <summary>
        /// Event raised when direct message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> DirectReceived;
        
        /// <summary>
        /// Event raised when common message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> CommonReceived;

        private void BindToCommonExchange(IReadOnlyDictionary<string, Type> messageTypeMapping)
        {
            var queueName = BindAnonymousQueue(Constants.Exchanges.VirtServersCommon, "");
            Consume(queueName, true, (sender, args) => CommonReceived?.Invoke(sender, args), messageTypeMapping);
        }
        
        private void BindToDirectExchange(IReadOnlyDictionary<string, Type> messageTypeMapping)
        {
            DirectQueueName = BindAnonymousQueue(Constants.Exchanges.VirtServersDirect, null);
            Consume(DirectQueueName, true, (sender, args) => DirectReceived?.Invoke(sender, args), messageTypeMapping);
        }
        
        /// <summary>
        /// Send message to all overseers
        /// </summary>
        /// <param name="message">Message body</param>
        /// <param name="type">Type of message</param>
        public void SendToOverseers(IRabbitMessage message)
        {
            Publish(Constants.Exchanges.Overseers, "", message);
        }
    }
}