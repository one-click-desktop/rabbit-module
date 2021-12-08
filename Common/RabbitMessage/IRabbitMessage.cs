namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    public interface IRabbitMessage
    {
        string AppId { get; set; }
        string Type { get; set; }
        object Message { get; set; }
    }
}