using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;

namespace NYTimes_HTTPServer_Reactive.Api;

public class NewsApiClientService(int maxResultLength = 100)
{
    private readonly NewsApiClient _newsApiClient = new NewsApiClient("f8aeb492cf6b4507b2ba6f6879994498");
    private int MaxResultLength { get; } = maxResultLength;

    private List<string> GetContentFromResponse(ArticlesResult response)
    {
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
    
    public async Task<List<string>> GetEverythingAsync(string keyword)
    {
        try
        {
            var response = await _newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = keyword
            });

            return GetContentFromResponse(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }

    public List<string> GetEverything(string keyword)
    {
        try
        {
            var response = _newsApiClient.GetEverything(new EverythingRequest
            {
                Q = keyword
            });

            return GetContentFromResponse(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }
}