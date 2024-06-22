using System.Net;
using System.Web;
using NYTimes_HTTPServer_Reactive.ReactiveModels;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class HttpServer : IObservable<(HttpListenerContext, string)>, IObserver<bool>
{
    private readonly HttpListener _httpListener;
    private bool _running;
    private readonly List<IObserver<(HttpListenerContext, string)>> _observers;

    public HttpServer(string address="localhost", int port = 5050)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://{address}:{port}/");
        _running = false;
        _observers = [];
    }

    public IDisposable Subscribe(IObserver<(HttpListenerContext, string)> observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<(HttpListenerContext, string)>(_observers, observer);
        // Drugi (bolji) nacin za vracanje IDisposable instance.
        // Ovo je factory koji kreira instancu IDisposable interfejsa bez potrebe pisanja Unsubscriber klase.
        // return Disposable.Create(() => _observers.Remove(observer));
    }

    public void OnNext(bool value)
    {
        Console.WriteLine($"Http server received value on thread: {Environment.CurrentManagedThreadId}");
        if (value) Start();
        else Stop();
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"Error when training a model:\n{error.Message}");
        ObserverUtility<(HttpListenerContext, string)>.NotifyOnError(_observers, error);
        Stop(isError: true);
    }

    public void OnCompleted()
    {
        Console.WriteLine("Model finished training\nPress Enter to stop the server...");
    }

    public static void SendResponse(HttpListenerContext context, byte[] responseBody, string contentType,
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

    private void AcceptConnection(HttpListenerContext context)
    {
        Console.WriteLine($"Http server doing work on thread: {Environment.CurrentManagedThreadId}");
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
            
            if (string.IsNullOrEmpty(rawUrl))
            {
                SendResponse(context, "No keyword given!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
                return;
            }
            
            var paramsCollection = HttpUtility.ParseQueryString(rawUrl);

            var keyword = paramsCollection.Get(0);
            if (keyword is null)
            {
                SendResponse(context, "Keyword is null!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
                return;
            }

            if (keyword.Length < 3)
            {
                SendResponse(context, "Keyword must be at least 3 characters!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
                return;
            }

            ObserverUtility<(HttpListenerContext, string)>.NotifyOnNext(_observers, (context, keyword));
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("API returned an error!");
            SendResponse(context, "Http request error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
        }
        catch (Exception error)
        {
            SendResponse(context, "Unknown server error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
            ObserverUtility<(HttpListenerContext, string)>.NotifyOnError(_observers, error);
        }
    }
    
    private void Start()
    {
        _httpListener.Start();
        _running = true;
        Listen();
        Console.WriteLine("Server started!");
    }

    public void Stop(bool isError = false)
    {
        _httpListener.Stop();
        _running = false;
        Console.WriteLine("Server stopped!");
        
        if (!isError)
            ObserverUtility<(HttpListenerContext, string)>.NotifyOnCompleted(_observers);
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
                    AcceptConnection(context);
                }
            }
            catch (HttpListenerException e)
            {
                Console.WriteLine("Server stopped listening! Message: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected server error: " + e.Message);
                ObserverUtility<(HttpListenerContext, string)>.NotifyOnError(_observers, e);
            }
        }
    }
}
