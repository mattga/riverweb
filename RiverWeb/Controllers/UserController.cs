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

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && u != null && u.Username != "")
            {
                u.UserId = id;

                if (u.ReadUser(connection))
                {
                    u.Status.Code = StatusCode.OK;
                    u.Status.Description = DataUtils.OK;
                }
                else
                {
                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "Incorrect username or password";
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/id/authenticate
        [HttpPost]
        public User Authenticate(string id, User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "SELECT * FROM Users LEFT OUTER JOIN Hosts ON Users.UserId=Hosts.UserId WHERE Email=\"" + user.Email + "\" AND Password=\"" + user.Password + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        u.UserId = DataUtils.getInt32(reader, "UserId");
                        u.Username = DataUtils.getString(reader, "Username");
                        u.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                        u.Email = DataUtils.getString(reader, "Email");
                        u.City = DataUtils.getString(reader, "City");
                        u.State = DataUtils.getString(reader, "State");
                        u.Country = DataUtils.getString(reader, "Country");
                        u.ImageUrl = DataUtils.getString(reader, "ImageUrl");
                        u.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                        u.Host = new Host();
                        u.Host.UserId = u.UserId;
                        u.Host.spUserName = DataUtils.getString(reader, "spUserName");
                        u.Host.spPassword = DataUtils.getString(reader, "spPassword");

                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = DataUtils.OK;
                    }
                }
                else
                {
                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "Incorrect username or password";
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/id/linksp
        [HttpPost]
        public BaseModel LinkSP(string id, Host host)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && host != null)
            {
                string query = "INSERT INTO Hosts (UserId, spUserName, spPassword) VALUES ('" + host.UserId + "','" + host.spUserName + "','" + host.spPassword +"')";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected > 0)
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = DataUtils.OK;
                }
                else
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "Couldn't link Spotify account.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/user
        [ActionName("DefaultAction")]
        public BaseModel Post(User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "SELECT UserId FROM Users WHERE Email=\"" + user.Email + "\" AND IsFaceBook=\"" + user.IsFaceBook + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (!reader.HasRows)
                {
                    int isFbInt = (user.IsFaceBook ? 1 : 0);
                    reader.Close();
                    query = "INSERT INTO Users (Username, Password, Email, ImageUrl, IsFaceBook) " +
                        "VALUES (\"" + user.Username + "\",\"" + user.Password + "\",\"" + user.Email + "\","
                            + (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ",\"" + isFbInt + "\")";
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = "Success creating user.";
                    }
                }
                else
                {
                    bm.Status.Code = StatusCode.AlreadyExists;
                    bm.Status.Description = "User already exists.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // PUT api/user
        [ActionName("DefaultAction")]
        public User Put(User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "";
                int isFbInt = (user.IsFaceBook ? 1 : 0);
                query = "SELECT UserId FROM Users WHERE Email=\"" + user.Email + "\" AND IsFaceBook=" + user.IsFaceBook;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    query = "UPDATE Users " +
                        "SET Password=\"" + user.Password + "\",Username=\"" + user.Username + "\",Email=\"" + user.Email +
                            "\",ImageUrl=\"" + user.ImageUrl + "\",IsFaceBook=" + user.IsFaceBook + " " +
                        "WHERE Email = \"" + user.Email + "\" AND Password = \"" + user.Password + "\"";
                    DataUtils.executeQuery(connection, query);
                    
                    query = "SELECT UserId " +
                        "FROM Users " +
                        "WHERE Email = \"" + user.Email + "\" AND Password = \"" + user.Password + "\"";
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    u.UserId = reader.GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                else
                {
                    query = " INSERT INTO Users (Username, Password, FirstName, LastName, ImageUrl, IsFaceBook) " +
                        "VALUES (\"" + user.Username + "\", \"" + user.Password + "\", \"" + user.Email + "\", " +
                        (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ", " + user.IsFaceBook + ")";
                    DataUtils.executeQuery(connection, query);

                    reader.Close();
                    u.UserId = ((MySqlDataReader)DataUtils.executeQuery(connection, "SELECT LAST_INSERT_ID()")).GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/5/follow
        [ActionName("DefaultAction")]
        [HttpPost]
        public BaseModel Follow(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "INSERT INTO Followers (FollowerId, FolloweeId) " +
                    "VALUES (" + id + "," + user.UserId + ")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected == 0)
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No user found.";
                }
                else
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Success creating user.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/user/5/unfollow
        [ActionName("DefaultAction")]
        [HttpPost]
        public BaseModel Unfollow(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "DELETE FROM Followers " +
                    "WHERE FollowerId = " + id + " AND FolloweeId = " + user.UserId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected == 0)
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No user found.";
                }
                else
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Success unfollowing user.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }
    }
}
