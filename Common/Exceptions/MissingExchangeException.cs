using System;

namespace OneClickDesktop.RabbitModule.Common.Exceptions
{
    public class MissingExchangeException : Exception
    {
        public MissingExchangeException()
        {
        }

        public MissingExchangeException(string message) : base(message)
        {
        }
    }
}