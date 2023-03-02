using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShopListAPI.Models;

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