
using System;
using System.Collections.Generic;
using System.Threading;
using LoggerPro; 

namespace LoggerPro.Appenders
{
   
    public class MREWLogItemList
    {
        private readonly List<TLogItem> _list = new List<TLogItem>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        
        public List<TLogItem> BeginWrite()
        {
            _lock.EnterWriteLock();
            return _list;
        }

        
        public void EndWrite()
        {
            _lock.ExitWriteLock();
        }
    }

    
    public class LoggerProMemoryAppender : LoggerProAppenderBase, ILogAppender
    {
        private readonly MREWLogItemList _mrewLogList;
        private readonly string _tag;
        private readonly int _maxSize;

        
        public LoggerProMemoryAppender(MREWLogItemList logList, string tag, int maxSize)
        {
            _mrewLogList = logList;
            _tag = tag;
            _maxSize = maxSize;
        }

        
        public override void Setup()
        {
            base.Setup();
            
        }

        
        public override void TearDown()
        {
            base.TearDown();
            
        }

        
        public override void WriteLog(TLogItem logItem)
        {
            if (logItem.LogTag != _tag)
                return;

            List<TLogItem> list = _mrewLogList.BeginWrite();
            try
            {
                if (list.Count >= _maxSize)
                {
                    
                    int threshold = (int)(_maxSize * 0.9);
                    while (list.Count > threshold)
                    {
                        list.RemoveAt(0);
                    }
                }
                list.Add(logItem.Clone());
            }
            finally
            {
                _mrewLogList.EndWrite();
            }
        }
    }
}
