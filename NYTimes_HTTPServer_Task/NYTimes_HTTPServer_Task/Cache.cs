using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer_Task;
public class Cache
{
    private readonly ReaderWriterLockSlim _cacheLock;
    private readonly HashSet<string> _cache;
    private readonly int _cacheCapacity;

    public Cache(int capacity = 20)
    {
        _cacheLock = new ReaderWriterLockSlim();
        _cacheCapacity = capacity;
        _cache = new HashSet<string>(_cacheCapacity);
    }

    private void SendToFile(string key, string value)
    {
        string path = Directory.GetCurrentDirectory() + "\\files";
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        object locker = new object();
        lock (locker)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(path, $"{key}.txt")))
            {
                writer.Write(value);
            }
        }
    }

    public void Add(string key, string value, int timeout)
    {
        if (!_cacheLock.TryEnterWriteLock(timeout))
        {
            throw new TimeoutException();
        }
        if (_cache.Contains(key))
        {
            throw new DuplicateNameException();
        }
        if (_cache.Count == _cacheCapacity)
        {
            //cleanup
            _cache.Clear();
        }
        _cache.Add(key);
        SendToFile(key, value);
        _cacheLock.ExitWriteLock();
    }

    public void Remove(string key)
    {
        _cacheLock.EnterReadLock();
        if (!_cache.Remove(key))
        {
            throw new KeyNotFoundException();
        }
        string pathToFile = Directory.GetCurrentDirectory() + "\\files" + $"\\{key}";
        File.Delete(pathToFile);
        _cacheLock.ExitReadLock();
    }

    public string Read(string key)
    {
        _cacheLock.EnterReadLock();
        try
        {
            if (!_cache.Contains(key))
            {
                throw new FileNotFoundException();
            }
            string pathToFile = Directory.GetCurrentDirectory() + "\\files" + $"\\{key}.txt";
            string data = File.ReadAllText(pathToFile);
            return data;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    public bool HasKey(string key)
    {
        return _cache.Contains(key);
    }

    //clearing cache at the end
    public void Destroy() 
    {
        _cacheLock.EnterWriteLock();
        _cache.Clear();
        string pathToFile = Directory.GetCurrentDirectory() + "\\files";
        Directory.Delete(pathToFile,true);
        _cacheLock.ExitWriteLock();
    }
}
