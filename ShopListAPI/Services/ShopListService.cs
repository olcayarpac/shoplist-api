using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopListAPI.Helpers;
using ShopListAPI.Models;

namespace ShopListAPI.Services;

public class ShopListService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<ShopList> _shopListsCollection;
    private readonly IConfiguration _configuration;

    public ShopListService(IOptions<DatabaseSettings> storeDatabaseSettings, IConfiguration configuration)
    {
        var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.UsersCollectionName);
        _shopListsCollection = mongoDatabase.GetCollection<ShopList>(storeDatabaseSettings.Value.ListsCollectionName);
        _configuration = configuration;
    }

    public async Task<List<string>> GetShopListsByUserId(string userId)
    {
        var user =  await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if(user is null){
            throw new MongoInternalException("User not found");
        }
        else if(user.ShopListIds is null){
            return new List<string>();
        }
        return user.ShopListIds;
    }

    public async Task CreateShopList(ShopList newShopList)
    {
        var user = await _usersCollection.Find(u => u.Id == newShopList.OwnerId).FirstOrDefaultAsync();
        if (user is null){
            throw new NullReferenceException("User id is not valid");
        }
        await _shopListsCollection.InsertOneAsync(newShopList);
        if(newShopList.Id is null){
            throw new MongoInternalException("Error while creating list");
        }
        user.ShopListIds.Add(newShopList.Id);
        await _usersCollection.UpdateOneAsync(Builders<User>.Filter.Eq("Id", user.Id),Builders<User>.Update.Push("ShopListIds",newShopList.Id));
    }

}