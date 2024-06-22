namespace NYTimes_HTTPServer_Reactive;

public static class Utils
{
    private static string GetAppPath()
    {
        return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) ?? ".";
    }

    public static string GetSrcPath()
    {
        var appPath = GetAppPath();
        return Path.Combine(appPath, "..", "..", "..", "src");
    }

    public static string GetDataPath()
    {
        var srcPath = GetSrcPath();
        return Path.Combine(srcPath, "AiModel", "Data", "sentiment_data.csv");
    }

    public static string GetModelPath()
    {
        var srcPath = GetSrcPath();
        return Path.Combine(srcPath, "AiModel", "Models", "model.zip");
    }
}