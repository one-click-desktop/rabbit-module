using System;
using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OneClickDesktop.RabbitModule.Common.EventArgs
{
    public class ReturnEventArgs: System.EventArgs
    {
        public string Exchange { get; }
        public string RoutingKey { get; }
        public ushort ReplyCode { get; }
        public string ReplyText { get; }
        public ReadOnlyMemory<byte> Message { get; }
        
        public Reason ReturnReason { get; }

        public ReturnEventArgs(string exchange, string routingKey, ushort replyCode, string replyText, ReadOnlyMemory<byte> message)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            ReplyCode = replyCode;
            ReplyText = replyText;
            Message = message;
            ReturnReason = replyCode switch
            {
                200 => Reason.GOODBYE,
                312 => Reason.NO_QUEUE,
                404 => Reason.NO_EXCHANGE,
                _ => Reason.UNKNOWN
            };
        }

        public ReturnEventArgs(BasicReturnEventArgs args) 
            : this(args.Exchange, args.RoutingKey, args.ReplyCode, args.ReplyText, args.Body)   
        {
        }

        public ReturnEventArgs(ShutdownEventArgs args)
            : this(null, null, args.ReplyCode, args.ReplyText, null)   
        {
        }
        
        public enum Reason
        {
            NO_QUEUE,
            NO_EXCHANGE,
            UNKNOWN,
            GOODBYE
        }
    }
}