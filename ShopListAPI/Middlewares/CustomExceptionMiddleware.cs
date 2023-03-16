using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using ShopListAPI.Models;
using MongoDB.Driver;
using ShopListAPI.Services;

namespace StoreAPI.Middlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMongoCollection<APILog> _APILogsCollection;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public CustomExceptionMiddleware(RequestDelegate next, IOptions<DatabaseSettings> storeDatabaseSettings)
        {
            _next = next;
            var mongoClient = new MongoClient(storeDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(storeDatabaseSettings.Value.DatabaseName);
            _APILogsCollection = mongoDatabase.GetCollection<APILog>(storeDatabaseSettings.Value.APILogsCollectionName);
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
                await HandleException(context, ex, watch);
            }
            finally
            {
                WriteLogToDB(context, requestTime, watch);
            }
        }

        private static Task HandleException(HttpContext context, Exception ex, Stopwatch watch)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = "[Error] HTTP " + context.Request.Method + " - " + context.Request.Path + " Error message "
                    + ex.Message + " in " + watch.Elapsed.TotalMilliseconds + "ms";
            Console.WriteLine(message);
            var result = JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.None);
            return context.Response.WriteAsync(result);
        }

        private void WriteLogToDB(HttpContext context, DateTime requestTime, Stopwatch responseWatcher)
        {
            APILog apiLog = new APILog();
            apiLog.Endpoint = context.Request.Path;
            apiLog.RequestTime = requestTime.ToString();
            apiLog.ResponseDuration = responseWatcher.Elapsed.TotalMilliseconds.ToString();
            apiLog.ResponseStatus = context.Response.StatusCode.ToString();
            apiLog.RequestMethod = context.Request.Method;
            _APILogsCollection.InsertOneAsync(apiLog);
            _rabbitMQPublisher.Publish("apiLogs", apiLog);
        }
    }

    public static class CustomExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionMiddleware>();
        }
    }
}