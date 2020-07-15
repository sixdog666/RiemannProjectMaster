using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;

namespace DetectCodeAndCurrent {
 //  public struct MySqlParameter {
 //      string ParameterString;
 //      string ParameterValue;
 //  }
    public class MysqlConnector {
        private static MysqlConnector mInstance = null;
        private static string gConnStr = "data source = 127.0.0.1;database=jixing_db;user id=root;password=riemann;pooling=false;charset=utf8";//"data source = 192.168.19.90; database=jixing_db;user id = root; password=rieman;pooling=false;charset=utf8";
       // private static string gConnStr = "data source = 192.168.19.100;database=jixing_db;user id=newuser1;password=riemann;pooling=false;charset=utf8;SslMode = None;";//"data source = 192.168.19.90; database=jixing_db;user id = root; password=rieman;pooling=false;charset=utf8";
        //private static string gConnStr = "data source = 192.168.19.90; database=jixing_db;user id = root; password=riemann;pooling=false;charset=utf8;SslMode = None;";
        public EventHandler Event_SQLError;
        public delegate void ThreadExceptionEventHandler(Exception ex);
        public ThreadExceptionEventHandler threadExceptionEventHandler;
        public MysqlConnector() {

        }

        public static MysqlConnector GetInstance() {

            if (mInstance == null) {
                mInstance = new MysqlConnector();
            }
            return mInstance;
       

        }
        public int ExecuteNonMySQL(string sqlstr, params MySqlParameter[] parameters) {

            MySqlConnection mysqlcon = new MySqlConnection(gConnStr);
            try {
                mysqlcon.Open();
                MySqlCommand mysqlcom = new MySqlCommand(sqlstr, mysqlcon);
                if (parameters != null) mysqlcom.Parameters.AddRange(parameters);
                int count = mysqlcom.ExecuteNonQuery();
                mysqlcom.Dispose();
                return count;
            }

            catch (MySqlException ex) {

                Console.WriteLine(ex.Message);
                threadExceptionEventHandler?.Invoke(ex);
                return -1;
            }
            finally {
                mysqlcon.Close();
            }
        }

        public DataTable GetMySqlRead(string sqlstr, params MySqlParameter[] parameters) {

            MySqlConnection mysqlcon = new MySqlConnection(gConnStr);
            try {
                mysqlcon.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                MySqlCommand mysqlcom = new MySqlCommand(sqlstr, mysqlcon);
                if (parameters != null) mysqlcom.Parameters.AddRange(parameters);
                MySqlDataAdapter mda = new MySqlDataAdapter(mysqlcom);
                DataTable dt = new DataTable();
                mda.Fill(dt);
                mysqlcon.Close();
                return dt;
            }

            catch (MySqlException ex) {
                Console.WriteLine(ex.Message);
                threadExceptionEventHandler?.Invoke(ex);
                return null;
            }
            finally {
                mysqlcon.Close();
            }
        }





    }
}
