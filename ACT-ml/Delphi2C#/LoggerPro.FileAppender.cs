
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LoggerPro.Appenders
{
   #region Base Class

   
   public abstract class LoggerProFileAppenderBase : LoggerProAppenderBase, ILogAppender
   {
      
      public const int DEFAULT_MAX_BACKUP_FILE_COUNT = 5;
      public const int DEFAULT_MAX_FILE_SIZE_KB = 1000;
      public const string DEFAULT_FILENAME_FORMAT = "{module}.{number}.{tag}.log";
      public const string DEFAULT_FILENAME_FORMAT_WITH_PID = "{module}.{number}.{pid}.{tag}.log";
      public const string DEFAULT_FILENAME_FORMAT_WITHOUT_TAG = "{module}.{number}.log";
      protected const int RETRY_DELAY = 200;
      protected const int RETRY_COUNT = 5;

      
      protected Encoding FileEncoding;
      protected int MaxBackupFileCount;
      protected int MaxFileSizeInKiloByte;
      protected string LogFileNameFormat;
      protected string LogsFolder;
      
      protected virtual void CheckLogFileNameFormat(string logFileNameFormat)
      {
         if (!logFileNameFormat.Contains("{number}") || !logFileNameFormat.Contains("{tag}"))
         {
            throw new LoggerProException(
               string.Format("Wrong FileFormat [{0}] - [HINT] A correct file format for {1} requires {{number}} and {{tag}} placeholders. A valid file format is : {2}",
                  logFileNameFormat, this.GetType().Name, DEFAULT_FILENAME_FORMAT));
         }
      }

      
      protected virtual string GetLogFileName(string tag, int fileNumber)
      {
         
         string moduleName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
         string fileFormat = LogFileNameFormat
            .Replace("{module}", moduleName)
            .Replace("{number}", fileNumber.ToString("D" + Math.Max(2, MaxBackupFileCount.ToString().Length)))
            .Replace("{tag}", tag)
            .Replace("{pid}", Environment.ProcessId.ToString("D8"));
         
         return Path.Combine(LogsFolder, fileFormat);
      }

      
      protected virtual StreamWriter CreateWriter(string fileName, int bufferSize = 32)
      {
         int retries = 0;
         while (true)
         {
            try
            {
               
               FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
               StreamWriter sw = new StreamWriter(fs, FileEncoding, bufferSize)
               {
                  AutoFlush = true
               };
               return sw;
            }
            catch (IOException)
            {
               if (++retries >= RETRY_COUNT)
                  throw;
               System.Threading.Thread.Sleep(RETRY_DELAY);
            }
         }
      }

      
      protected virtual void EmitStartRotateLogItem(StreamWriter writer)
      {
         WriteToStream(writer, "#[START LOG " + DateTime.Now.ToString("o") + "]");
      }

      
      protected virtual void EmitEndRotateLogItem(StreamWriter writer)
      {
         WriteToStream(writer, "#[ROTATE LOG " + DateTime.Now.ToString("o") + "]");
      }

      
      protected virtual void WriteToStream(StreamWriter writer, string value)
      {
         writer.WriteLine(value);
         writer.Flush();
      }

      
      protected virtual void RotateFile(string logTag, out string newFileName)
      {
         newFileName = GetLogFileName(logTag, 0);
         
         string oldest = GetLogFileName(logTag, MaxBackupFileCount - 1);
         if (File.Exists(oldest))
         {
            TryDeleteFile(oldest);
         }
         
         for (int i = MaxBackupFileCount - 1; i >= 1; i--)
         {
            string src = GetLogFileName(logTag, i);
            string dst = GetLogFileName(logTag, i + 1);
            if (File.Exists(src))
            {
               TryMoveFile(src, dst);
            }
         }
         
         string firstBackup = GetLogFileName(logTag, 1);
         TryMoveFile(newFileName, firstBackup);
      }

      protected void TryDeleteFile(string fileSrc)
      {
         int retries = 0;
         while (true)
         {
            try
            {
               File.Delete(fileSrc);
               if (!File.Exists(fileSrc))
                  break;
            }
            catch
            {
               if (++retries == RETRY_COUNT)
                  throw new LoggerProException($"Cannot delete file {fileSrc}");
               System.Threading.Thread.Sleep(100);
            }
         }
      }

      protected void TryMoveFile(string fileSrc, string fileDest)
      {
         int retries = 0;
         while (true)
         {
            try
            {
               File.Move(fileSrc, fileDest);
               break;
            }
            catch (IOException)
            {
               if (++retries == RETRY_COUNT)
                  throw new LoggerProException($"Cannot rename {fileSrc} to {fileDest}");
               System.Threading.Thread.Sleep(100);
            }
         }
      }

      
      protected virtual void InternalWriteLog(StreamWriter writer, TLogItem logItem)
      {
         WriteToStream(writer, FormatLog(logItem));
      }

      
      public LoggerProFileAppenderBase(int maxBackupFileCount = DEFAULT_MAX_BACKUP_FILE_COUNT,
                                       int maxFileSizeInKiloByte = DEFAULT_MAX_FILE_SIZE_KB,
                                       string logsFolder = "",
                                       string logFileNameFormat = DEFAULT_FILENAME_FORMAT,
                                       ILogItemRenderer logItemRenderer = null,
                                       Encoding encoding = null)
         : base(logItemRenderer)
      {
         LogsFolder = logsFolder;
         MaxBackupFileCount = Math.Max(1, maxBackupFileCount);
         MaxFileSizeInKiloByte = maxFileSizeInKiloByte;
         CheckLogFileNameFormat(logFileNameFormat);
         LogFileNameFormat = logFileNameFormat;
         FileEncoding = encoding ?? Encoding.Default;
      }

      public override void Setup()
      {
         base.Setup();
         
         if (string.IsNullOrEmpty(LogsFolder))
         {
#if WINDOWS
            LogsFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#else
            LogsFolder = Environment.CurrentDirectory;
#endif
         }
         if (!Directory.Exists(LogsFolder))
            Directory.CreateDirectory(LogsFolder);
      }
   }

   #endregion

   #region LoggerProFileAppender

   
   public class LoggerProFileAppender : LoggerProFileAppenderBase
   {
      
      private readonly Dictionary<string, StreamWriter> _writersDictionary = new Dictionary<string, StreamWriter>();

      private void AddWriter(string logTag, out StreamWriter writer, out string logFileName)
      {
         logFileName = GetLogFileName(logTag, 0);
         writer = CreateWriter(logFileName);
         _writersDictionary.Add(logTag, writer);
      }

      private void RotateLog(string logTag, StreamWriter writer)
      {
         string logFileName;
         EmitEndRotateLogItem(writer);
         _writersDictionary.Remove(logTag);
         RotateFile(logTag, out logFileName);
         AddWriter(logTag, out writer, out logFileName);
         EmitStartRotateLogItem(writer);
      }

      public override void Setup()
      {
         base.Setup();
         
      }

      public override void TearDown()
      {
         foreach (var writer in _writersDictionary.Values)
         {
            writer.Dispose();
         }
         _writersDictionary.Clear();
         base.TearDown();
      }

      public override void WriteLog(TLogItem logItem)
      {
         StreamWriter writer;
         string logFileName;
         if (!_writersDictionary.TryGetValue(logItem.LogTag, out writer))
         {
            AddWriter(logItem.LogTag, out writer, out logFileName);
         }

         InternalWriteLog(writer, logItem);

         if (((FileStream)writer.BaseStream).Length > MaxFileSizeInKiloByte * 1024)
         {
            RotateLog(logItem.LogTag, writer);
         }
      }
   }

   #endregion

   #region LoggerProSimpleFileAppender

   
   public class LoggerProSimpleFileAppender : LoggerProFileAppenderBase
   {
      private StreamWriter _fileWriter;

      protected override void CheckLogFileNameFormat(string logFileNameFormat)
      {
         
         if (!logFileNameFormat.Contains("{number}"))
         {
            throw new LoggerProException(
               string.Format("Wrong FileFormat [{0}] - [HINT] A correct file format for {1} requires the {{number}} placeholder. A valid file format is : {2}",
                  logFileNameFormat, this.GetType().Name, DEFAULT_FILENAME_FORMAT));
         }
      }

      public override void Setup()
      {
         base.Setup();
         _fileWriter = CreateWriter(GetLogFileName("", 0));
      }

      public override void TearDown()
      {
         _fileWriter?.Dispose();
         base.TearDown();
      }

      public override void WriteLog(TLogItem logItem)
      {
         InternalWriteLog(_fileWriter, logItem);
         if (((FileStream)_fileWriter.BaseStream).Length > MaxFileSizeInKiloByte * 1024)
         {
            RotateLog();
         }
      }

      private void RotateLog()
      {
         EmitEndRotateLogItem(_fileWriter);
         _fileWriter.Dispose();
         RotateFile("", out string newFileName);
         _fileWriter = CreateWriter(GetLogFileName("", 0));
         EmitStartRotateLogItem(_fileWriter);
      }

      public LoggerProSimpleFileAppender(
         int maxBackupFileCount = DEFAULT_MAX_BACKUP_FILE_COUNT,
         int maxFileSizeInKiloByte = DEFAULT_MAX_FILE_SIZE_KB,
         string logsFolder = "",
         string logFileNameFormat = DEFAULT_FILENAME_FORMAT,
         ILogItemRenderer logItemRenderer = null,
         Encoding encoding = null)
         : base(maxBackupFileCount, maxFileSizeInKiloByte, logsFolder, logFileNameFormat, logItemRenderer, encoding)
      {
      }
   }

   #endregion

   #region LoggerProFileByFolderAppender

   
   public class LoggerProFileByFolderAppender : LoggerProFileAppender
   {
      private StreamWriter _fileWriter;
      private DateTime _currentDate;

      private string GetLogFolder()
      {
         
         string baseFolder = string.IsNullOrEmpty(LogsFolder)
            ? Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            : LogsFolder;

         string logsDir = Path.Combine(baseFolder, "Logs");
         if (!Directory.Exists(logsDir))
            Directory.CreateDirectory(logsDir);

         
         string dayFolder = Path.Combine(logsDir, DateTime.Now.ToString("yyyyMMdd"));
         if (!Directory.Exists(dayFolder))
            Directory.CreateDirectory(dayFolder);

         return dayFolder;
      }

      
      protected override string GetLogFileName(string tag, int fileNumber)
      {
         string moduleName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
         string fileName = string.Format("{0}.{1}.log", moduleName, fileNumber.ToString("D2"));
         return Path.Combine(GetLogFolder(), fileName);
      }

      protected override void CheckLogFileNameFormat(string logFileNameFormat)
      {
         // Do nothing, file format is fixed.
      }

      private void ChangeLogFolder()
      {
         InternalRotateLog(out string newFileName, () =>
         {
            newFileName = GetLogFileName("", 0);
         });
      }

      private void RotateLog()
      {
         InternalRotateLog(out string newFileName, () =>
         {
            RotateFile("", out newFileName);
         });
      }

      private void InternalRotateLog(out string newFileName, Action<TLoggerProFileByFolderAppender> makeFileNameProc = null)
      {
         EmitEndRotateLogItem(_fileWriter);
         _fileWriter.Dispose();
         if (makeFileNameProc != null)
         {
            
            makeFileNameProc(this);
         }
         else
         {
            newFileName = GetLogFileName("", 0);
         }
         _fileWriter = CreateWriter(newFileName, 16 * 1024);
         EmitStartRotateLogItem(_fileWriter);
      }

      public override void Setup()
      {
         base.Setup();
         _fileWriter = CreateWriter(GetLogFileName("", 0));
         _currentDate = DateTime.Today;
      }

      public override void TearDown()
      {
         _fileWriter?.Dispose();
         base.TearDown();
      }

      public override void WriteLog(TLogItem logItem)
      {
         if (_currentDate != DateTime.Today)
         {
            ChangeLogFolder();
            _currentDate = DateTime.Today;
         }
         string logRow;
         if (OnLogRow != null)
         {
            OnLogRow(logItem, out logRow);
         }
         else
         {
            logRow = LogItemRenderer.RenderLogItem(logItem);
         }
         WriteToStream(_fileWriter, logRow);
         if (((FileStream)_fileWriter.BaseStream).Length > MaxFileSizeInKiloByte * 1024)
         {
            RotateLog();
         }
      }

      public LoggerProFileByFolderAppender(
         int maxBackupFileCount = DEFAULT_MAX_BACKUP_FILE_COUNT,
         int maxFileSizeInKiloByte = DEFAULT_MAX_FILE_SIZE_KB,
         string logsFolder = "",
         ILogItemRenderer logItemRenderer = null,
         Encoding encoding = null)
         : base(maxBackupFileCount, maxFileSizeInKiloByte, logsFolder, DEFAULT_FILENAME_FORMAT_WITHOUT_TAG, logItemRenderer, encoding)
      {
      }
   }

   #endregion

   #region LoggerProLogFmtFileAppender

   
   public class LoggerProLogFmtFileAppender : LoggerProSimpleFileAppender
   {
      public LoggerProLogFmtFileAppender(
         int maxBackupFileCount = DEFAULT_MAX_BACKUP_FILE_COUNT,
         int maxFileSizeInKiloByte = DEFAULT_MAX_FILE_SIZE_KB,
         string logsFolder = "",
         string logFileNameFormat = DEFAULT_FILENAME_FORMAT,
         Encoding encoding = null)
         : base(maxBackupFileCount, maxFileSizeInKiloByte, logsFolder, logFileNameFormat, new LogItemRendererLogFmt(), encoding)
      {
      }

      protected override void EmitEndRotateLogItem(StreamWriter writer)
      {
         // In this appender, do nothing
      }

      protected override void EmitStartRotateLogItem(StreamWriter writer)
      {
         // In this appender, do nothing
      }

      protected override string GetLogFileName(string tag, int fileNumber)
      {
        
         string origFileName = base.GetLogFileName(tag, fileNumber);
         string ext = Path.GetExtension(origFileName);
         if (string.IsNullOrEmpty(ext))
         {
            ext = ".log";
         }
         return Path.ChangeExtension(origFileName, ".logfmt" + ext);
      }
   }

   #endregion
}
