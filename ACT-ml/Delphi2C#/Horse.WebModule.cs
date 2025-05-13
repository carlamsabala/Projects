using System;
using Horse.Request;     
using Horse.Response;    
using Horse.Core;        
using System.Web;       

namespace Horse.WebModule
{
    
    public class HorseWebModule 
    {
        private THorseCore _horse;
        private static HorseWebModule _instance;

        
        public THorseCore Horse
        {
            get { return _horse; }
            set { _horse = value; }
        }

       
        public HorseWebModule(object owner = null)
        {
            
            _horse = THorseCore.GetInstance();
            _instance = this;
        }

        
        public static HorseWebModule GetInstance()
        {
            return _instance;
        }

        
        public void HandlerAction(object sender, TWebRequest request, TWebResponse response, ref bool handled)
        {
            handled = true;
            
            HorseRequest horseRequest = new HorseRequest(request);
            HorseResponse horseResponse = new HorseResponse(response);
            try
            {
                try
                {
                    
                    _horse.Routes.Execute(horseRequest, horseResponse);
                }
                catch (Exception ex)
                {
                    
                    if (!(ex is HorseCallbackInterruptedException))
                    {
                        throw;
                    }
                }
            }
            finally
            {
                
                if (horseRequest.Body<object>() == horseResponse.Content())
                {
                    horseResponse.Content(null);
                }
                horseRequest.Dispose();
                horseResponse.Dispose();
            }
        }
    }
}
