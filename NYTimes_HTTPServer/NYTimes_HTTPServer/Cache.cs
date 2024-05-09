using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer;

public struct Data
{
    public string data;
    public DateTime time;

    public Data(string data)
    {
        this.data = data;
        time = DateTime.Now;
    }
}

public class Cache
{
    private readonly ReaderWriterLockSlim _cacheLock;
    private readonly Dictionary<string, Data> _cache;
    private int _cacheCapacity;

    public Cache(int capacity = 5)
    {
        _cacheLock = new ReaderWriterLockSlim();
        _cacheCapacity = capacity;
        _cache= new Dictionary<string, Data>(_cacheCapacity);
    }

    public void Add(string key, string value, int timeout)
    {
        if (!_cacheLock.TryEnterWriteLock(timeout))
        {
            throw new TimeoutException();
        }
        if (_cache.ContainsKey(key))
        {
            throw new DuplicateNameException();
        }
        _cache.Add(key, new Data(value));
        _cacheLock.ExitWriteLock();
    }

    public void Remove(string key)
    {
        _cacheLock.EnterReadLock();
        if (!_cache.Remove(key))
        {
            throw new KeyNotFoundException();
        }
        _cacheLock.ExitReadLock();
    }

    public string Read(string key)
    {
        _cacheLock.EnterReadLock();
        try
        {
            return _cache[key].data;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    public bool HasKey(string key)
    {
        if (_cache.ContainsKey(key))
        {
            if (_cache[key].time.AddMinutes(15) >= DateTime.Now)
            {
                return true;
            }
            else
            {
                Remove(key);
            }
        }
        return false;
    }
}
