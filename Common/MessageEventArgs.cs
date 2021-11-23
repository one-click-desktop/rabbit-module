using System;

namespace OneClickDesktop.RabbitModule.Common
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs()
        {
        }

        public MessageEventArgs(object message, string type)
        {
            Message = message;
            Type = type;
        }

        public object Message { get; set; }
        
        public string Type { get; set; }
    }
}