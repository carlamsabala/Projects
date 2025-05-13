using System;

namespace Horse.Core.Route.Contract
{
    
    public delegate void THorseCallback();
    public delegate void THorseCallbackRequestResponse();
    public delegate void THorseCallbackRequest();
    public delegate void THorseCallbackResponse();

    public interface IHorseCoreRoute<T> where T : class
    {
        IHorseCoreRoute<T> AddCallback(THorseCallback callback);
        IHorseCoreRoute<T> AddCallbacks(THorseCallback[] callbacks);
        IHorseCoreRoute<T> All(THorseCallback callback);
        IHorseCoreRoute<T> All(THorseCallback middleware, THorseCallback callback);
        IHorseCoreRoute<T> All(params THorseCallback[] callbacks);
        IHorseCoreRoute<T> All(THorseCallback[] callbacks, THorseCallback callback);
        IHorseCoreRoute<T> Get(THorseCallback callback);
        IHorseCoreRoute<T> Get(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Get(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Get(THorseCallbackResponse callback);
        IHorseCoreRoute<T> Put(THorseCallback callback);
        IHorseCoreRoute<T> Put(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Put(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Put(THorseCallbackResponse callback);
        IHorseCoreRoute<T> Head(THorseCallback callback);
        IHorseCoreRoute<T> Head(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Head(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Head(THorseCallbackResponse callback);
        IHorseCoreRoute<T> Post(THorseCallback callback);
        IHorseCoreRoute<T> Post(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Post(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Post(THorseCallbackResponse callback);
        IHorseCoreRoute<T> Patch(THorseCallback callback);
        IHorseCoreRoute<T> Delete(THorseCallback callback);
        IHorseCoreRoute<T> Patch(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Patch(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Patch(THorseCallbackResponse callback);
        IHorseCoreRoute<T> Delete(THorseCallbackRequestResponse callback);
        IHorseCoreRoute<T> Delete(THorseCallbackRequest callback);
        IHorseCoreRoute<T> Delete(THorseCallbackResponse callback);
        T End();
    }
}
