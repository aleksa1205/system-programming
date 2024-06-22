using System.Net;
using System.Reactive.Concurrency;
using Microsoft.ML;
using NYTimes_HTTPServer_Reactive.AiModel;
using NYTimes_HTTPServer_Reactive.ReactiveModels;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class SentimentAnalysis : IObservable<(HttpListenerContext, List<SentimentPrediction>)>, IObserver<(HttpListenerContext, List<string>)>
{
    private readonly List<IObserver<(HttpListenerContext, List<SentimentPrediction>)>> _observers = [];
    private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;

    public IDisposable Subscribe(IObserver<(HttpListenerContext, List<SentimentPrediction>)> observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<(HttpListenerContext, List<SentimentPrediction>)>(_observers, observer);
    }

    public void OnNext((HttpListenerContext, List<string>) value)
    {
        Console.WriteLine($"Sentiment analysis received value on thread: {Environment.CurrentManagedThreadId}");
        
        var context = value.Item1;
        var contentList = value.Item2;

        try
        {
            Console.WriteLine($"Sentiment analysis doing work on thread: {Environment.CurrentManagedThreadId}");
            
            List<SentimentPrediction> result = [];
            if (_predictionEngine is null)
                LoadModel();

            foreach (var contentElement in contentList)
            {
                var input = new SentimentData
                {
                    SentimentText = contentElement
                };

                var prediction = _predictionEngine!.Predict(input);
                result.Add(prediction);
            }

            ObserverUtility<(HttpListenerContext, List<SentimentPrediction>)>.NotifyOnNext(_observers,
                (context, result));
        }
        catch (Exception e)
        {
            HttpServer.SendResponse(context, "Prediction error"u8.ToArray(), "text/plain",
                HttpStatusCode.InternalServerError);
            ObserverUtility<(HttpListenerContext, List<SentimentPrediction>)>.NotifyOnError(_observers, e);
        }
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"NewsApiCall layer returned an error:\n{error.Message}");
        ObserverUtility<(HttpListenerContext, List<SentimentPrediction>)>.NotifyOnError(_observers, error);
    }

    public void OnCompleted()
    {
        Console.WriteLine("Observation completed for SentimentAnalysis layer.");
        ObserverUtility<(HttpListenerContext, List<SentimentPrediction>)>.NotifyOnCompleted(_observers);
    }

    private void LoadModel()
    {
        var modelPath = Utils.GetModelPath();
        
        var context = new MLContext(seed: 0);
        var model = context.Model.Load(modelPath, out var modelInputSchema);

        _predictionEngine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }
}