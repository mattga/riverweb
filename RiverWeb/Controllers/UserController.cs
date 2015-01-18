using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using RiverWeb.Models;
using RiverWeb.Utils;

namespace RiverWeb.Controllers
{
    public class UserController : ApiController
    {

        // GET api/user/5
        [ActionName("DefaultAction")]
        public User Get(int id)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null && u != null && u.Username != "")
            {
                u.UserId = id;

                if (u.ReadUser(connection))
                {
                    u.Status.Code = StatusCode.OK;
                    u.Status.Description = DataUtility.OK;
                }
                else
                {
                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "Incorrect username or password";
                }
                DataUtility.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/id/authenticate
        [HttpPost]
        public User Authenticate(string id, User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            try
            {
                if (connection != null && user != null && user.Username != "")
                {
                    string query = "SELECT * FROM Users WHERE Email=\"" + user.Email + "\" AND Password=\"" + user.Password + "\"";
                    MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            u.UserId = DataUtility.getInt32(reader, "UserId");
                            u.Username = DataUtility.getString(reader, "Username");
                            u.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                            u.Email = DataUtility.getString(reader, "Email");
                            u.City = DataUtility.getString(reader, "City");
                            u.State = DataUtility.getString(reader, "State");
                            u.Country = DataUtility.getString(reader, "Country");
                            u.ImageUrl = DataUtility.getString(reader, "ImageUrl");
                            u.spUsername = DataUtility.getString(reader, "spUsername");

                            u.Status.Code = StatusCode.OK;
                            u.Status.Description = DataUtility.OK;
                        }
                    }
                    else
                    {
                        u.Status.Code = StatusCode.NotFound;
                        u.Status.Description = "Incorrect username or password";
                    }
                    DataUtility.closeConnection(connection);
                }
            }
            catch (Exception ex)
            {
                u.Status.Description = ex.StackTrace;
            }

            return u;
        }

        // POST api/user
        [ActionName("DefaultAction")]
        public User Post(User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "SELECT UserId FROM Users WHERE Email=\"" + user.Email + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                if (!reader.HasRows)
                {
                    query = "INSERT INTO Users (Username, Password, Email, ImageUrl) " +
                        "VALUES (\"" + user.Username + "\",\"" + user.Password + "\",\"" + user.Email + "\","
                            + (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ")";
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        query = "SELECT LAST_INSERT_ID()";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                        if (reader.Read())
                        {
                            u.UserId = reader.GetInt32(0);
                        }

                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success creating user.";
                    }
                }
                else
                {
                    u.Status.Code = StatusCode.AlreadyExists;
                    u.Status.Description = "User already exists.";
                }
                DataUtility.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/id/linkspotify
        [HttpPost]
        public User LinkSpotify(string id, User user) {
            User u = new User();
            u.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null &&user != null)
            {
                string query = "SELECT * FROM Users WHERE spEmail='" + user.spEmail + "'";
                MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                if (reader.Read())
                {
                    u.UserId = DataUtility.getInt32(reader, "UserId");
                    u.Username = DataUtility.getString(reader, "Username");
                    u.Email = DataUtility.getString(reader, "Email");

                    u.Status.Code = StatusCode.AlreadyExists;
                    u.Status.Description = "Spotify account already linked to a user.";
                }
                else if (id == "")
                {
                    query = "INSERT INTO Users (spUsername, spCanonicalUsername, spEmail) VALUES ('" + user.spUsername +
                            "','" + user.spCanonicalUsername + "','" + user.spEmail + "')";
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        query = "SELECT LAST_INSERT_ID()";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                        if (reader.Read())
                        {
                            u.UserId = reader.GetInt32(0);
                        }
                    }

                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "New account created with spotidy information.";
                }
                else
                {
                    query = "UPDATE Users SET spUsername='" + user.spUsername + 
                        "', spCanonicalUsername='" + user.spCanonicalUsername + 
                        "', spEmail='" + user.spEmail + "'" + 
                        "WHERE UserId=" + id;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    query = "SELECT * FROM Users " +
                        "WHERE UserId=" + id;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    u.UserId = Convert.ToInt32(id);
                    u.Username = DataUtility.getString(reader, "Username");
                    u.Email = DataUtility.getString(reader, "Email");

                    u.Status.Code = StatusCode.OK;
                    u.Status.Description = "Spotify information linked to account.";
                }

                DataUtility.closeConnection(connection);
            }

            return u;
        }

        // PUT api/user
        [ActionName("DefaultAction")]
        public User Put(User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "";
                query = "SELECT UserId FROM Users WHERE Email=\"" + user.Email + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                if (reader.Read())
                {
                    query = "UPDATE Users " +
                        "SET Password=\"" + user.Password + "\",Username=\"" + user.Username + "\",Email=\"" + user.Email +
                            "\",ImageUrl=\"" + user.ImageUrl + "\" " +
                        "WHERE Email = \"" + user.Email + "\" AND Password = \"" + user.Password + "\"";
                    DataUtility.executeQuery(connection, query);
                    
                    query = "SELECT UserId " +
                        "FROM Users " +
                        "WHERE Email = \"" + user.Email + "\" AND Password = \"" + user.Password + "\"";
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    u.UserId = reader.GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                else
                {
                    query = " INSERT INTO Users (Username, Password, FirstName, LastName, ImageUrl) " +
                        "VALUES (\"" + user.Username + "\", \"" + user.Password + "\", \"" + user.Email + "\", " +
                        (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ")";
                    DataUtility.executeQuery(connection, query);

                    reader.Close();
                    u.UserId = ((MySqlDataReader)DataUtility.executeQuery(connection, "SELECT LAST_INSERT_ID()")).GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                DataUtility.closeConnection(connection);
            }

            return u;
        }
    }
}
