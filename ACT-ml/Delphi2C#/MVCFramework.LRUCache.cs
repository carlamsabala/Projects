using System;
using System.Collections.Generic;
using System.Threading;

namespace MVCFramework
{

    public delegate bool MVCLRUCacheAction(string key);

    
    public class MVCLRUCache<T> where T : class
    {
        
        private class MVCLRUCacheItem
        {
            public string Key { get; }
            public T Value { get; }

            public MVCLRUCacheItem(string key, T item)
            {
                Key = key;
                Value = item;
            }
        }

        
        private readonly List<MVCLRUCacheItem> _cache;
        
        private readonly int _capacity;
        
        private readonly object _syncRoot = new object();

        
        public MVCLRUCache(int capacity)
        {
            _capacity = capacity;
            _cache = new List<MVCLRUCacheItem>();
        }

        
        public void Clear()
        {
            lock (_syncRoot)
            {
                _cache.Clear();
            }
        }

        
        public bool Contains(string key, out int itemIndex)
        {
            lock (_syncRoot)
            {
                for (int i = 0; i < _cache.Count; i++)
                {
                    if (key == _cache[i].Key)
                    {
                        itemIndex = i;
                        return true;
                    }
                }
                itemIndex = -1;
                return false;
            }
        }

        
        public void Put(string key, T item)
        {
            lock (_syncRoot)
            {
                
                if (_cache.Count == _capacity)
                {
                    
                    _cache.RemoveAt(_cache.Count - 1);
                }
                
                _cache.Insert(0, new MVCLRUCacheItem(key, item));
            }
        }

        
        public bool TryGet(string key, out T item)
        {
            lock (_syncRoot)
            {
                int index;
                if (Contains(key, out index))
                {
                    if (index > 0)
                    {
                        
                        var cacheItem = _cache[index];
                        _cache.RemoveAt(index);
                        _cache.Insert(0, cacheItem);
                    }
                    item = _cache[0].Value;
                    return true;
                }
                item = null;
                return false;
            }
        }

        
        public void RemoveIf(MVCLRUCacheAction action)
        {
            lock (_syncRoot)
            {
                int index = 0;
                while (index < _cache.Count)
                {
                    if (action(_cache[index].Key))
                    {
                        _cache.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
        }

        
        public uint Size()
        {
            lock (_syncRoot)
            {
                return (uint)_cache.Count;
            }
        }

        
        public void Lock()
        {
            Monitor.Enter(_syncRoot);
        }

        
        public void UnLock()
        {
            Monitor.Exit(_syncRoot);
        }
    }
}
