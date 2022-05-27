using System.Text;
using System.Text.Json;
using Fansoft.WebAppOrder.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Fansoft.WebAppOrder.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    
    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "InsertOrder")]
    public IActionResult InsertOrder(Order order)
    {
        try
        {
            #region InserirNaFila

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orderQueue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                string message = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: "orderQueue",
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            #endregion
            return Accepted(order);

        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao tentar criar um pedido", e);
            return new StatusCodeResult(500);
        }
    }
}