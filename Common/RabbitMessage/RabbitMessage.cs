namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    internal class RabbitMessage : IRabbitMessage
    {
        public string SenderIdentifier { get; set; }
        public string MessageType { get; set; }
        public object MessageBody { get; set; }

        internal RabbitMessage(string appId, string type, object message)
        {
            SenderIdentifier = appId;
            MessageType = type;
            MessageBody = message;
        }
    }
}