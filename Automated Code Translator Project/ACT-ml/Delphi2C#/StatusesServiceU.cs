using System;
using System.Data;
using MVCFramework.Commons;
using MainDMU;
using EntitiesU;

namespace YourNamespace
{
    public class StatusService : BaseService, IDisposable
    {
        private dmMain _module;

        public StatusService() : base()
        {
            _module = new dmMain();
        }

        public void PersistStatus(NotificationEntity aStatus)
        {
            _module.qryInsertNotification.ExecSQL("", new object[] { aStatus.Value });
            CurrentStatusEntity.GetInstance(() => GetLastPersistedStatus()).SetStatus(GetLastPersistedStatus());
        }

        public CurrentStatusEntity GetCurrentStatus()
        {
            return CurrentStatusEntity.GetInstance(() => GetLastPersistedStatus());
        }

        public FullStatusEntity GetLastPersistedStatus()
        {
            DataSet ds;
            _module.Connection.ExecSQL("select id, value, created_at from notifications order by id desc limit 1", out ds);
            FullStatusEntity result = new FullStatusEntity();
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                result.Id = -1;
                result.Value = "";
                result.PushedAt = "";
            }
            else
            {
                DataRow row = ds.Tables[0].Rows[0];
                result.Id = Convert.ToInt32(row["id"]);
                result.Value = row["value"].ToString();
                result.PushedAt = row["created_at"].ToString();
            }
            ds.Dispose();
            return result;
        }

        public void Dispose()
        {
            if (_module != null)
            {
                _module.Dispose();
                _module = null;
            }
        }
    }
}
