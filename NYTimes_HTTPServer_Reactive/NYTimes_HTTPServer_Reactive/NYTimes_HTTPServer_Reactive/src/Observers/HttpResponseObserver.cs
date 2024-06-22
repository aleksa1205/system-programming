using System.Net;
using System.Text;
using System.Text.Json;
using NYTimes_HTTPServer_Reactive.AiModel;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

namespace NYTimes_HTTPServer_Reactive.Observers;

public class AnalyzedData
{
    public required string Content { get; set; }
    public required string Sentiment { get; set; }
}

public class HttpResponseObserver : IObserver<(HttpListenerContext, List<SentimentPrediction>)>
{
    public void OnNext((HttpListenerContext, List<SentimentPrediction>) value)
    {
        var context = value.Item1;
        var list = value.Item2;

        if (list.Count == 0)
        {
            HttpServer.SendResponse(context, "No data found for a given keyword."u8.ToArray(), "text/plain");
            return;
        }

        var outputBytes = SerializeToJson(list);
        
        HttpServer.SendResponse(context, outputBytes, "text/plain");
    }

    public void OnCompleted()
    {
        Console.WriteLine("Observation Completed for HttpResponseObserver.");
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"SentimentAnalysis layer returned an error:\n{error.Message}");
    }

    private static byte[] SerializeToString(List<SentimentPrediction> list)
    {
        var data = list.Select(ac => $"Content: {ac.SentimentText}\nSentiment: {ac.Prediction}\n");
        
        var dataAsString = string.Join(Environment.NewLine, data);
        return Encoding.UTF8.GetBytes(dataAsString);
    }

    private static byte[] SerializeToJson(List<SentimentPrediction> list)
    {
        var newList = list.Select(el => new AnalyzedData
        {
            Content = el.SentimentText!,
            Sentiment = el.Prediction!
        });
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        return JsonSerializer.SerializeToUtf8Bytes(newList, options);
    }
}