using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer_Task;

public class ApiService
{
    private readonly string _baseURL;
    private readonly string _apiKey;
    private readonly HttpClient _client;

    public ApiService()
    {
        _baseURL = "https://api.nytimes.com/svc/books/v3/reviews.json";
        _apiKey = "UcAapg6PsCo4lydmhGVWqop8DNx1AmU5";
        _client = new HttpClient();
    }

    public async Task<List<string>> FetchDataTitles(string name, string surname)
    {
        List<string> books = new List<string>();
        string url = $"{_baseURL}?author={name}+{surname}&api-key={_apiKey}";
        HttpResponseMessage response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(responseBody);
        if (responseBody.Contains("faultstring"))
        {
            throw new HttpRequestException();
        }
        foreach(var el in obj["results"]!)
        {
            books.Add(el["book_title"]!.ToObject<string>()!);
        }
        return books;
    }
}