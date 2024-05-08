using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer;

public class ApiService
{
    private readonly string baseURL;
    private readonly string apiKey;
    private readonly HttpClient client;

    public ApiService()
    {
        baseURL = "https://api.nytimes.com/svc/books/v3/reviews.json";
        apiKey = "UcAapg6PsCo4lydmhGVWqop8DNx1AmU5";
        client = new HttpClient();
    }

    public List<string> FetchData(string name, string surname)
    {
        List<Book> books=new List<Book>();

        string url = $"{baseURL}?author={name}+{surname}&api-key={apiKey}";
        HttpResponseMessage response = client.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();

        //ovo je sinhorna operacija
        var responseBody = response.Content.ReadAsStringAsync().Result;
        var obj = JObject.Parse(responseBody);
        if (responseBody.Contains("faultstring"))
        {
            books = null!;
            throw new HttpRequestException();
        }
        var res = obj["results"];
        books = res!.ToObject<List<Book>>()!;

        return books.Select(x => x.book_title).ToList();
    }
}

//await client.GetAsync(url).ContinueWith(async (response) =>
//{
//    var responseBody = await response.Result.Content.ReadAsStringAsync();
//    var obj = JObject.Parse(responseBody);
//    if (responseBody.Contains("faultstring"))
//    {
//        books = null!;
//        return;
//    }
//    var res = obj["results"];
//    books = res!.ToObject<List<Book>>()!;

//    data.Add(JObject.Parse(responseBody));
//});