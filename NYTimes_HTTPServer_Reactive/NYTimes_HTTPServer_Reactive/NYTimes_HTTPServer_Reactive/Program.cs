using NYTimes_HTTPServer_Reactive.Observers;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

var server = new HttpServer();
server.Start();
Console.WriteLine("Press Enter to stop the server...");

var observer = new ConsoleObserver();
var subscription = server.Subscribe(observer);

Thread.Sleep(10000);
Console.WriteLine("Unsubscribing...");
subscription.Dispose();

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();