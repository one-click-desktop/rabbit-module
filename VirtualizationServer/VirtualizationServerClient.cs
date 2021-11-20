using System;
using OneClickDesktop.RabbitModule.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Constants = OneClickDesktop.RabbitModule.Common.Constants;

namespace OneClickDesktop.RabbitModule.VirtualizationServer
{
    public class VirtualizationServerClient: AbstractRabbitClient
    {
        private string queueName;
        
        /// <summary>
        /// Creates RabbitMQ client for Overseer and establishes connection
        /// </summary>
        /// <param name="hostname">Hostname of RabbitMQ server</param>
        /// <param name="port">RabbitMQ server port</param>
        /// <param name="directHandler">Handler for messages received directly. Needs to acknowledge message</param>
        /// <param name="commonHandler">Handler for messages received on common virtualization servers exchange</param>
        protected VirtualizationServerClient(string hostname, int port, 
                                             EventHandler<BasicDeliverEventArgs> directHandler,
                                             EventHandler<BasicDeliverEventArgs> commonHandler)
            : base(hostname, port)
        {
            BindToCommonExchange(commonHandler);
            BindToDirectExchange(directHandler);
        }

        private void BindToCommonExchange(EventHandler<BasicDeliverEventArgs> commonHandler)
        {
            queueName = BindAnonymousQueue(Constants.Exchanges.VirtServersCommon, "");
            Consume(queueName, true, commonHandler);
        }
        
        private void BindToDirectExchange(EventHandler<BasicDeliverEventArgs> directHandler)
        {
            queueName = BindAnonymousQueue(Constants.Exchanges.VirtServersDirect, null);
            Consume(queueName, false, directHandler);
        }

        public void SendToOverseers(Object message)
        {
            Publish(Constants.Exchanges.Overseers, "", null, message);
        }
    }
}