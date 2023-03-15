using System.Collections.Immutable;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

    public async Task<List<ShopList>> GetShopListsByUserId(string userId)
    {
        var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null)
        {
            throw new MongoInternalException("User not found");
        }
        else if (user.ShopListIds is null)
        {
            return new List<ShopList>();
        }
        var filter = Builders<ShopList>.Filter.In("Id", user.ShopListIds);
        return await _shopListsCollection.Find(filter).ToListAsync();
    }

    public async Task CreateShopList(ShopList newShopList)
    {
        var user = await _usersCollection.Find(u => u.Id == newShopList.OwnerId).FirstOrDefaultAsync();
        if (user is null)
        {
            throw new MongoInternalException("User id is not valid");
        }
        foreach (var shopListItem in newShopList.ListItems)
        {
            shopListItem.Id = ObjectId.GenerateNewId().ToString();
        }
        await _shopListsCollection.InsertOneAsync(newShopList);
        if (newShopList.Id is null)
        {
            throw new MongoInternalException("Error while creating list");
        }
        user.ShopListIds.Add(newShopList.Id);
        await _usersCollection.UpdateOneAsync(Builders<User>.Filter.Eq("Id", user.Id), Builders<User>.Update.Push("ShopListIds", newShopList.Id));
    }

    public async Task<ShopList> GetListById(string listId)
    {
        var shopList = await _shopListsCollection.Find(list => list.Id == listId).FirstOrDefaultAsync();
        if (shopList is null)
        {
            throw new MongoInternalException("List id is not valid");
        }
        return shopList;

    }

    public async Task InsertListItem(string listId, ListItem newItem)
    {
        newItem.Id = ObjectId.GenerateNewId().ToString();
        await _shopListsCollection.UpdateOneAsync(Builders<ShopList>.Filter.Eq("Id", listId), Builders<ShopList>.Update.Push("ListItems", newItem));
    }

    public async Task DeleteListItem(string listId, string itemId)
    {
        var filter = Builders<ShopList>.Filter.Where(list => list.Id == listId);
        var update = Builders<ShopList>.Update.PullFilter(list => list.ListItems, Builders<ListItem>.Filter.Where(item => item.Id == itemId));
        await _shopListsCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteShopList(string listId)
    {
        await _shopListsCollection.DeleteOneAsync(Builders<ShopList>.Filter.Eq("Id", listId));
    }

    public async Task<ShopList> UpdateItemDoneStatus(string listId, string itemId, bool isDone)
    {
        var filter = Builders<ShopList>.Filter.Where(list => list.Id == listId && list.ListItems.Any(i => i.Id == itemId));
        var update = Builders<ShopList>.Update.Set("ListItems.$.IsDone", isDone);
        var result = await _shopListsCollection.UpdateOneAsync(filter, update);
        return await CheckAndUpdateListDoneStatus(listId);
    }

    public async Task<ShopList> CheckAndUpdateListDoneStatus(string listId)
    {
        bool isDone = true;
        var resultList = await GetListById(listId);
        foreach (var listItem in resultList.ListItems)
        {
            if (!listItem.IsDone) isDone = false;
        }
        var filter = Builders<ShopList>.Filter.Where(list => list.Id == listId);
        var update = Builders<ShopList>.Update.Set("IsDone", isDone);
        return await _shopListsCollection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<bool> CheckUserHasList(string userId, string listId)
    {
        var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null)
        {
            throw new Exception("User not found");
        }
        else if (user.ShopListIds.Contains(listId))
        {
            return true;
        }

        return false;
    }
}