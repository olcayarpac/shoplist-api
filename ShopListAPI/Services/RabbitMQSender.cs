using System.Text;
using RabbitMQ.Client;

namespace ShopListAPI.Services;

public class RabbitMQSender
{
    private readonly ConnectionFactory factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQSender()
    {
        factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void SendMessage(string message)
    {
        _channel.QueueDeclare(queue: "hello",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: string.Empty,
                            routingKey: "hello",
                            basicProperties: null,
                            body: body);
        Console.WriteLine($" [x] Sent {message}");
    }
}