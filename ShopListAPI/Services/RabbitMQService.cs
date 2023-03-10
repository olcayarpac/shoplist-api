using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ShopListAPI.Models;

namespace ShopListAPI.Services;

public class RabbitMQService
{
    private readonly string _hostName = "localhost";
    private readonly int _port = 5672;

    public IConnection? GetRabbitMQConnection()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = _hostName,
            Port = _port
        };

        try
        {
            return connectionFactory.CreateConnection();
        }
        catch
        {
            Console.WriteLine("Error occured while connecting RabbitMq");
        }
        return null;
    }
}

public class RabbitMQPublisher
{
    private readonly RabbitMQService _rabbitMQService;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher()
    {
        _rabbitMQService = new RabbitMQService();
        _connection = _rabbitMQService.GetRabbitMQConnection();
        if (_connection != null)
            _channel = _connection.CreateModel();
    }

    public void Publish(string queueName, APILog apiLog)
    {
        try
        {

            _channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(apiLog));

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: null,
                body: body);
        }
        catch
        {
            Console.WriteLine("Cannot publish to rabbitmq!");
        }
    }
}