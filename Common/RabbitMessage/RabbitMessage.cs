namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    internal class RabbitMessage : IRabbitMessage
    {
        public string SenderIdentifier { get; set; }
        public string Type { get; set; }
        public object Body { get; set; }

        internal RabbitMessage(string appId, string type, object message)
        {
            SenderIdentifier = appId;
            Type = type;
            Body = message;
        }
    }
}