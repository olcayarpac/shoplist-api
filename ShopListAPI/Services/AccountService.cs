using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopListAPI.Helpers;
using ShopListAPI.Models;

namespace ShopListAPI.Services;

public class AccountService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IConfiguration _configuration;

    public AccountService(IOptions<DatabaseSettings> storeDatabaseSettings, IConfiguration configuration)
    {
        var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.UsersCollectionName);
        _configuration = configuration;
    }

    public async Task<List<User>> GetAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(User newUser)
    {
        var existingUser = await _usersCollection.Find(u => u.Email == newUser.Email || u.Username == newUser.Username).FirstOrDefaultAsync();
        if (existingUser is not null)
            throw new InvalidOperationException("User already exists");
        await _usersCollection.InsertOneAsync(newUser);
    }

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(u => u.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(u => u.Id == id);

    public async Task<User> CheckCredentials(User user)
    {
        var existingUser = await _usersCollection.Find(u => u.Username == user.Username && u.Password == user.Password).FirstOrDefaultAsync();
        if (existingUser is null)
        {
            throw new InvalidCredentialException("Invalid username or password");
        }

        return existingUser;
    }
    public async Task<Token> CreateTokenAsync(string userId)
    {
        TokenHelper tokenHelper = new(_configuration);
        var token = tokenHelper.CreateAccessToken();
        await UpdateUserRefreshTokenAsync(userId, token.RefreshToken, token.ExpireDate.AddHours(12));

        return token;
    }

    public async Task UpdateUserRefreshTokenAsync(string userId, string refreshToken, DateTime expireDate)
    {
        var filter = Builders<User>.Filter.Eq("Id", userId);
        var update = Builders<User>.Update.Set("RefreshToken", refreshToken).Set("RefreshTokenExpireDate", expireDate);
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task<Token> RefreshTokenAsync(string refreshToken)
    {
        var existingUser = await _usersCollection.Find(u => u.RefreshToken == refreshToken && u.RefreshTokenExpireDate > DateTime.Now).FirstOrDefaultAsync();
        if (existingUser is null)
        {
            throw new InvalidCredentialException("Invalid refresh token");
        }
        TokenHelper tokenHelper = new(_configuration);
        var token = tokenHelper.CreateAccessToken();
        await UpdateUserRefreshTokenAsync(existingUser.Id, token.RefreshToken, token.ExpireDate.AddHours(12));
        return token;
    }
}