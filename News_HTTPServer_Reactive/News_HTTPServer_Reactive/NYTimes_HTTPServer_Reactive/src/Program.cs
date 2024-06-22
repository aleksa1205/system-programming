using NYTimes_HTTPServer_Reactive.Observers;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

var model = new ModelTraining();
var server = new HttpServer();
var newsApiCall = new NewsApiCall();
var analysis = new SentimentAnalysis();

var serverSubscription = model.Subscribe(server);
var newsApiSubscription = server.Subscribe(newsApiCall);
var analysisSubscription = newsApiCall.Subscribe(analysis);

var observer = new HttpResponseObserver();
var observerSubscription = analysis.Subscribe(observer);

model.StartTraining();

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();

serverSubscription.Dispose();
newsApiSubscription.Dispose();
analysisSubscription.Dispose();
observerSubscription.Dispose();

return;