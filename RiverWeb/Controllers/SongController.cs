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
    public class SongController : ApiController
    {

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Play(string id, Room room)
        {
            BaseModel status = new BaseModel();
            HttpResponseMessage response = Request.CreateResponse<BaseModel>(HttpStatusCode.OK, status);

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null)
            {
                string query = "UPDATE Rooms SET ";
                int affectedRows = DataUtility.executeNonQuery(connection, query);

                if (affectedRows > 0)
                {
                    status.Status.Code = StatusCode.OK;
                    status.Status.Description = DataUtility.OK;
                }
                else
                {
                    status.Status.Code = StatusCode.Error;
                    status.Status.Description = "No song set to played";
                }
                connection.Close();
            }

            return response;
        }

        public BaseModel Delete(string id, Room room)
        {
            BaseModel status = new BaseModel();

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null)
            {
                string query = "CALL DeleteSongAndDistributeTokens(\"" + room.RoomName + "\",\"spotify:track:" + id + "\")";
                int affectedRows = DataUtility.executeNonQuery(connection, query);

                if (affectedRows > 0)
                {
                    status.Status.Code = StatusCode.OK;
                    status.Status.Description = DataUtility.OK;
                }
                else
                {
                    status.Status.Code = StatusCode.Error;
                    status.Status.Description = "No song deleted";
                }
                connection.Close();
            }

            return status;
        }
    }
}
