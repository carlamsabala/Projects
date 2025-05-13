
using System;
using System.Data;

namespace LoggerPro.Appenders
{
    
        public delegate void OnDBWriteError(object sender, TLogItem logItem, Exception dbError, ref int retryCount);

    
    public delegate IDbConnection GetDBConnection();

   
    public delegate T GetStoredProc<T>(IDbConnection connection) where T : class;

    
    public delegate void SetParams<T>(T dataObject, TLogItem logItem) where T : class;

    
    public abstract class LoggerProDBAppender<T> : LoggerProAppenderBase where T : class
    {
        protected const int MAX_RETRY_COUNT = 5;

        protected OnDBWriteError OnDBWriteErrorCallback;
        protected GetDBConnection GetDBConnectionCallback;
        protected GetStoredProc<T> GetStoredProcCallback;
        protected SetParams<T> SetParamsCallback;

        protected IDbConnection DbConnection;
        protected T DbObject;

       
        public LoggerProDBAppender(GetDBConnection getDBConnection, GetStoredProc<T> getStoredProc, SetParams<T> setParams,
                                   OnDBWriteError onDBWriteError)
        {
            GetDBConnectionCallback = getDBConnection ?? throw new ArgumentNullException(nameof(getDBConnection));
            GetStoredProcCallback = getStoredProc ?? throw new ArgumentNullException(nameof(getStoredProc));
            SetParamsCallback = setParams ?? throw new ArgumentNullException(nameof(setParams));
            OnDBWriteErrorCallback = onDBWriteError; 
        }

                protected abstract void RefreshParams(T dataObject);

        
        protected abstract void ExecuteDataObject(T dataObject);

        
        public override void Setup()
        {
            base.Setup();
            DbConnection = GetDBConnectionCallback();
        }

        
        public override void TearDown()
        {
            base.TearDown();
            if (DbObject != null)
            {
                if (DbObject is IDisposable disposableObj)
                {
                    disposableObj.Dispose();
                }
                DbObject = null;
            }
            if (DbConnection != null)
            {
                try
                {
                    DbConnection.Close();
                }
                finally
                {
                    DbConnection.Dispose();
                    DbConnection = null;
                }
            }
        }

        
        public override void TryToRestart(out bool restarted)
        {
            restarted = false;
            try
            {
                if (DbObject != null)
                {
                    if (DbObject is IDisposable disposableObj)
                    {
                        disposableObj.Dispose();
                    }
                    DbObject = null;
                }
                if (DbConnection != null)
                {
                    DbConnection.Close();
                    DbConnection.Dispose();
                    DbConnection = null;
                }
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
            DbConnection = GetDBConnectionCallback();
            restarted = true;
        }

                public override void WriteLog(TLogItem logItem)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (DbObject == null)
                    {
                        
                        DbConnection.Open();
                        DbObject = GetStoredProcCallback(DbConnection);
                        RefreshParams(DbObject);
                    }
                    SetParamsCallback(DbObject, logItem);
                    ExecuteDataObject(DbObject);
                    break;
                }
                catch (Exception ex)
                {
                    if (OnDBWriteErrorCallback != null)
                    {
                        OnDBWriteErrorCallback(this, logItem, ex, ref retryCount);
                    }
                    retryCount++;
                    if (retryCount >= MAX_RETRY_COUNT)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
