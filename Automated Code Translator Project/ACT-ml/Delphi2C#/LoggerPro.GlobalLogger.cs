
using System;
using LoggerPro; 
using LoggerPro.Appenders; 

namespace LoggerPro
{
   
   public static class GlobalLogger
   {
      private static ILogWriter _logger = null;
      private static readonly object _lock = new object();
      private static bool _shuttedDown = false;

      
      public static ILogWriter Log
      {
         get
         {
            if (_logger == null)
            {
               if (!_shuttedDown)
               {
                  lock (_lock)
                  {
                     
                     if (_logger == null)
                     {
                        
                        _logger = LoggerProBuilder.BuildLogWriter(
                           new ILogAppender[] { new LoggerProFileAppender() }
                        );
                     }
                  }
               }
            }
            return _logger;
         }
      }

      
      public static void ReleaseGlobalLogger()
      {
         if (_logger != null)
         {
            lock (_lock)
            {
               if (_logger != null)
               {
                  _logger = null;
                  _shuttedDown = true;
               }
            }
         }
      }
   }
}
