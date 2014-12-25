using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using RiverWeb.Models;
using RiverWeb.Utils;
using MySql.Data.MySqlClient;

namespace RiverWeb.Controllers
{
    public class RoomController : ApiController
    {
        [ActionName("DefaultAction")]
        public IEnumerable<Room> Get(string latitude="", string longitude="")
        {
            List<Room> rs = new List<Room>();

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT *";
                if (latitude != "" && longitude != "")
                {
                    query += ",p.distance_unit " +
                                    "* DEGREES(ACOS(COS(RADIANS(p.latpoint)) " +
                                    "* COS(RADIANS(z.latitude)) " +
                                    "* COS(RADIANS(p.longpoint) - RADIANS(z.longitude)) " +
                                    "+ SIN(RADIANS(p.latpoint)) " +
                                    "* SIN(RADIANS(z.latitude)))) AS distance_in_km " +
                                "FROM Rooms AS z " +
                                "JOIN ( " +
                                    "SELECT " + latitude + " AS latpoint, " + longitude + " AS longpoint, " +
                                        "50.0 AS radius, 69.0 AS distance_unit " +
                                ") AS p ON 1=1 " +
                                "WHERE z.latitude " +
                                    "BETWEEN p.latpoint  - (p.radius / p.distance_unit) " +
                                        "AND p.latpoint  + (p.radius / p.distance_unit) " +
                                "AND z.longitude " +
                                    "BETWEEN p.longpoint - (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                        "AND p.longpoint + (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                "ORDER BY distance_in_km";
                }
                else
                {
                    query += " FROM Rooms";
                }
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                while (reader.Read())
                {
                    Room r = new Room();
                    r.RoomName = reader.GetString(0);
                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtils.OK;

                    rs.Add(r);
                }
                connection.Close();
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
                            s.ProviderId = reader.GetString(2);
                            s.SongName = reader.GetString(3);
                            s.SongArtist = reader.GetString(4);
                            s.SongAlbum = reader.GetString(5);
                            s.SongLength = reader.GetInt32(6);
                            s.SongYear = reader.GetInt32(7);
                            s.Tokens = reader.GetInt32(8);
                            s.IsPlaying = reader.GetInt32(9);
                            s.AlbumArtURL = reader.GetString(10);

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
                connection.Close();
            }

            return r;
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage AddSong(string id, Room room)
        {
            BaseModel status = new BaseModel();
            status.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, status);
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "CALL AddSongToRoom(\"" + room.RoomName + "\",\""
                                                        + room.Users.ToArray()[0].User.Username + "\",\""
                                                        + room.Songs.ToArray()[0].ProviderId + "\",\""
                                                        + room.Songs.ToArray()[0].SongName + "\",\""
                                                        + room.Songs.ToArray()[0].SongArtist + "\",\""
                                                        + room.Songs.ToArray()[0].SongAlbum + "\",\""
                                                        + room.Songs.ToArray()[0].SongLength + "\",\""
                                                        + room.Songs.ToArray()[0].SongYear + "\",\""
                                                        + room.Songs.ToArray()[0].AlbumArtURL + "\",\""
                                                        + room.Songs.ToArray()[0].Tokens + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
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
                connection.Close();
            }

            return response;
        }
        
        [ActionName("DefaultAction")]
        public Room Post(Room room)
        { 
            Room r = new Room();
            r.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && room != null && room.RoomName != "")
            {

                string query = "INSERT INTO Rooms " + 
                                "(HostId,RoomName,isPrivate" + (room.isPrivate ? ",AccessCode," : ",") + "Latitude,Longitude) " +
                                "VALUES (" + room.HostId + ",'" + room.RoomName + "'," + room.isPrivate +
                                (room.isPrivate ? ","+room.AccessCode : "" ) + "," + room.Latitude + "," + room.Longitude + ")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected > -1)
                {
                    r.RoomName = room.RoomName;

                    query = "SELECT LAST_INSERT_ID()";
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    if (reader.Read())
                    {
                        r.RoomId = reader.GetInt32(0);
                    }

                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtils.OK;
                }
                else
                {
                    r.Status.Code = StatusCode.AlreadyExists;
                    r.Status.Description = "Room already exists";
                }

                connection.Close();
            }

            return r;
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage JoinRoom(string id, User user)
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
                    if (reader.GetInt32(0) > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.NotFound;
                        bm.Status.Description = "Room not found";
                    }
                }
                connection.Close();
            }

            return response;
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage JoinNearest(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, bm);

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "SELECT *, " +
                                "p.distance_unit " +
                                         "* DEGREES(ACOS(COS(RADIANS(p.latpoint)) " +
                                         "* COS(RADIANS(z.latitude)) " +
                                         "* COS(RADIANS(p.longpoint) - RADIANS(z.longitude)) " +
                                         "+ SIN(RADIANS(p.latpoint)) " +
                                         "* SIN(RADIANS(z.latitude)))) AS distance_in_km " +
                                "FROM Rooms AS z " +
                                "JOIN ( " +
                                    "SELECT " +  user.Latitude + " AS latpoint, " + user.Longitude + " AS longpoint, " +
                                        "50.0 AS radius, 69.0 AS distance_unit " +
                                ") AS p ON 1=1 " +
                                "WHERE z.latitude " +
                                    "BETWEEN p.latpoint  - (p.radius / p.distance_unit) " +
                                        "AND p.latpoint  + (p.radius / p.distance_unit) " +
                                "AND z.longitude " +
                                    "BETWEEN p.longpoint - (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                        "AND p.longpoint + (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                "ORDER BY distance_in_km";

                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = DataUtils.OK;
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.NotFound;
                        bm.Status.Description = "Room not found";
                    }
                }
                connection.Close();
            }

            return response;
        }

    }
}
