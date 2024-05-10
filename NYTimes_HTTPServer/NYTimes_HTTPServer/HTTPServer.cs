using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NYTimes_HTTPServer;

public class HTTPServer
{
    private readonly HttpListener _httpListener;
    private readonly Thread _listenerThread;
    private readonly Cache _cache;
    private readonly ReaderWriterLockSlim _readWriteLock;
    private bool _running;

    public HTTPServer(string address = "localhost", int port = 5050)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://{address}:{port}/");
        _listenerThread = new Thread(Listen);
        _cache = new Cache();
        _readWriteLock = new ReaderWriterLockSlim();
        _running = false;
    }

    private static void SendResponse(HttpListenerContext context, byte[] responseBody, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var logString =
            $"REQUEST:\n{context.Request.HttpMethod} {context.Request.RawUrl} HTTP/{context.Request.ProtocolVersion}\n" +
            $"Host: {context.Request.UserHostName}\nUser-agent: {context.Request.UserAgent}\n-------------------\n" +
            $"RESPONSE:\nStatus: {statusCode}\nDate: {DateTime.Now}\nContent-Type: {contentType}" +
            $"\nContent-Length: {responseBody.Length}\n";
        context.Response.ContentType = contentType;
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentLength64 = responseBody.Length;
        using (Stream outputStream = context.Response.OutputStream)
        {
            outputStream.Write(responseBody, 0, responseBody.Length);
        }
        Console.WriteLine(logString);
    }

    private void SendToFile(string name, string surname, string content)
    {
        _readWriteLock.EnterWriteLock();
        try
        {
            string pathToFile = Directory.GetCurrentDirectory() + "\\files";
            using (StreamWriter outputStream = new StreamWriter(Path.Combine(pathToFile, $"{name} {surname}.txt")))
            {
                outputStream.Write(content);
            }
            Console.WriteLine("Written to file!");
        }
        catch(Exception)
        {
            Console.WriteLine("Error while trying to write to file!");
        }
        finally
        {
            _readWriteLock.ExitWriteLock();
        }
    }

    private void AcceptConnection(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.HttpMethod != "GET")
        {
            Console.WriteLine("Method not allowed!");
            SendResponse(context, "Method not allowed!"u8.ToArray(), "text/plain", HttpStatusCode.MethodNotAllowed);
            return;
        }
        try
        {
            ApiService apiService = new ApiService();
            string url = request.RawUrl!;
            string allTitles;

            if (url == String.Empty)
            {
                SendResponse(context, "No name and surname given!"u8.ToArray(), "text/plain", HttpStatusCode.BadRequest);
                return;
            }

            if(!url.Contains("_"))
            {
                SendResponse(context, "Invalid format for name and surname!"u8.ToArray(), "text/plain", HttpStatusCode.NotAcceptable);
                return;
            }    
            var locationOfPart = url!.IndexOf("_");
            var name = url.Substring(1, locationOfPart - 1);
            var surname = url.Substring(locationOfPart + 1);
            if(!name.All(Char.IsLetter) && !surname.All(Char.IsLetter))
            {
                SendResponse(context, "Name and surname can only contain letters!"u8.ToArray(), "text/plain", HttpStatusCode.UnprocessableContent);
                return;
            }

            string fileName = name + ' ' + surname;
            if (_cache.HasKey(fileName))
            {
                allTitles = _cache.Read(fileName);
                Console.WriteLine("OBTAINED FROM CACHE!");
            }
            else
            {
                List<string> dataTitles = apiService.FetchDataTitles(name, surname);
                if (dataTitles!.Count == 0)
                {
                    SendResponse(context, "No data for given name and surname found!"u8.ToArray(), "text/plain", HttpStatusCode.NoContent);
                    return;
                }
                allTitles = String.Join(Environment.NewLine, dataTitles);
                SendToFile(name, surname, allTitles);
                _cache.Add(fileName, allTitles, 1000);
            }
            byte[] dataAsBytes = System.Text.Encoding.UTF8.GetBytes(allTitles);
            SendResponse(context, dataAsBytes, "text/plain");
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("List with given key does not exist in cache!");
            SendResponse(context, "List with given key does not exist in cache!"u8.ToArray(), "text/plain", HttpStatusCode.NotFound);
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Time for writing into cache has passed!");
            SendResponse(context, "Time for writing into cache has passed!"u8.ToArray(), "text/plain", HttpStatusCode.RequestTimeout);
        }
        catch (DuplicateNameException)
        {
            Console.WriteLine("List with given key already exists in cache!");
            SendResponse(context, "List with given key already exists in cache!"u8.ToArray(), "text/plain", HttpStatusCode.Conflict);
        }
        catch(HttpRequestException)
        {
            Console.WriteLine("API returned an error!");
            SendResponse(context, "API returned an error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
        }
        catch(Exception)
        {
            Console.WriteLine("Unknown error!");
            SendResponse(context, "Unknown error!"u8.ToArray(), "text/plain", HttpStatusCode.InternalServerError);
        }
    }

    public void Start()
    {
        _httpListener.Start();
        _listenerThread.Start();
        _running = true;
        Console.WriteLine("Server started!");
    }
    
    public void Stop()
    {
        _httpListener.Stop();
        _listenerThread.Join();
        _running = false;
        Console.WriteLine("Server stopped!");
    }
    
    private void Listen()
    {
        while (_running)
        {
            try
            {
                var context = _httpListener.GetContext();
                if (_running)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        AcceptConnection(context);
                    });
                }

            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Server stopped listening!");
            }
        }
    }
}
