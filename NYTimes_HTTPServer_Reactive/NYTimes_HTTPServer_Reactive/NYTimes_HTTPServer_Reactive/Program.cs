using System.Net;
using System.Reactive;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

var server = new HttpServer();
server.Start();
Console.WriteLine("Press Enter to stop the server...");

var newsApiCall = new NewsApiCall();
server.Subscribe(newsApiCall);

var observer = Observer.Create<(HttpListenerContext, List<string>)>(
    onNext: (value) => Print(value.Item2)
);

newsApiCall.Subscribe(observer);

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();

static void Print(List<string> list)
{
    foreach (var el in list)
    {
        Console.WriteLine(el);
        Console.WriteLine();
    }
}