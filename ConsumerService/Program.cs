using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var mongoClient = new MongoClient("mongodb://localhost:27017");
var mongoDatabase = mongoClient.GetDatabase("shopListDB");
var _APILogsCollection = mongoDatabase.GetCollection<APILog>("apiLogs");
var _doneListsCollection = mongoDatabase.GetCollection<ShopList>("doneLists");
var _shopListsCollection = mongoDatabase.GetCollection<ShopList>("shopLists");

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
channel.QueueDeclare("apiLogs", durable: false, exclusive: false, autoDelete: false, arguments: null);
//Set Event object which listen message from chanel which is sent by producer
var apiLogsConsumer = new EventingBasicConsumer(channel);
apiLogsConsumer.Received += (model, eventArgs) =>
{
    Console.WriteLine("apilogs");
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var messageJson = JsonConvert.DeserializeObject<APILog>(message);
    if (messageJson is not null)
    {
        _APILogsCollection.InsertOne(messageJson);
    }
};
//read the message
channel.BasicConsume(queue: "apiLogs", autoAck: true, consumer: apiLogsConsumer);

using var channel2 = connection.CreateModel();
//declare the queue after mentioning name and a few property related to that
channel2.QueueDeclare("doneLists", durable: false, exclusive: false, autoDelete: false, arguments: null);
//Set Event object which listen message from chanel which is sent by producer
var doneListsConsumer = new EventingBasicConsumer(channel);
doneListsConsumer.Received += (model, eventArgs) =>
{
    Console.WriteLine("donelists");
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var messageJson = JsonConvert.DeserializeObject<ShopList>(message);
    if (messageJson is not null)
    {
        _doneListsCollection.InsertOne(messageJson);
    }
};
//read the message
channel.BasicConsume(queue: "doneLists", autoAck: true, consumer: doneListsConsumer);

Console.ReadKey();

public class APILog
{
    public string RequestTime { get; set; } = null!;
    public string ResponseDuration { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string ResponseStatus { get; set; } = null!;
    public string RequestMethod { get; set; } = null!;
}

public class ShopList
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? OwnerId { get; set; }
    public string? ListName { get; set; }
    public bool? IsDone { get; set; }
    public string? Description { get; set; }
    public List<ListItem> ListItems { get; set; } = new List<ListItem>();
}

public class ListItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? ItemName { get; set; }
    public double Amount { get; set; } = 0;
    public string? AmountType { get; set; }
    public string? Description { get; set; }
    public bool IsDone { get; set; } = false;
}