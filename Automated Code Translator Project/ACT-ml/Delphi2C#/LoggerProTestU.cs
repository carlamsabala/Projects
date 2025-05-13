
using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using LoggerPro;            
using LoggerPro.Proxy;     

namespace LoggerPro.Tests
{
    
    public static class TestHelpers
    {
        
        public static bool LogItemAreEquals(LogItem a, LogItem b)
        {
            Assert.AreEqual(a.LogType, b.LogType, "LogType is different");
            Assert.AreEqual(a.LogMessage, b.LogMessage, "LogMessage is different");
            Assert.AreEqual(a.LogTag, b.LogTag, "LogTag is different");
            Assert.AreEqual(a.TimeStamp, b.TimeStamp, "TimeStamp is different");
            Assert.AreEqual(a.ThreadID, b.ThreadID, "ThreadID is different");
            return true;
        }
    }

    
    public class TestAppender : ILogAppender
    {
        private readonly Action _onSetup;
        private readonly Action _onTearDown;
        private readonly Action<LogItem> _onWriteLog;

        public TestAppender(Action onSetup, Action onTearDown, Action<LogItem> onWriteLog)
        {
            _onSetup = onSetup;
            _onTearDown = onTearDown;
            _onWriteLog = onWriteLog;
        }

        public void Setup() => _onSetup?.Invoke();

        public void TearDown() => _onTearDown?.Invoke();

        public void WriteLog(LogItem logItem) => _onWriteLog?.Invoke(logItem);

        
        public int AppendersCount => 1;

        
        public string[] GetAppendersClassNames() => new string[] { GetType().Name };
    }

    
    [TestFixture]
    public class LoggerProTest
    {
        
        private LogItem _lastLogItem;

        [SetUp]
        public void Setup()
        {
            // Nothing needed here.
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up if needed.
        }

        [Test]
        public void TestTLogItemClone()
        {
            LogItem logItem = new LogItem(LogType.Debug, "message", "tag");
            try
            {
                LogItem clonedLogItem = logItem.Clone();
                try
                {
                    Assert.IsTrue(TestHelpers.LogItemAreEquals(logItem, clonedLogItem));
                }
                finally
                {
                    clonedLogItem.Dispose();
                }
            }
            finally
            {
                logItem.Dispose();
            }
        }

