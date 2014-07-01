using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Web;
using RiverWeb.Models;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace RiverWeb.Utils
{
    public class DataUtils
    {
        public static String OK = "OK";

        public static void captureException(Exception ex, ModelStatus status)
        {
            status.Code = StatusCode.Error;
            status.StackTrace = ex.StackTrace;
            status.Description = ex.Message;
            if (ex.InnerException != null && ex.InnerException.Message != null)
            {
                status.Description += "\r\n" + ex.InnerException;
            }
        }

        static public MySqlConnection getConnection()
        {
            MySqlConnection connection = new MySqlConnection();

            try
            {
                connection.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                connection.Open();
            }
            catch
            {
                connection = null;
            }

            return connection;
        }

        static public void closeConnection(MySqlConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        static public IDataReader executeQuery(MySqlConnection connection, String sql)
        {
            IDataReader reader = null;

            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand();

                try
                {
                    command.CommandText = sql;
                    command.Connection = connection;

                    reader = command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    command = null;

                }
            }

            return reader;
        }

        static public int executeNonQuery(MySqlConnection connection, String sql)
        {
            int affectedRows = 0;

            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand();

                try
                {

                    command.CommandText = sql;
                    command.Connection = connection;

                    affectedRows = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    command = null;
                }
            }

            return affectedRows;
        }

        static public void executeNonQueryParams(MySqlConnection connection, String sql, MySqlParameter[] parms)
        {
            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand();

                try
                {
                    command.CommandText = sql;
                    command.Connection = connection;
                    foreach (MySqlParameter parm in parms)
                    {
                        command.Parameters.Add(parm);
                    }
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    command = null;
                }
            }
        }

        static public String getString(IDataReader reader, String columnName)
        {
            String data = "";

            Object value = reader.GetValue(reader.GetOrdinal(columnName));
            if (value is DBNull)
            {
                //do nothing
            }
            else if (value is Guid)
            {
                Guid temp = (Guid)value;
                data = "{" + temp.ToString() + "}";
            }
            else if (value is String)
            {
                data = (String)value;
            }
            else if (value is Int32)
            {
                Int32 temp = (Int32)value;
                data = temp.ToString();
            }
            else if (value is Boolean)
            {
                Boolean temp = (Boolean)value;
                data = temp.ToString();
            }
            else if (value is DateTime)
            {
                DateTime temp = (DateTime)value;
                data = temp.ToShortDateString();
            }

            return data;
        }

        static public Boolean getBoolean(IDataReader reader, String columnName)
        {
            Boolean value = new Boolean();

            int columnId = reader.GetOrdinal(columnName);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    value = (Boolean)o;
                }
            }

            return value;
        }

        static public Decimal getDecimal(IDataReader reader, String column)
        {
            Decimal value = new Decimal();

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    value = (Decimal)o;
                }
            }

            return value;
        }

        static public int getInt(IDataReader reader, String column)
        {
            int value = 0;

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    value = (Int32)o;
                }
            }

            return value;
        }

        static public Boolean hasValue(IDataReader reader, string column)
        {
            Boolean isNotNull = false;

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    isNotNull = true;
                }
            }

            return isNotNull;
        }

        static public DateTime getDateTime(IDataReader reader, String column)
        {
            DateTime value = DateTime.MinValue;

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    value = (DateTime)o;
                }
            }

            return value;
        }

        static public Object getData(IDataReader reader, String column)
        {
            Object value = null;

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                value = reader.GetValue(columnId);
            }

            return value;
        }

        static public Guid getGuid(IDataReader reader, String column)
        {
            Guid value = Guid.Empty;

            int columnId = reader.GetOrdinal(column);
            if (columnId >= 0)
            {
                Object o = reader.GetValue(columnId);
                if (o != DBNull.Value)
                {
                    value = (Guid)o;
                }
            }

            return value;
        }

        public static double unixTicks(DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static string formatDate(DateTime date)
        {
            string formattedDate = "";
            if (date > DateTime.MinValue && date < DateTime.MaxValue)
            {
                formattedDate = date.ToString("MM/dd/yyyy");
            }
            return formattedDate;
        }

        public static object DBDateValue(DateTime date)
        {
            object dbDateValue = DBNull.Value;
            if (date > DateTime.MinValue && date < DateTime.MaxValue)
            {
                dbDateValue = date;
            }
            return dbDateValue;
        }

        public static String safeString(String textString)
        {
            String processedString = textString;
            if (textString != null)
            {
                processedString = textString.Replace("'", "''");
            }
            return processedString;
        }

    }
}
