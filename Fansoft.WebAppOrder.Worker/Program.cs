using System.Text;
using System.Text.Json;
using Fansoft.WebAppOrder.Worker.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using(var connection = factory.CreateConnection())
using(var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "orderQueue",
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {

        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<Order>(message);
            channel.BasicAck(ea.DeliveryTag,false);

            Console.WriteLine($" [x] OrderNumber {order.OrderNumber} | {order.ItemName} | {order.Price:N2}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            channel.BasicNack(ea.DeliveryTag,false, true);
            throw;
        }
        
    };
    channel.BasicConsume(queue: "orderQueue",
        autoAck: false, // por padrão é tru porém para não dar um ok em caso de erro, alterei para false e
                        // adicionei try catch no código
        consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}