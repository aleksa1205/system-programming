using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;

namespace NYTimes_HTTPServer_Reactive;

public class NewsApiClientService(int maxResultLength = 100)
{
    private readonly NewsApiClient _newsApiClient = new NewsApiClient("f8aeb492cf6b4507b2ba6f6879994498");
    private int MaxResultLength { get; } = maxResultLength;

    public async Task<List<string>> GetEverything(string keyword)
    {
        var response = await _newsApiClient.GetEverythingAsync(new EverythingRequest
        {
            Q = keyword
        });

        List<string> result = [];
        
        if (response.Status == Statuses.Ok)
        {
            Console.WriteLine($"Number of articles found: {response.TotalResults}.\nFirst {MaxResultLength} will be returned.");

            result = 
                (from article in response.Articles
                select article.Content)
                .Take(MaxResultLength)
                .ToList();
            
        }
        else
        {
            Console.WriteLine($"Get everything call failed with code: {response.Error.Code}\n{response.Error.Message}");
        }

        return result;
    }
}