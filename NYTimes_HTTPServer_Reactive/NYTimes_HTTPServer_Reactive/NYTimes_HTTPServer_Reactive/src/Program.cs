using System.Net;
using System.Reactive;
using NYTimes_HTTPServer_Reactive.Observers;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

var model = new ModelTraining();
var server = new HttpServer();
var newsApiCall = new NewsApiCall();
var analysis = new SentimentAnalysis();

server.Start();
Console.WriteLine("Press Enter to stop the server...");

server.Subscribe(newsApiCall);

newsApiCall.Subscribe(analysis);
model.Subscribe(analysis);

model.StartTraining();

var observer = new HttpResponseObserver();
analysis.Subscribe(observer);

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();

return;