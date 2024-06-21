using System.Net;
using System.Text;
using NYTimes_HTTPServer_Reactive.ReactiveLayers;

namespace NYTimes_HTTPServer_Reactive.Observers;

public class HttpResponseObserver : IObserver<(HttpListenerContext, List<AnalyzedContent>)>
{
    public void OnNext((HttpListenerContext, List<AnalyzedContent>) value)
    {
        var context = value.Item1;
        var list = value.Item2;

        var data = list.Select(ac => $"Content: {ac.ContentText}\nSentiment: {ac.Sentiment}\n");
        
        var dataAsString = string.Join(Environment.NewLine, data);
        var dataAsBytes = Encoding.UTF8.GetBytes(dataAsString);
        HttpServer.SendResponse(context, dataAsBytes, "text/plain");
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }
}