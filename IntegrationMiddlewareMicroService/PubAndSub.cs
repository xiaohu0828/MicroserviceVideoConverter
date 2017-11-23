using System;
using MicroServiceCommon;
using MicroServiceCommon.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IntegrationMiddlewareMicroService
{
    public class PubAndSub
    {
        private const string QueueName = "TranscodingWorker_Queue";

        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        public PubAndSub()
        {
            CreateConnection();
        }

        public void SendMessage(VideoFile file)
        {
            _model.BasicPublish("", QueueName, null, file.Serialize());
            Console.WriteLine("Send message to Queue with video file id {0}, name {1}", file.Id, file.Name);
        }

        private static void CreateConnection()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.QueueDeclare(QueueName, true, false, false, null);
            _model.BasicAcks += ModelOnBasicAcks;
            _model.BasicReturn += ModelOnBasicReturn;
        }

        private static void ModelOnBasicReturn(object sender, BasicReturnEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void ModelOnBasicAcks(object sender, BasicAckEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
