using System.Net;
using System.Reactive.Concurrency;
using NYTimes_HTTPServer_Reactive.Api;
using NYTimes_HTTPServer_Reactive.ReactiveModels;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class NewsApiCall : IObserver<(HttpListenerContext, string)>, IObservable<(HttpListenerContext, List<string>)>
{
    private readonly List<IObserver<(HttpListenerContext, List<string>)>> _observers = [];
    private readonly IScheduler _scheduler = TaskPoolScheduler.Default;

    public IDisposable Subscribe(IObserver<(HttpListenerContext, List<string>)> observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<(HttpListenerContext, List<string>)>(_observers, observer);
        // return Disposable.Create(() => _observers.Remove(observer));
    }

    public void OnNext((HttpListenerContext, string) value)
    {
        Console.WriteLine($"News api call layer received value on thread: {Environment.CurrentManagedThreadId}");
        var context = value.Item1;
        var keyword = value.Item2;

        _scheduler.Schedule(() =>
        {
            try
            {
                Console.WriteLine($"News api call layer doing work on thread: {Environment.CurrentManagedThreadId}");
                var apiService = new NewsApiClientService();
                var contentList = apiService.GetEverything(keyword);

                ObserverUtility<(HttpListenerContext, List<string>)>.NotifyOnNext(_observers, (context, contentList));
            }
            catch (Exception e)
            {
                HttpServer.SendResponse(context, "API returned an error!"u8.ToArray(), "text/plain",
                    HttpStatusCode.InternalServerError);
                ObserverUtility<(HttpListenerContext, List<string>)>.NotifyOnError(_observers, e);
            }
        });
    }

    public void OnError(Exception e)
    {
        if (e is HttpRequestException requestException)
            Console.WriteLine($"Http request error:\n{requestException.Message}");
        else
            Console.WriteLine($"HttpServer layer returned an error:\n{e.Message}.");
        
        ObserverUtility<(HttpListenerContext, List<string>)>.NotifyOnError(_observers, e);
    }

    public void OnCompleted()
    {
        Console.WriteLine("Observation completed for NewApiCall layer.");
        ObserverUtility<(HttpListenerContext, List<string>)>.NotifyOnCompleted(_observers);
    }
}