using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShopListAPI.Models;

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