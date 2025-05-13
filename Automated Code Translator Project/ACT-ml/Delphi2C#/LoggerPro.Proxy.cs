
using System;
using LoggerPro; 

namespace LoggerPro.Proxy
{
    
    public interface ILogAppenderProxy
    {
        ILogAppender InternalAppender { get; }
    }

    
    public static class LoggerProFilter
    {
        
        public static ILogAppender Build(ILogAppender appender, Func<LogItem, bool> filter)
        {
            return new LoggerProAppenderFilterImpl(appender, filter);
        }
    }

    
    public delegate bool LogWriterPredicate(LogType logType, string message, string tag);

    
    public class LogWriterDecorator : ILogWriter
    {
        private readonly ILogWriter _decoratedLogWriter;
        private readonly LogWriterPredicate _filter;

        
        protected LogWriterDecorator(ILogWriter logWriter, LogWriterPredicate filter)
        {
            _decoratedLogWriter = logWriter;
            _filter = filter;
        }

        
        public static ILogWriter Build(ILogWriter logWriter, LogWriterPredicate filter)
        {
            return new LogWriterDecorator(logWriter, filter);
        }

        
        public void Debug(string message, string tag)
        {
            Log(LogType.Debug, message, tag);
        }

        public void Debug(string message, object[] args, string tag)
        {
            Log(LogType.Debug, string.Format(message, args), tag);
        }

        public void Info(string message, string tag)
        {
            Log(LogType.Info, message, tag);
        }

        public void Info(string message, object[] args, string tag)
        {
            Log(LogType.Info, string.Format(message, args), tag);
        }

        public void Warn(string message, string tag)
        {
            Log(LogType.Warning, message, tag);
        }

        public void Warn(string message, object[] args, string tag)
        {
            Log(LogType.Warning, string.Format(message, args), tag);
        }

        public void Error(string message, string tag)
        {
            Log(LogType.Error, message, tag);
        }

        public void Error(string message, object[] args, string tag)
        {
            Log(LogType.Error, string.Format(message, args), tag);
        }

        public void Fatal(string message, string tag)
        {
            Log(LogType.Fatal, message, tag);
        }

        public void Fatal(string message, object[] args, string tag)
        {
            Log(LogType.Fatal, string.Format(message, args), tag);
        }

        public void Log(LogType type, string message, string tag)
        {
            if (_filter(type, message, tag))
            {
                _decoratedLogWriter.Log(type, message, tag);
            }
        }

        public void Log(LogType type, string message, object[] args, string tag)
        {
            Log(type, string.Format(message, args), tag);
        }

        
        public string[] GetAppendersClassNames() => _decoratedLogWriter.GetAppendersClassNames();

        public ILogAppender GetAppender(int index) => _decoratedLogWriter.GetAppender(index);

        public void AddAppender(ILogAppender appender) => _decoratedLogWriter.AddAppender(appender);

        public void DelAppender(ILogAppender appender) => _decoratedLogWriter.DelAppender(appender);

        public int AppendersCount() => _decoratedLogWriter.AppendersCount();
    }

    
    internal class LoggerProAppenderFilterImpl : LoggerProAppenderBase, ILogAppender, ILogAppenderProxy
    {
        private readonly ILogAppender _internalAppender;
        private readonly Func<LogItem, bool> _filter;

        public LoggerProAppenderFilterImpl(ILogAppender appender, Func<LogItem, bool> filter)
        {
            _filter = filter;
            _internalAppender = appender;
        }

        public ILogAppender InternalAppender => _internalAppender;

        public override void Setup()
        {
            _internalAppender.Setup();
        }

        public override void TearDown()
        {
            _internalAppender.TearDown();
        }

        public override void WriteLog(LogItem logItem)
        {
            if (_filter(logItem))
            {
                _internalAppender.WriteLog(logItem);
            }
        }
    }
}
