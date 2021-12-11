using System;

namespace OneClickDesktop.RabbitModule.Common.Exceptions
{
    public class BrokerConnectionException : Exception
    {
        public BrokerConnectionException()
        {
        }

        public BrokerConnectionException(string message) : base(message)
        {
        }
    }
}