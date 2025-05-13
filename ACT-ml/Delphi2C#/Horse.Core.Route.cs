using System;
using Horse.Core.Route.Contract; 
using Horse.Core;                

namespace Horse.Core.Route
{
    public class HorseCoreRoute<T> : IHorseCoreRoute<T> where T : class
    {
        private readonly string _path;
        private readonly object _horseCore; 

        public HorseCoreRoute(string aPath)
        {
            _path = aPath;
            
            _horseCore = HorseCore.GetInstance();
        }

        public IHorseCoreRoute<T> This() => this;

        public IHorseCoreRoute<T> AddCallback(THorseCallback callback)
        {
            ((HorseCore)_horseCore).AddCallback(callback);
            return this;
        }

        public IHorseCoreRoute<T> AddCallbacks(THorseCallback[] callbacks)
        {
            foreach (var cb in callbacks)
            {
                AddCallback(cb);
            }
            return this;
        }

        public IHorseCoreRoute<T> All(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Use(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> All(THorseCallback middleware, THorseCallback callback)
        {
            ((HorseCore)_horseCore).Use(_path, new THorseCallback[] { middleware, callback });
            return this;
        }

        public IHorseCoreRoute<T> All(THorseCallback[] callbacks)
        {
            ((HorseCore)_horseCore).Use(_path, callbacks);
            return this;
        }

        public IHorseCoreRoute<T> All(THorseCallback[] callbacks, THorseCallback callback)
        {
            ((HorseCore)_horseCore).Use(_path, callbacks);
            ((HorseCore)_horseCore).Use(_path, new THorseCallback[] { callback });
            return this;
        }

        public IHorseCoreRoute<T> Get(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Get(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Get(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Get(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Get(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Get(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Get(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Get(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Put(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Put(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Put(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Put(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Put(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Put(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Put(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Put(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Head(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Head(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Head(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Head(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Head(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Head(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Head(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Head(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Post(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Post(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Post(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Post(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Post(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Post(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Post(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Post(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Patch(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Patch(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Patch(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Patch(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Patch(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Patch(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Patch(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Patch(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Delete(THorseCallback callback)
        {
            ((HorseCore)_horseCore).Delete(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Delete(THorseCallbackRequestResponse callback)
        {
            ((HorseCore)_horseCore).Delete(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Delete(THorseCallbackRequest callback)
        {
            ((HorseCore)_horseCore).Delete(_path, callback);
            return this;
        }

        public IHorseCoreRoute<T> Delete(THorseCallbackResponse callback)
        {
            ((HorseCore)_horseCore).Delete(_path, callback);
            return this;
        }

        public T End()
        {
            return _horseCore as T;
        }
    }
}
