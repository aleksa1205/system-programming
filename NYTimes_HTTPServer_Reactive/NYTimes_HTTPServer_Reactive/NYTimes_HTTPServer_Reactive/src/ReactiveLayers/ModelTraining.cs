using Microsoft.ML;
using NYTimes_HTTPServer_Reactive.AiModel;
using Spectre.Console;
using Console = Spectre.Console.AnsiConsole;

namespace NYTimes_HTTPServer_Reactive.ReactiveLayers;

public class ModelTraining : IObservable<bool>
{
    private readonly MLContext _mlContext = new MLContext(seed: 0);
    private readonly string _appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) ?? ".";
    private readonly string _dataPath;
    private readonly string _modelPath;
    private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;
    private ITransformer? _trainedModel;
    private IDataView? _trainingDataView;
    private DataOperationsCatalog.TrainTestData _trainingDataViewSplit;

    private readonly List<IObserver<bool>> _observers = [];

    public ModelTraining()
    {
        var srcPath = Path.Combine(_appPath, "..", "..", "..", "src");
        _dataPath = Path.Combine(srcPath, "AiModel", "Data", "sentiment_data.csv");
        _modelPath = Path.Combine(srcPath, "AiModel", "Models", "model.zip");
    }

    public void StartTraining()
    {
        _trainingDataView = _mlContext.Data.LoadFromTextFile<SentimentData>(_dataPath, separatorChar: ',');
        _trainingDataViewSplit = _mlContext.Data.TrainTestSplit(_trainingDataView, testFraction: 0.2);

        var pipeline = ProcessData();
        var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);
        Evaluate(_trainingDataView.Schema);
        
        NotifyObservers();
    }

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber<bool>(_observers, observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(true);
            observer.OnCompleted();
        }
    }

    private IEstimator<ITransformer> ProcessData()
    {
        // Extract features and transform the data
        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey(inputColumnName: nameof(SentimentData.Sentiment), outputColumnName: "Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: nameof(SentimentData.SentimentText),
                outputColumnName: "SentimentTextFeaturized"))
            .Append(_mlContext.Transforms.Concatenate("Features", "SentimentTextFeaturized"))
            .AppendCacheCheckpoint(_mlContext);

        return pipeline;
    }
    
    private IEstimator<ITransformer> BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline)
    {
        var trainingPipeline = pipeline
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _trainedModel = trainingPipeline.Fit(_trainingDataViewSplit.TrainSet);
        
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_trainedModel);
        
        return trainingPipeline;
    }

    private void Evaluate(DataViewSchema trainingDataViewSchema)
    {
        var dataTestView = _trainedModel!.Transform(_trainingDataViewSplit.TestSet);
        var testMetrics =
            _mlContext.MulticlassClassification.Evaluate(dataTestView);
        
        var rule = new Rule("Create and Train Model");
        Console
            .Live(rule)
            .Start(console =>
            {
                rule.Title = "Training Complete, Evaluating Accuracy.";
                console.Refresh();

                var table = new Table()
                    .MinimalBorder()
                    .Title("Model Accuracy");
                table.AddColumns("MicroAccuracy", "MacroAccuracy", "LogLoss", "LogLossReduction");
                table.AddRow($"{testMetrics.MicroAccuracy:P2}", $"{testMetrics.MacroAccuracy:P2}", $"{testMetrics.LogLoss:0.###}", $"{testMetrics.LogLossReduction:0.###}");

                console.UpdateTarget(table);
                console.Refresh();
            });
        
        SaveModelAsFile(_mlContext, trainingDataViewSchema, _trainedModel);
    }
    
    private void SaveModelAsFile(MLContext mlContext,DataViewSchema trainingDataViewSchema, ITransformer model)
    {
        mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
    }
}