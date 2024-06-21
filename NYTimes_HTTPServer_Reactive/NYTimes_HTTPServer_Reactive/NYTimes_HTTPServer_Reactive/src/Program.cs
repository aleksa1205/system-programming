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

var model = new ModelTraining();
var observer2 = Observer.Create<bool>(
    onNext: (value) => Console.WriteLine(value),
    onCompleted: () => Console.WriteLine("Model training finished")
);

model.Subscribe(observer2);
model.StartTraining();

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();

return;

static void Print(List<string> list)
{
    foreach (var el in list)
    {
        Console.WriteLine(el);
        Console.WriteLine();
    }
}