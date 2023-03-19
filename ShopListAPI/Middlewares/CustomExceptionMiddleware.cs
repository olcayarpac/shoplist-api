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
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
            APILog apiLog = new()
            {
                Endpoint = context.Request.Path,
                RequestTime = requestTime.ToString(),
                ResponseDuration = responseWatcher.Elapsed.TotalMilliseconds.ToString(),
                ResponseStatus = context.Response.StatusCode.ToString(),
                RequestMethod = context.Request.Method
            };
            _rabbitMQPublisher.PublishAPILog("apiLogs", apiLog);
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