using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneClickDesktop.RabbitModule.Common;
using RabbitMQ.Client.Exceptions;

namespace OneClickDesktop.RabbitModule.VirtualizationServer
{
    public class HeartbeatClient : AbstractRabbitClient, IDisposable
    {
        private readonly int waitTime;
        private readonly int checks;

        private readonly ConcurrentDictionary<string, (int missing, bool found)> queues =
            new ConcurrentDictionary<string, (int missing, bool found)>();
        private readonly Task checkTask;
        private readonly EventHandler<string> missingHandler;
        private readonly EventHandler<string> foundHandler;

        /// <summary>
        /// Creates Heartbeat client for Virtual Server and establishes connection
        /// </summary>
        /// <param name="hostname">Hostname of RabbitMQ server</param>
        /// <param name="port">RabbitMQ server port</param>
        /// <param name="waitTime">Wait time in milliseconds between checks</param>
        /// <param name="checks">Checks before counting queue as missing</param>
        public HeartbeatClient(string hostname, int port,
            int waitTime = 60000, int checks = 2) : base(hostname, port)
        {
            this.waitTime = waitTime;
            this.checks = checks;

            checkTask = new Task(() =>
            {
                while (true)
                {
                    foreach (var queue in queues.Keys)
                    {
                        var (missing, found) = queues[queue];
                        try
                        {
                            // channel is broken after previous try
                            RestoreChannel();
                            Channel.QueueDeclarePassive(queue);
                        }
                        catch (OperationInterruptedException e) when (e.ShutdownReason.ReplyCode == 404)
                        {
                            queues[queue] = (missing + 1, found: found);

                            // raise event if queue is missing checks times
                            if (queues[queue].missing == this.checks)
                            {
                                Missing?.Invoke(this, queue);
                            }

                            continue;
                        }
                        catch (OperationInterruptedException e) when (e.ShutdownReason.ReplyCode == 405)
                        {
                        }

                        // raise event if queue previously reported as missing is found or first check
                        if (missing >= this.checks || !found)
                        {
                            Found?.Invoke(this, queue);
                        }

                        queues[queue] = (0, true);
                    }

                    Thread.Sleep(this.waitTime);
                }
            });
            checkTask.Start();
        }

        /// <summary>
        /// Event raised when queue is declared as missing
        /// </summary>
        public event EventHandler<string> Missing;

        /// <summary>
        /// Event raised when queue is found after being declared as missing
        /// </summary>
        public event EventHandler<string> Found;

        /// <summary>
        /// Adds queue to check
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <exception cref="ArgumentException">Throws if queue is already being checked</exception>
        public void RegisterQueue(string queueName)
        {
            if (!queues.TryAdd(queueName, (0, false)))
            {
                throw new ArgumentException("Queue already in collection");
            }
        }

        /// <summary>
        /// Removes queue from checks
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <exception cref="ArgumentException">Throws if queue is not being checked</exception>
        public void RemoveQueue(string queueName)
        {
            if (!queues.TryRemove(queueName, out var _))
            {
                throw new ArgumentException("Queue is not in collection");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                checkTask.Dispose();
            }
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}