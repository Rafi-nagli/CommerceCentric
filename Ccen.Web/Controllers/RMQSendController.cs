using Newtonsoft.Json;
using RabbitMQ.Client;
using RMQModels;
using System;
using System.Text;

namespace Amazon.Web.Controllers
{
    public class RMQSendController
    {
        public RMQSendController(string text = null)
        {
            var rabbitConfig = new RabbitConfig();
            var connectionString = $"amqp://{rabbitConfig.RabbitUserName}:{rabbitConfig.RabbitPassword}@{rabbitConfig.RabbitHost}:{rabbitConfig.RabbitPort}/";
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString)
            };

            var exchange = "OrderSearchService";
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Create Exchange
                channel.ExchangeDeclare(exchange, ExchangeType.Direct);

                //Create Queue
                channel.QueueDeclare(nameof(RMQUpdateOrderModel), true, false, false, null);

                //Bind Queue to Exchange
                channel.QueueBind(nameof(RMQUpdateOrderModel), exchange, nameof(RMQUpdateOrderModel));
                //while (true)
                {
                    //var text = Console.ReadLine();
                    text = text ?? "send from ccen-web";
                    channel.BasicPublish(exchange, nameof(RMQUpdateOrderModel), null,
                        GetByteFromClass(new RMQUpdateOrderModel() { Id = 0, Name = text }));
                }
            }
        }


        private static byte[] GetByteFromClass(object obj)
        {
            var o = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(o);
        }
    }

    public class RabbitConfig
    {
        public string RabbitUserName { get; set; }
        public int RabbitPort { get; set; }
        public string RabbitHost { get; set; }
        public string RabbitPassword { get; set; }

        public RabbitConfig()
        {
            RabbitUserName = "rabbitmq";
            RabbitPassword = "8GAnxvz95Dkx7Mac";
            RabbitPort = 5672;
            RabbitHost = "154.27.80.183";
        }
    }
}

namespace RMQModels
{
    public class RMQUpdateOrderModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}