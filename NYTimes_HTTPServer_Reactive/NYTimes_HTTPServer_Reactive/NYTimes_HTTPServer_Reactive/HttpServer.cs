using System.Net;
using System.Web;

namespace NYTimes_HTTPServer_Reactive;

public class HttpServer
{
    private readonly HttpListener _httpListener;
    private bool _running;

    public HttpServer(string address="localhost", int port = 5050)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://{address}:{port}/");
        _running = false;
    }

    private static void SendResponse(HttpListenerContext context, byte[] responseBody, string contentType,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var logString =
            $"REQUEST:\n{context.Request.HttpMethod} {context.Request.RawUrl} HTTP/{context.Request.ProtocolVersion}\n" +
            $"Host: {context.Request.UserHostName}\nUser-agent: {context.Request.UserAgent}\n-------------------\n" +
            $"RESPONSE:\nStatus: {statusCode}\nDate: {DateTime.Now}\nContent-Type: {contentType}" +
            $"\nContent-Length: {responseBody.Length}\n";

        context.Response.ContentType = contentType;
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentLength64 = responseBody.Length;
        using (var outputStream = context.Response.OutputStream)
        {
            outputStream.Write(responseBody, 0, responseBody.Length);
        }

        Console.WriteLine(logString);
    }

    private async Task AcceptConnection(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.HttpMethod != "GET")
        {
            Console.WriteLine("Method not allowed!");
            SendResponse(context, "Method not allowed!"u8.ToArray(), "text/plain", HttpStatusCode.MethodNotAllowed);
            return;
        }


        try
        {
            var rawUrl = request.RawUrl;
            
            if (string.IsNullOrEmpty(request.RawUrl))
            {
                SendResponse(context, "No keyword given!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
                return;
            }
            
            var apiService = new NewsApiClientService(5);
            
            var paramsCollection = HttpUtility.ParseQueryString(rawUrl!);

            var keyword = paramsCollection.Get(0);
            Console.WriteLine(keyword);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("API returned an error!");
            SendResponse(context, "API returned an error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
        }
        catch (Exception)
        {
            Console.WriteLine("Unknown error!");
            SendResponse(context, "Unknown error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
        }
    }

    public void Start()
    {
        _httpListener.Start();
        _running = true;
        Listen();
        Console.WriteLine("Server started!");
    }

    public void Stop()
    {
        _httpListener.Stop();
        _running = false;
        Console.WriteLine("Server stopped!");
    }

    private async void Listen()
    {
        while (_running)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();

                if (_running)
                {
                    await Task.Run(() => AcceptConnection(context));
                }
            }
            catch (HttpListenerException e)
            {
                Console.WriteLine("Server stopped listening! Message: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected server error: " + e.Message);
            }
        }
    }
}
