using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using log4net;
using log4net.Config;

namespace MySQLClass
{

    //Don't forget to add the MySQL.Data dll to your projects references
    //It can be downloaded for free from MySQL's official website.
    //Link to the .NET Connector (.dll file) http://dev.mysql.com/downloads/connector/net/


    class MySQLClient
    {
        private static readonly ILog logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MySqlConnection conn = null;


        #region Constructors
        public MySQLClient(string hostname, string database, string username, string password)
        {
            conn = new MySqlConnection("server=" + hostname + ";database=" + database + ";uid=" + username + ";Pwd=" + password+ ";");

        }

        public MySQLClient(string hostname, string database, string username, string password, int portNumber)
        {
            conn = new MySqlConnection("host=" + hostname + ";database=" + database + ";username=" + username + ";password=" + password + ";port=" + portNumber.ToString() + ";");
        }

        public MySQLClient(string hostname, string database, string username, string password, int portNumber, int connectionTimeout)
        {
            conn = new MySqlConnection("host=" + hostname + ";database=" + database + ";username=" + username + ";password=" + password + ";port=" + portNumber.ToString() + ";Connection Timeout=" + connectionTimeout.ToString() + ";");
        }
        #endregion

        #region Open/Close Connection
        private bool Open()
        {
            //This opens temporary connection
            try
            {
                conn.Open();
                return true;
            }
            catch
            {
                //Here you could add a message box or something like that so you know if there were an error.
                return false;
            }
        }

        private bool Close()
        {
            //This method closes the open connection
            try
            {
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Preprocess database
        public bool DatabaseExists()
        {
            string query = "SHOW TABLES LIKE 'BancoCircuitoMaxTemp';";

            DataTable dataTable = new DataTable();

            if (this.Open())
            {
                try
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.Fill(dataTable);
                }
                catch { }
                this.Close();

                return (dataTable.Rows.Count > 0);
            }
            else
            {
                logger.Error("Checar se Base existe - Erro ao acessar a base.");
                return false;
            }
        }

        public bool CreateDatabase()
        {
            string query = "CREATE TABLE BancoCircuitoMaxTemp (BancoNumero INT NOT NULL," +
                                    "TemperaturaMaxCircuito FLOAT(5,2) NOT NULL," +
                                    "primary key (BancoNumero));";

            try
            {
                if (this.Open())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.ExecuteNonQuery();
                    this.Close();
                    return true;
                }
            }
            catch {
                logger.Error("Criando base - Erro ao criar a base de dados.");
                return false;
            }
            return false;
        }

        public bool FillUpDatabase()
        {
            string query = "";

            try
            {
                if (this.Open())
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        query = "INSERT INTO BancoCircuitoMaxTemp (BancoNumero, TemperaturaMaxCircuito)" +
                            "VALUES ('" + i + "', '0.0');";

                        MySqlCommand cmd = new MySqlCommand(query, conn);

                        cmd.ExecuteNonQuery();
                    }
                    this.Close();
                    return true;
                }
            }
            catch
            {
                logger.Error("Preenchendo base pela primeira vez - Erro");
                return false;
            }

            return false;
        }

        #endregion

        public void Insert(string table, string column, string value)
        {
            //Insert values into the database.

            //Example: INSERT INTO names (name, age) VALUES('John Smith', '33')
            //Code: MySQLClient.Insert("names", "name, age", "'John Smith, '33'");
            string query = "INSERT INTO " + table + " (" + column + ") VALUES (" + value + ")";

            try
            {
                if (this.Open())
                {
                    //Opens a connection, if succefull; run the query and then close the connection.

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.ExecuteNonQuery();
                    this.Close();
                }
            }
            catch { }
            return;
        }

        public void Update(string table, string SET, string WHERE)
        {
            //Update existing values in the database.

            //Example: UPDATE names SET name='Joe', age='22' WHERE name='John Smith'
            //Code: MySQLClient.Update("names", "name='Joe', age='22'", "name='John Smith'");
            string query = "UPDATE " + table + " SET " + SET + " WHERE " + WHERE + "";

            if (this.Open())
            {
                try
                {
                    //Opens a connection, if succefull; run the query and then close the connection.

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    this.Close();
                }
                catch { this.Close(); }
            }
            return;
        }

        public void Delete(string table, string WHERE)
        {
            //Removes an entry from the database.

            //Example: DELETE FROM names WHERE name='John Smith'
            //Code: MySQLClient.Delete("names", "name='John Smith'");
            string query = "DELETE FROM " + table + " WHERE " + WHERE + "";

            if (this.Open())
            {
                try
                {
                    //Opens a connection, if succefull; run the query and then close the connection.

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    this.Close();
                }
                catch { this.Close(); }
            }
            return;
        }

        public DataTable Select(string table, string WHERE)
        {
            //This methods selects from the database, it retrieves data from it.
            //You must make a dictionary to use this since it both saves the column
            //and the value. i.e. "age" and "33" so you can easily search for values.

            //Example: SELECT * FROM names WHERE name='John Smith'
            // This example would retrieve all data about the entry with the name "John Smith"

            //Code = Dictionary<string, string> myDictionary = Select("names", "name='John Smith'");
            //This code creates a dictionary and fills it with info from the database.

            string query = "SELECT * FROM " + table + " WHERE " + WHERE + ";";

            DataTable dataTable = new DataTable();

            if (this.Open())
            {
                try
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.Fill(dataTable);
                }
                catch { }
                this.Close();

                return dataTable;
            }
            else
            {
                return dataTable;
            }
        }

        public int Count(string table)
        {
            //This counts the numbers of entries in a table and returns it as an integear

            //Example: SELECT Count(*) FROM names
            //Code: int myInt = MySQLClient.Count("names");

            string query = "SELECT Count(*) FROM " + table + "";
            int Count = -1;
            if (this.Open() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    Count = int.Parse(cmd.ExecuteScalar() + "");
                    this.Close();
                }
                catch { this.Close(); }
                return Count;
            }
            else
            {
                return Count;
            }
        }
    }
}
