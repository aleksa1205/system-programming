using System.Net;
using System.Reactive.Disposables;
using System.Web;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class HttpServer : IObservable<(HttpListenerContext, string)>
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

            NotifyObservers(context, keyword);

            // var apiService = new NewsApiClientService(5);
            // var data = await apiService.GetEverything(keyword);
            // if (data.Count == 0)
            // {
            //     SendResponse(context, "No content for given keyword!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
            //     return;
            // }
            //
            // var dataAsString = string.Join(Environment.NewLine, data);
            // var dataAsBytes = Encoding.UTF8.GetBytes(dataAsString);
            // SendResponse(context, dataAsBytes, "text/plain");
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

    private void NotifyObservers(HttpListenerContext context, string keyword)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext((context, keyword));
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
            }
        }
    }
}
