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
    public class RoomRestController : ApiController
    {

        public IEnumerable<Room> Get()
        {
            List<Room> rs = new List<Room>();

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL GetAllRooms()";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                while (reader.Read())
                {
                    Room r = new Room();
                    r.RoomName = reader.GetString(0);
                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtils.OK;

                    rs.Add(r);
                }
            }

            return rs;
        }

        public Room Get(string id)
        {
            Room r = new Room();
            r.Status.Code = StatusCode.NotFound;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL GetRoomFromName(\"" + id + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    r.RoomId = reader.GetInt32(0);
                    r.RoomName = reader.GetString(2);
                    r.Songs = new List<Song>();
                    r.Users = new List<RoomUser>();
                    
                    if (reader.NextResult())
                    {
                        Song s;
                        
                        while (reader.Read())
                        {
                            s = new Song();
                            s.SongId = reader.GetInt32(0);
                            s.SongName = reader.GetString(3);
                            s.SongArtist = reader.GetString(4);

                            r.Songs.Add(s);
                        }

                        if (reader.NextResult())
                        {
                            RoomUser ru;

                            while (reader.Read())
                            {
                                ru = new RoomUser();
                                ru.User = new User();
                                ru.User.UserId = reader.GetInt32(0);
                                ru.User.Username = reader.GetString(1);
                                ru.Tokens = reader.GetInt32(2);
                                
                                r.Users.Add(ru);
                            }

                            r.Status.Code = StatusCode.OK;
                            r.Status.Description = DataUtils.OK;
                        }
                    }
                }
            }

            return r;
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage AddSong(string id, Song song)
        {
            BaseModel status = new BaseModel();
            status.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, status);
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL AddSongToRoom(\"" + id + "\",\""
                                                        + song.ProviderId + "\",\""
                                                        + song.SongName + "\",\""
                                                        + song.SongArtist + "\",\""
                                                        + song.SongAlbum + "\",\""
                                                        + song.SongLength + "\",\""
                                                        + song.Tokens + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) == 0)
                    {
                        status.Status.Code = StatusCode.OK;
                        status.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        status.Status.Code = StatusCode.AlreadyExists;
                        status.Status.Description = "Points added to existing song";
                    }
                }
            }

            return response;
        }

        public HttpResponseMessage Post(Room room)
        {
            Room r = new Room();
            r.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, r);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && room != null && room.RoomName != "")
            {

                string query = "CALL CreateRoomIfNotExist(\"" + room.RoomName + "\",\"" + room.Users.ToArray()[0].User.Username + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > -1)
                    {
                        r.RoomName = room.RoomName;
                        r.Users = room.Users;
                        r.Status.Code = StatusCode.OK;
                        r.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        r.Status.Code = StatusCode.AlreadyExists;
                        r.Status.Description = "Room already exists";
                    }
                }
            }

            return response;
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage JoinGroup(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, bm);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "CALL JoinRoomWithUsername(\"" + user.Username + "\",\"" + id + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) == 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.AlreadyExists;
                        bm.Status.Description = "User already exists in that room";
                    }
                }
            }

            return response;
        }
    }
}
