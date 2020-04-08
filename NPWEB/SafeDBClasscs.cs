using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace NPWEB
{
    /// <summary>
    /// 安全风险管控项目数据库连接 mysql
    /// </summary>
    public class SafeDBClasscs
    {
        private string constr = "";
        private MySqlConnection mysqlcon = null;
        private MySqlCommand mysqlcom = null;
        public int getconMysql()
        {
            int result_open = 0;
            string url = "User ID=root;Password=gtzdh@8220;Host=39.98.233.216;Port=3306;Database=safemanager;Allow User Variables=True;Charset=utf8";

            mysqlcon = new MySqlConnection(url);
            //创建需要的Sql语句
            mysqlcon.Open();
            if (mysqlcon.State == ConnectionState.Open)
            {
                result_open = 1;
            }
            return result_open;
        }
        public DataTable GetDataTable(string sqlstr)
        {
            DataTable mydt = null;
            if (getconMysql() == 1)
            {
                MySqlDataAdapter myda = new MySqlDataAdapter(sqlstr, mysqlcon);
                // DataSet myds = new DataSet();
                mydt = new DataTable();
                myda.Fill(mydt);
                mysqlcon.Close();
            }
            return mydt;
        }
        public int getsqlCom(string sqlstr)
        {
            int result_com = 0;
            try
            {
                if (getconMysql() == 1)
                {
                    mysqlcom = new MySqlCommand(sqlstr, mysqlcon);
                    result_com = mysqlcom.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                mysqlcon.Close();
            }
            return result_com;
        }
    }
}