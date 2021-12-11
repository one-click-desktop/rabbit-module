namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    public interface IRabbitMessage
    {
        string SenderIdentifier { get; set; }
        string MessageType { get; set; }
        object MessageBody { get; set; }
    }
}