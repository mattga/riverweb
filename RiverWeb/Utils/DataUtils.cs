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

        static public string getString(IDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            if (reader.IsDBNull(index))
            {
                return null;
            }

            return reader.GetString(index);
        }

        static public int getInt32(IDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            if (reader.IsDBNull(index))
            {
                return 0;
            }

            return reader.GetInt32(index);
        }

        static public double getDouble(IDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            if (reader.IsDBNull(index))
            {
                return 0.00;
            }

            return reader.GetDouble(index);
        }

        static DateTime? getDateTime(IDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            if (reader.IsDBNull(index))
            {
                return null;
            }

            return reader.GetDateTime(index);
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
