using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using ShopListAPI.Models;
using MongoDB.Driver;
using ShopListAPI.Services;

namespace StoreAPI.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public AuthorizationMiddleware(RequestDelegate next, IOptions<DatabaseSettings> storeDatabaseSettings)
        {
            _next = next;
            var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(storeDatabaseSettings.Value.UsersCollectionName);
            _rabbitMQPublisher = new RabbitMQPublisher();
        }

        public async Task Invoke(HttpContext context)
        {
            var watch = Stopwatch.StartNew();
            var requestTime = DateTime.Now;
            try
            {
                string message = "[Request]  HTTP " + context.Request.Method + " - " + context.Request.Path;
                Console.WriteLine(message);
                await _next(context);
                watch.Stop();
                message = "[Response] HTTP " + context.Request.Method + " - " + context.Request.Path + " responded "
                    + context.Response.StatusCode + " in " + watch.Elapsed.TotalMilliseconds + "ms";
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                watch.Stop();
            }
            finally
            {
            }
        }
    }

    public static class AuthorizationMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}