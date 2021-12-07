namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    internal class RabbitMessage : IRabbitMessage
    {
        public string AppId { get; set; }
        public string Type { get; set; }
        public object Message { get; set; }

        internal RabbitMessage(string appId, string type, object message)
        {
            AppId = appId;
            Type = type;
            Message = message;
        }
    }
}