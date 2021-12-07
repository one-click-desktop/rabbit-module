using System;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.RabbitModule.Common.EventArgs
{
    public class MessageEventArgs : System.EventArgs
    {
        public IRabbitMessage RabbitMessage { get; set; }

        public MessageEventArgs(IRabbitMessage message)
        {
            RabbitMessage = message;
        }
    }
}