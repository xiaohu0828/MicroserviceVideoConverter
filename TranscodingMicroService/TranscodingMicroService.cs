using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroServiceCommon;
using MicroServiceCommon.Models;
using NReco.VideoConverter;
using RabbitMQ.Client;

namespace TranscodingMicroService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TranscodingMicroService : StatelessService
    {
        private const string QueueName = "TranscodingWorker_Queue";
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        public TranscodingMicroService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public static void Receive()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(QueueName, true, false, false, null);
                    channel.BasicQos(0, 1, false);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(QueueName, false, consumer);

                    var ffMeg = new FFMpegConverter();
                    ffMeg.ConvertProgress += (sender, args) =>
                    {
                        Console.WriteLine(args.Processed + "/" + args.TotalDuration);

                    };
                    ffMeg.LogReceived += (sender, args) =>
                    {
                        Console.WriteLine(args.Data);
                    };

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        var message = (VideoFile)ea.Body.DeSerialize(typeof(VideoFile));

                        try
                        {
                            ffMeg.ConvertMedia(message.Path,
                                message.Path.Replace(message.Name, message.Id + "-video.webm"), Format.webm);
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (FFMpegException e)
                        {
                            Console.WriteLine(e);
                            // Requeue the reject message and do it again.
                            channel.BasicReject(ea.DeliveryTag, true);
                        }
                    }
                }
            }
        }
    }
}
