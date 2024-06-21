using System.Net;
using Microsoft.ML;
using NYTimes_HTTPServer_Reactive.AiModel;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class AnalyzedContent
{
    public required string ContentText { get; set; }
    public required string Sentiment { get; set; }
}

public class ModelNotTrainedException : Exception
{
    public ModelNotTrainedException() { }
    public ModelNotTrainedException(string msg) : base(msg) { }
}

public class SentimentAnalysis : 
    IObservable<(HttpListenerContext, List<AnalyzedContent>)>, 
    IObserver<(HttpListenerContext, List<string>)>,
    IObserver<bool>
{
    private readonly List<IObserver<(HttpListenerContext, List<AnalyzedContent>)>> _observers = [];
    private bool _isModelTrained = false;
    private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;

    public IDisposable Subscribe(IObserver<(HttpListenerContext, List<AnalyzedContent>)> observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<(HttpListenerContext, List<AnalyzedContent>)>(_observers, observer);
    }

    public void OnNext((HttpListenerContext, List<string>) value)
    {
        var context = value.Item1;
        var contentList = value.Item2;
        if (_isModelTrained)
        {
            List<AnalyzedContent> result = [];
            if (_predictionEngine is null) 
                LoadModel();

            foreach (var contentElement in contentList)
            {
                var input = new SentimentData
                {
                    SentimentText = contentElement
                };
                
                var prediction = _predictionEngine!.Predict(input);
                var output = new AnalyzedContent
                {
                    Sentiment = prediction.Prediction!,
                    ContentText = contentElement
                };
                result.Add(output);
            }
            
            NotifyObservers(context, result);
        }
        else
            throw new ModelNotTrainedException();
    }

    public void OnNext(bool value)
    {
        _isModelTrained = value;
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnCompleted()
    {
        Console.WriteLine("");
    }

    private void NotifyObservers(HttpListenerContext context, List<AnalyzedContent> analyzedList)
    {
        foreach (var observer in _observers)
            observer.OnNext((context, analyzedList));
    }

    private void LoadModel()
    {
        var appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) ?? ".";
        var srcPath = Path.Combine(appPath, "..", "..", "..", "src");
        var modelPath = Path.Combine(srcPath, "AiModel", "Models", "model.zip");
        
        var context = new MLContext(seed: 0);
        var model = context.Model.Load(modelPath, out var modelInputSchema);

        _predictionEngine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }
}