using System.Net;

namespace NYTimes_HTTPServer_Reactive.Observers;

public class ConsoleObserver : IObserver<(HttpListenerContext, string)>
{
    public void OnNext((HttpListenerContext, string) value)
    {
        Console.WriteLine($"Received value: {value}.");
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"An error occurred: {error.Message}.");
    }

    public void OnCompleted()
    {
        Console.WriteLine("Observation completed.");
    }
}