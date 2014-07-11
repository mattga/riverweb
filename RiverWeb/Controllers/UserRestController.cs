using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using RiverWeb.Models;
using RiverWeb.Utils;
using MySql.Data.MySqlClient;

namespace RiverWeb.Controllers
{
    public class UserRestController : ApiController
    {
        public IEnumerable<User> Get()
        {
            return null;
        }

        public String Get(string id)
        {
            return id;
        }
        
        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post(User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse<User>(HttpStatusCode.OK, u);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "CALL CreateUserIfNotExist(\"" + user.Username + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) == 0)
                    {
                        u.Username = user.Username;
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        u.Status.Code = StatusCode.AlreadyExists;
                        u.Status.Description = "User already exists";
                    }
                }
                connection.Close();
            }

            return response;
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(string id, User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse<User>(HttpStatusCode.OK, u);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null)
            {
                string query = "CALL UpdateUserIfNotExist(\"" + id + "\",\"" + user.Username + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        u.Username = user.Username;
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        u.Status.Code = StatusCode.AlreadyExists;
                        u.Status.Description = "User already exists";
                    }
                }
                connection.Close();
            }

            return response;
        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage Delete(User user)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ModelStatus
                {
                    Code = StatusCode.Error
                });
        }
    }
}
