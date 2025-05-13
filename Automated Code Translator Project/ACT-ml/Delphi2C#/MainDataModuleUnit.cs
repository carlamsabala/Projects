using System;
using System.Data;
using System.IO;
using FirebirdSql.Data.FirebirdClient;
using WinesBO; 

namespace MyApp.Data
{
    
    public class WineCellarDataModule : IDisposable
    {
        
        private FbConnection _connection;

        
        public WineCellarDataModule()
        {
            
            _connection = new FbConnection();
            _connection.ConnectionString = ""; 
            ConnectionBeforeConnect();
        }

       
        private void ConnectionBeforeConnect()
        {
            string envDbPath = Environment.GetEnvironmentVariable("database.path");
            string dbPath;
            if (!string.IsNullOrEmpty(envDbPath))
            {
                dbPath = envDbPath;
            }
            else
            {
                
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                dbPath = Path.Combine(exePath, @"..\..\WINES_FB30.FDB");
            }

            
            _connection.ConnectionString =
                $"User=SYSDBA;Password=masterkey;Database={dbPath};DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Pooling=true;";
        }

        
        public DataSet GetAllWines()
        {
            return FindWines("");
        }

        
        public DataSet GetWineById(int id)
        {
            string sql = "SELECT * FROM wine WHERE id = @id";
            return ExecuteQuery(sql, new FbParameter("@id", id));
        }

        
        public DataSet FindWines(string search)
        {
            string sql;
            FbParameter param = null;
            if (string.IsNullOrWhiteSpace(search))
            {
                sql = "SELECT * FROM wine";
            }
            else
            {
                sql = "SELECT * FROM wine WHERE NAME CONTAINING @search";
                param = new FbParameter("@search", search);
            }
            return ExecuteQuery(sql, param);
        }

        
        public void AddWine(TWine wine)
        {
            
            string sql = "INSERT INTO wine (NAME, YEAR, DESCRIPTION) VALUES (@NEW_Name, @NEW_Year, @NEW_Description)";
            using (FbCommand cmd = new FbCommand(sql, _connection))
            {
                
                cmd.Parameters.AddWithValue("@NEW_Name", wine.Name);
                cmd.Parameters.AddWithValue("@NEW_Year", wine.Year);
                cmd.Parameters.AddWithValue("@NEW_Description", wine.Description);
                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }
        }

        
        public void UpdateWine(TWine wine)
        {
            
            string sql = "UPDATE wine SET NAME = @NEW_Name, YEAR = @NEW_Year, DESCRIPTION = @NEW_Description WHERE id = @OLD_ID";
            using (FbCommand cmd = new FbCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@NEW_Name", wine.Name);
                cmd.Parameters.AddWithValue("@NEW_Year", wine.Year);
                cmd.Parameters.AddWithValue("@NEW_Description", wine.Description);
                cmd.Parameters.AddWithValue("@OLD_ID", wine.Id);
                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }
        }

        
        public void DeleteWine(int id)
        {
            string sql = "DELETE FROM wine WHERE id = @OLD_ID";
            using (FbCommand cmd = new FbCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@OLD_ID", id);
                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }
        }

        
        private DataSet ExecuteQuery(string sql, params FbParameter[] parameters)
        {
            DataSet ds = new DataSet();
            using (FbDataAdapter adapter = new FbDataAdapter(sql, _connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                }
                _connection.Open();
                adapter.Fill(ds);
                _connection.Close();
            }
            return ds;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
