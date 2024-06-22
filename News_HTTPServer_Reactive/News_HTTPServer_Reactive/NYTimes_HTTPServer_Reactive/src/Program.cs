using System.Net;
using System.Reactive;
using NYTimes_HTTPServer_Reactive.AiModel;
using NYTimes_HTTPServer_Reactive.Observers;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

var model = new ModelTraining();
var server = new HttpServer();
var newsApiCall = new NewsApiCall();
var analysis = new SentimentAnalysis();

var serverSubscription = model.Subscribe(server);
var newsApiSubscription = server.Subscribe(newsApiCall);
var analysisSubscription = newsApiCall.Subscribe(analysis);

var httpResponseObserver = new HttpResponseObserver();

var consoleObserver1 = Observer.Create<(HttpListenerContext, List<SentimentPrediction>)>(
    onNext: (value) => Console.WriteLine($"Observer 1: {value.Item2[0].Prediction}")
);
var consoleObserver2 = Observer.Create<(HttpListenerContext, List<SentimentPrediction>)>(
    onNext: (value) => Console.WriteLine($"Observer 2: {value.Item2[2].Prediction}")
);

var observerSubscription = analysis.Subscribe(httpResponseObserver);
var observer2Subscription = analysis.Subscribe(consoleObserver1);
var observer3Subscription = analysis.Subscribe(consoleObserver2);

model.StartTraining();

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();

serverSubscription.Dispose();
newsApiSubscription.Dispose();
analysisSubscription.Dispose();
observerSubscription.Dispose();
observer2Subscription.Dispose();
observer3Subscription.Dispose();