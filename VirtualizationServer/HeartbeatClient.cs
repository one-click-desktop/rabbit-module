using OneClickDesktop.RabbitModule.Common;

namespace OneClickDesktop.RabbitModule.VirtualizationServer
{
    public class HeartbeatClient: AbstractRabbitClient
    {
        // TODO: process that check after some time if queues in list exist, if queue doesn't exist in specified number continuous checks then call callback
        protected HeartbeatClient(string hostname, int port) : base(hostname, port)
        {
            
        }
    }
}