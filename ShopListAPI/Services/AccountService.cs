using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopListAPI.Models;

namespace ShopListAPI.Services;

public class AccountService
{
    private readonly IMongoCollection<User> _usersCollection;

    public AccountService(IOptions<DatabaseSettings> storeDatabaseSettings)
    {
        var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(User newUser){
        var existingUser = await _usersCollection.Find(p => p.Email == newUser.Email || p.Username == newUser.Username).FirstOrDefaultAsync();
        await _usersCollection.InsertOneAsync(newUser);
    }

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(p => p.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(p => p.Id == id);
}