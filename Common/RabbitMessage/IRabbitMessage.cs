namespace OneClickDesktop.RabbitModule.Common.RabbitMessage
{
    /// <summary>
    /// Message used for communication using RabbitMQ
    /// </summary>
    public interface IRabbitMessage
    {
        /// <summary>
        /// Identifier of a sender
        /// </summary>
        string SenderIdentifier { get; set; }
        
        /// <summary>
        /// Type of message, used to properly handle message by receiver (as agreed by participants)
        /// </summary>
        string Type { get; set; }
        
        /// <summary>
        /// Message body, deserialized on receive (cast according to <see cref="Type">Type</see>)
        /// </summary>
        object Body { get; set; }
    }
}