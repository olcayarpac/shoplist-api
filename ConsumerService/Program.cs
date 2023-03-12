using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MongoDB.Driver;
using Newtonsoft.Json;


var mongoClient = new MongoClient("mongodb://localhost:27017");
var mongoDatabase = mongoClient.GetDatabase("shopListDB");
var _APILogsCollection = mongoDatabase.GetCollection<APILog>("apiLogs");

//Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672
};
//Create the RabbitMQ connection using connection factory details as i mentioned above
var connection = factory.CreateConnection();
//Here we create channel with session and model
using var channel = connection.CreateModel();
//declare the queue after mentioning name and a few property related to that
channel.QueueDeclare("apiLogs", exclusive: false, durable: false, autoDelete: false, arguments: null);
//Set Event object which listen message from chanel which is sent by producer
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, eventArgs) =>
{
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Product message received: {message}");
    _APILogsCollection.InsertOne(JsonConvert.DeserializeObject<APILog>(message));
};
//read the message
channel.BasicConsume(queue: "apiLogs", autoAck: true, consumer: consumer);
Console.ReadKey();



public class APILog
{
    public string RequestTime { get; set; } = null!;
    public string ResponseDuration { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string ResponseStatus { get; set; } = null!;
    public string RequestMethod { get; set; } = null!;
}