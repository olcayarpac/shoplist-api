using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShopListAPI.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    [BsonElement("Username")]
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public List<string> ShopListIds { get; set; } = new List<string>();
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpireDate { get; set; }
    public string Role { get; set; } = "User";

}