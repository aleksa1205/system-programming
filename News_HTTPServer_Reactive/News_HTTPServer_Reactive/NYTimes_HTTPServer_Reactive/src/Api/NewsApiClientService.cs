using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;

namespace NYTimes_HTTPServer_Reactive.Api;

public class NewsApiClientService(int maxResultLength = 100)
{
    private readonly NewsApiClient _newsApiClient = new NewsApiClient("1eba372932c343c8a45752eb795bd5fa");
    private int MaxResultLength { get; } = maxResultLength;

    private List<string> GetContentFromResponse(ArticlesResult response)
    {
        List<string> result = [];
        
        if (response.Status == Statuses.Ok)
        {
            var numberToPrint = MaxResultLength > response.TotalResults ? response.TotalResults : MaxResultLength;
            Console.WriteLine($"\nNumber of articles found: {response.TotalResults}.\nFirst {numberToPrint} will be returned.\n");

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
        var response = await _newsApiClient.GetEverythingAsync(new EverythingRequest
        {
            Q = keyword
        });

        return GetContentFromResponse(response);
    }

    public List<string> GetEverything(string keyword)
    {
        var response = _newsApiClient.GetEverything(new EverythingRequest
        {
            Q = keyword
        });

        return GetContentFromResponse(response);
    }
}