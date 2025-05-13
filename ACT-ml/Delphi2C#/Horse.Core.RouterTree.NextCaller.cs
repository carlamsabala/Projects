using System;
using System.Collections.Generic;
using System.Net;

namespace Horse.Core.RouterTree
{
    
    public delegate void THorseCallback(THorseRequest request, THorseResponse response, Action next);

    
    public delegate bool TCallNextPath(Queue<string> path, TMethodType httpType, THorseRequest request, THorseResponse response);

    
    public enum TMethodType
    {
        GET,
        POST,
        PUT,
        DELETE,
        HEAD,
        PATCH
    }

    
    public class THorseRequest
    {
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    
    public class THorseResponse
    {
        public int Status { get; set; }
        public THorseResponse Send(string message)
        {
            Console.WriteLine("Response: " + message);
            return this;
        }
        public THorseResponse StatusCode(int status)
        {
            Status = status;
            Console.WriteLine("Status: " + status);
            return this;
        }
    }

    
    public class HorseCallbackInterrupted : Exception { }
    public class HorseException : Exception
    {
        public int Status { get; set; }
        public static HorseException New(int status, string message)
        {
            return new HorseException(message) { Status = status };
        }
        public HorseException(string message) : base(message) { }
    }

    
    public class BooleanHolder
    {
        public bool Value { get; set; }
    }

    
    public class TNextCaller
    {
        private int FIndex;
        private int FIndexCallback;
        private Queue<string> FPath;
        private TMethodType FHTTPType;
        private THorseRequest FRequest;
        private THorseResponse FResponse;
        private List<THorseCallback> FMiddleware;
        private Dictionary<TMethodType, List<THorseCallback>> FCallBack;
        private TCallNextPath FCallNextPath;
        private bool FIsGroup;
        private string FTag;
        private bool FIsParamsKey;
        private BooleanHolder FFound;
        private object FHorseCore; 

        
        public TNextCaller(object horseCore)
        {
            FHorseCore = horseCore;
        }

        
        public TNextCaller Init()
        {
            if (!FIsGroup && FPath != null && FPath.Count > 0)
            {
                string LCurrent = FPath.Dequeue();
                if (FIsParamsKey && FRequest != null)
                {
                    
                    FRequest.Params[FTag] = WebUtility.UrlDecode(LCurrent);
                }
            }
            FIndex = -1;
            FIndexCallback = -1;
            return this;
        }

        
        public void Next()
        {
            FIndex++;
            if (FMiddleware != null && FMiddleware.Count > FIndex)
            {
                FFound.Value = true;
                FMiddleware[FIndex](FRequest, FResponse, Next);
                if (FMiddleware.Count > FIndex)
                    Next();
            }
            else if ((FPath == null || FPath.Count == 0) && FCallBack != null)
            {
                FIndexCallback++;
                if (FCallBack.TryGetValue(FHTTPType, out List<THorseCallback> LCallback))
                {
                    if (LCallback.Count > FIndexCallback)
                    {
                        try
                        {
                            FFound.Value = true;
                            LCallback[FIndexCallback](FRequest, FResponse, Next);
                        }
                        catch (Exception E)
                        {
                            if (!(E is HorseCallbackInterrupted) &&
                                !(E is HorseException) &&
                                FResponse.Status < (int)HttpStatusCode.BadRequest)
                            {
                                FResponse.Send("Internal Application Error")
                                         .StatusCode((int)HttpStatusCode.InternalServerError);
                            }
                            throw;
                        }
                        Next();
                    }
                }
                else
                {
                    if (FCallBack.Count > 0)
                    {
                        FFound.Value = true;
                        FResponse.Send("Method Not Allowed")
                                 .StatusCode((int)HttpStatusCode.MethodNotAllowed);
                    }
                    else
                    {
                        FResponse.Send("Not Found")
                                 .StatusCode((int)HttpStatusCode.NotFound);
                    }
                }
            }
            else
            {
                FFound.Value = FCallNextPath(FPath, FHTTPType, FRequest, FResponse);
            }
            if (!FFound.Value)
                FResponse.Send("Not Found")
                         .StatusCode((int)HttpStatusCode.NotFound);
        }

        
        public TNextCaller SetCallback(Dictionary<TMethodType, List<THorseCallback>> aCallback)
        {
            FCallBack = aCallback;
            return this;
        }

        public TNextCaller SetFound(BooleanHolder aFound)
        {
            FFound = aFound;
            return this;
        }

        public TNextCaller SetHTTPType(TMethodType aHTTPType)
        {
            FHTTPType = aHTTPType;
            return this;
        }

        public TNextCaller SetIsGroup(bool aIsGroup)
        {
            FIsGroup = aIsGroup;
            return this;
        }

        public TNextCaller SetIsParamsKey(bool aIsParamsKey)
        {
            FIsParamsKey = aIsParamsKey;
            return this;
        }

        public TNextCaller SetMiddleware(List<THorseCallback> aMiddleware)
        {
            FMiddleware = aMiddleware;
            return this;
        }

        public TNextCaller SetOnCallNextPath(TCallNextPath aCallNextPath)
        {
            FCallNextPath = aCallNextPath;
            return this;
        }

        public TNextCaller SetPath(Queue<string> aPath)
        {
            FPath = aPath;
            return this;
        }

        public TNextCaller SetRequest(THorseRequest aRequest)
        {
            FRequest = aRequest;
            return this;
        }

        public TNextCaller SetResponse(THorseResponse aResponse)
        {
            FResponse = aResponse;
            return this;
        }

        public TNextCaller SetTag(string aTag)
        {
            FTag = aTag;
            return this;
        }

        
        public T End<T>() where T : class
        {
            return FHorseCore as T;
        }
    }
}
