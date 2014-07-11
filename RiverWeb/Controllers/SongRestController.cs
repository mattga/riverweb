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
    public class SongRestController : ApiController
    {

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Play(string id, Room room)
        {
            BaseModel status = new BaseModel();
            HttpResponseMessage response = Request.CreateResponse<BaseModel>(HttpStatusCode.OK, status);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL SetSongPlaying(\"" + room.RoomName + "\",\"" + id + "\")";
                int affectedRows = DataUtils.executeNonQuery(connection, query);

                if (affectedRows > 0)
                {
                    status.Status.Code = StatusCode.OK;
                    status.Status.Description = DataUtils.OK;
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

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL DeleteSongAndDistributeTokens(\"" + room.RoomName + "\",\"" + id + "\")";
                int affectedRows = DataUtils.executeNonQuery(connection, query);

                if (affectedRows > 0)
                {
                    status.Status.Code = StatusCode.OK;
                    status.Status.Description = DataUtils.OK;
                }
                else
                {
                    status.Status.Code = StatusCode.Error;
                    status.Status.Description = "No song set to played";
                }
                connection.Close();
            }

            return status;
        }
    }
}
