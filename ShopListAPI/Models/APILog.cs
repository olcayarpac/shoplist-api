namespace ShopListAPI.Models;

public class APILog
{
    public string RequestTime { get; set; } = null!;
    public string ResponseDuration { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string ResponseStatus { get; set; } = null!;
    public string RequestMethod { get; set; } = null!;
}