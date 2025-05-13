using System;
using System.IO;
using FirebirdSql.Data.FirebirdClient;

namespace CreateDatabaseSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "User=SYSDBA;" +
                                      "Password=masterkey;" +
                                      "Database=fbtests.fdb;" +
                                      "DataSource=localhost;" +
                                      "Port=3050;" +
                                      "Dialect=3;";
            try
            {
                if (!File.Exists("fbtests.fdb"))
                {
                    FbConnection.CreateDatabase(connectionString);
                    Console.WriteLine("Database fbtests.fdb created");
                }
                else
                {
                    Console.WriteLine("Database fbtests.fdb already exists. Re-attaching.");
                }

                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Re-attached database fbtests.fdb");

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new FbCommand("CREATE TABLE dates_table (d1 DATE)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        Console.WriteLine("Table dates_table created");
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new FbCommand("INSERT INTO dates_table VALUES (CURRENT_DATE)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        Console.WriteLine("Record inserted into dates_table");
                    }

                    connection.Close();
                }
            }
            catch (FbException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
