
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Path;
using LoggerPro; 

namespace LoggerPro.Appenders
{
    
    public class LoggerProOutputDebugStringAppender : LoggerProAppenderBase, ILogAppender
    {
#if WINDOWS
        private string _moduleName;
#endif

        public LoggerProOutputDebugStringAppender(ILogItemRenderer logItemRenderer = null)
            : base(logItemRenderer)
        {
        }

        public override void Setup()
        {
            base.Setup();
#if WINDOWS
            
            try
            {
                string fullPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                _moduleName = Path.GetFileName(fullPath);
            }
            catch
            {
                _moduleName = "UnknownModule";
            }
#endif
        }

        public override void TearDown()
        {
            base.TearDown();
            
        }

        public override void WriteLog(LogItem logItem)
        {
#if WINDOWS
            
            string logText = $"({_moduleName}) {FormatLog(logItem)}";
            OutputDebugString(logText);
#endif
        }

        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern void OutputDebugString(string lpOutputString);
    }
}
