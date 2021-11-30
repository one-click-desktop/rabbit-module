using System;

namespace OneClickDesktop.RabbitModule.Common.EventArgs
{
    public class MessageEventArgs : System.EventArgs
    {
        public object Message { get; }
        
        public string Type { get; }

        public MessageEventArgs(object message, string type)
        {
            Message = message;
            Type = type;
        }
    }
}