using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer;

public class HTTPServer
{
    private readonly HttpListener _httpListener;
    private readonly Thread _listenerThread;
    private bool _running;

    public HTTPServer(string address="localhost", int port = 5050)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://{address}:{port}/");
        _listenerThread = new Thread(Listen);
        _running = true;
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