        [Test]
        [TestCase(0, "DEBUG")]
        [TestCase(1, "INFO")]
        [TestCase(2, "WARNING")]
        [TestCase(3, "ERROR")]
        [TestCase(4, "FATAL")]
        public void TestTLogItemTypeAsString(byte logType, string expected)
        {
            LogItem logItem = new LogItem((LogType)logType, "message", "tag");
            try
            {
                Assert.AreEqual(expected, logItem.LogTypeAsString);
            }
            finally
            {
                logItem.Dispose();
            }
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestLogLevel(bool useProxy)
        {
            
            List<string> history = new List<string>();
            object syncLock = new object();
            long invalidItemLogged = 0;
            ManualResetEvent mre = new ManualResetEvent(false);

            
            TestAppender appender = new TestAppender(
                onSetup: () => history.Add("setup"),
                onTearDown: () => history.Add("teardown"),
                onWriteLog: (logItem) =>
                {
                    history.Add("writelog" + logItem.LogTypeAsString);
                    
                    if (logItem.LogMessage.Equals("ignoredmessage", StringComparison.Ordinal))
                    {
                        Interlocked.Increment(ref invalidItemLogged);
                    }
                    
                    lock (syncLock)
                    {
                        _lastLogItem?.Dispose();
                        _lastLogItem = logItem.Clone();
                        mre.Set();
                    }
                }
            );

            ILogAppender finalAppender = appender;
            if (useProxy)
            {
                
                finalAppender = LoggerProFilter.Build(appender, logItem =>
                {
                    return !logItem.LogMessage.Equals("ignoredmessage", StringComparison.Ordinal);
                });
            }
            invalidItemLogged = 0;
            ILogWriter logWriter = LoggerProExtensions.BuildLogWriter(new ILogAppender[] { finalAppender });

            
            mre.Reset();
            logWriter.Debug("debug message", "debug");
            if (useProxy)
                logWriter.Debug("ignoredmessage", "debug");
            Assert.IsTrue(mre.WaitOne(5000), "Event not released after 5 seconds");
            lock (syncLock)
            {
                Assert.AreEqual("debug message", _lastLogItem.LogMessage);
                Assert.AreEqual("debug", _lastLogItem.LogTag);
                Assert.AreEqual("DEBUG", _lastLogItem.LogTypeAsString);
                Assert.AreEqual(0, invalidItemLogged);
            }

            
            mre.Reset();
            invalidItemLogged = 0;
            logWriter.Info("info message", "info");
            if (useProxy)
                logWriter.Info("ignoredmessage", "info");
            Assert.IsTrue(mre.WaitOne(5000), "Event not released after 5 seconds");
            lock (syncLock)
            {
                Assert.AreEqual("info message", _lastLogItem.LogMessage);
                Assert.AreEqual("info", _lastLogItem.LogTag);
                Assert.AreEqual("INFO", _lastLogItem.LogTypeAsString);
                Assert.AreEqual(0, invalidItemLogged);
            }

            
            mre.Reset();
            invalidItemLogged = 0;
            logWriter.Warn("warning message", "warning");
            if (useProxy)
                logWriter.Warn("ignoredmessage", "warning");
            Assert.IsTrue(mre.WaitOne(5000), "Event not released after 5 seconds");
            lock (syncLock)
            {
                Assert.AreEqual("warning message", _lastLogItem.LogMessage);
                Assert.AreEqual("warning", _lastLogItem.LogTag);
                Assert.AreEqual("WARNING", _lastLogItem.LogTypeAsString);
                Assert.AreEqual(0, invalidItemLogged);
            }

            
            mre.Reset();
            invalidItemLogged = 0;
            logWriter.Error("error message", "error");
            if (useProxy)
                logWriter.Error("ignoredmessage", "error");
            Assert.IsTrue(mre.WaitOne(5000), "Event not released after 5 seconds");
            lock (syncLock)
            {
                Assert.AreEqual("error message", _lastLogItem.LogMessage);
                Assert.AreEqual("error", _lastLogItem.LogTag);
                Assert.AreEqual("ERROR", _lastLogItem.LogTypeAsString);
                Assert.AreEqual(0, invalidItemLogged);
            }

            
            mre.Reset();
            invalidItemLogged = 0;
            logWriter.Fatal("fatal message", "fatal");
            if (useProxy)
                logWriter.Fatal("ignoredmessage", "fatal");
            Assert.IsTrue(mre.WaitOne(5000), "Event not released after 5 seconds");
            lock (syncLock)
            {
                Assert.AreEqual("fatal message", _lastLogItem.LogMessage);
                Assert.AreEqual("fatal", _lastLogItem.LogTag);
                Assert.AreEqual("FATAL", _lastLogItem.LogTypeAsString);
                Assert.AreEqual(0, invalidItemLogged);
            }

            
            logWriter = null;
            
            Assert.AreEqual(7, history.Count);
            Assert.AreEqual("setup", history[0]);
            Assert.AreEqual("writelogDEBUG", history[1]);
            Assert.AreEqual("writelogINFO", history[2]);
            Assert.AreEqual("writelogWARNING", history[3]);
            Assert.AreEqual("writelogERROR", history[4]);
            Assert.AreEqual("writelogFATAL", history[5]);
            Assert.AreEqual("teardown", history[6]);
        }

        [Test]
        public void TestAddAndDeleteAppenders()
        {
            
            ILogAppender appender1 = new OutputDebugStringAppender();
            ILogAppender appender2 = new OutputDebugStringAppender();

            ILogWriter logWriter = LoggerProExtensions.BuildLogWriter(new ILogAppender[] { appender1, appender2 });
            logWriter.Debug("Added Appenders", "Appender");
            Assert.AreEqual(2, logWriter.AppendersCount);

            logWriter.DelAppender(appender1);
            logWriter.Debug("Deleted Appenders", "Appender");
            Assert.AreEqual(1, logWriter.AppendersCount);

            logWriter.DelAppender(appender2);
            logWriter.Debug("Deleted Appenders", "Appender");
            Assert.AreEqual(0, logWriter.AppendersCount);

            
            logWriter.Debug("Deleted Appenders", "Appender");
        }

        [Test]
        [TestCase("{timestamp}|{threadid}|{loglevel}|{message}|{tag},2020-03-15 12:30:20:123|    1234|LOGLEVEL|THIS IS THE MESSAGE|THE_TAG",
                  "{timestamp}|{threadid}|{loglevel}|{message}|{tag},2020-03-15 12:30:20:123|    1234|LOGLEVEL|THIS IS THE MESSAGE|THE_TAG")]
        [TestCase("{timestamp}|{loglevel}|{message}|{tag},2020-03-15 12:30:20:123|LOGLEVEL|THIS IS THE MESSAGE|THE_TAG",
                  "{timestamp}|{loglevel}|{message}|{tag},2020-03-15 12:30:20:123|LOGLEVEL|THIS IS THE MESSAGE|THE_TAG")]
        [TestCase("{timestamp} -- {message},2020-03-15 12:30:20:123 -- THIS IS THE MESSAGE",
                  "{timestamp} -- {message},2020-03-15 12:30:20:123 -- THIS IS THE MESSAGE")]
        [TestCase("{timestamp}[TID {threadid}][{loglevel}]{message}[{tag}],2020-03-15 12:30:20:123[TID     1234][LOGLEVEL]THIS IS THE MESSAGE[THE_TAG]",
                  "{timestamp}[TID {threadid}][{loglevel}]{message}[{tag}],2020-03-15 12:30:20:123[TID     1234][LOGLEVEL]THIS IS THE MESSAGE[THE_TAG]")]
        public void TestLogLayoutToLogIndices(string logLayout, string expectedOutput)
        {
            
            string layoutWithIndices = LoggerProExtensions.LogLayoutByPlaceHoldersToLogLayoutByIndexes(logLayout);

            
            DateTime dt = new DateTime(2020, 3, 15, 12, 30, 20, 123);
            
            string formatted = string.Format(layoutWithIndices,
                dt.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                "    1234",       
                "LOGLEVEL",
                "THIS IS THE MESSAGE",
                "THE_TAG");

            Assert.AreEqual(expectedOutput, formatted);
        }
    }
}
