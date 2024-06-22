using Microsoft.ML.Data;

namespace NYTimes_HTTPServer_Reactive.AiModel;

public class SentimentData
{
    [LoadColumn(0)]
    public string? Sentiment { get; set; }
    
    [LoadColumn(1)]
    public string? SentimentText { get; set; }
}

public class SentimentPrediction : SentimentData
{
    [ColumnName("PredictedLabel")] 
    public string? Prediction { get; set; }
}