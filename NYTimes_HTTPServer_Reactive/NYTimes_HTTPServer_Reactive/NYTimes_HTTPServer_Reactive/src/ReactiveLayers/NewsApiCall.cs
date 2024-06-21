using System.Net;
using NYTimes_HTTPServer_Reactive.Api;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class NewsApiCall : IObserver<(HttpListenerContext, string)>, IObservable<(HttpListenerContext, List<string>)>
{
    private readonly List<IObserver<(HttpListenerContext, List<string>)>> _observers = [];

    public IDisposable Subscribe(IObserver<(HttpListenerContext, List<string>)> observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<(HttpListenerContext, List<string>)>(_observers, observer);
        // return Disposable.Create(() => _observers.Remove(observer));
    }

    private void NotifyObservers(HttpListenerContext context, List<string> contentList)
    {
        foreach (var observer in _observers)
            observer.OnNext((context, contentList));            
    }

    public async void OnNext((HttpListenerContext, string) value)
    {
        var context = value.Item1;
        var keyword = value.Item2;

        var apiService = new NewsApiClientService(5);
        var contentList = await apiService.GetEverythingAsync(keyword);
        
        NotifyObservers(context, contentList);
    }

    public void OnError(Exception e)
    {
        Console.WriteLine($"Unexpected error: {e.Message}.");
    }

    public void OnCompleted()
    {
        Console.WriteLine("Observation completed for NewApiCall layer.");
    }
}