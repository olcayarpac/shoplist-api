using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopListAPI.Helpers;
using ShopListAPI.Models;

namespace ShopListAPI.Services;

public class ShopListService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<User> _shopListsCollection;
    private readonly IConfiguration _configuration;

    public ShopListService(IOptions<DatabaseSettings> storeDatabaseSettings, IConfiguration configuration)
    {
        var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.UsersCollectionName);
        _shopListsCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.ListsCollectionName);
        _configuration = configuration;
    }
}