using NYTimes_HTTPServer_Reactive;

var server = new HttpServer();
server.Start();
Console.WriteLine("Press Enter to stop the server...");

// var newsApi = new NewsApiClientService(5);
// var result = await newsApi.GetEverything("bitcoin");
// foreach (var element in result)
// {
//     Console.WriteLine(element + "\n");
// }

while (Console.ReadKey().Key != ConsoleKey.Enter) { }
server.Stop();