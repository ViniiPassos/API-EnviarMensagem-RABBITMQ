using Mensageria2.InputModels;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mensageria2.Controllers
{
    [Route("api/messages")]
    public class MensagesController : ControllerBase
    {
        private const string QUEUE_NAME = "messages";
        private readonly ConnectionFactory _factory;
        public MensagesController()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
            };
        }

        [HttpPost]
        public IActionResult SendMessaage([FromBody] SendMessageInputModel sendMessageInputModel)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //Declarar a fila para que caso ela nao exista ainda, eu crio ela
                    channel.QueueDeclare(
                        queue: QUEUE_NAME,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                        );

                    // Formatar os dados para envio para a fila
                    var stringMessage = JsonSerializer.Serialize(sendMessageInputModel);
                    var byteArray = Encoding.UTF8.GetBytes(stringMessage);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: QUEUE_NAME,
                        basicProperties: null,
                        body: byteArray
                        );
                }
            }

            return Accepted();
        }
    }
}
