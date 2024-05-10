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

    public List<string> FetchDataTitles(string name, string surname)
    {
        List<string> books=new List<string>();

        string url = $"{baseURL}?author={name}+{surname}&api-key={apiKey}";
        HttpResponseMessage response = client.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();

        //ovo je sinhorna operacija
        var responseBody = response.Content.ReadAsStringAsync().Result;
        var obj = JObject.Parse(responseBody);
        if (responseBody.Contains("faultstring"))
        {
            throw new HttpRequestException();
        }
        foreach (var el in obj["results"]!)
        {
            books.Add(el["book_title"]!.ToObject<string>()!);
        }
        return books;
    }
}

//FetchData koriscenjem await i async bolji nacin bice obradjen u 2. domacem
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

//Nacin 1 sa klasom Book
//var res = obj["results"];
//books = res!.ToObject<List<Book>>()!;
//return books.Select(x => x.book_title).ToList();