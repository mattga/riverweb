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
                string query = "SELECT z.*,COUNT(rs.SongId) AS SongCount,COUNT(ru.UserId) AS UserCount";
                if (latitude != "" && longitude != "")
                {
                    query += ",p.distance_unit " +
                                    "* DEGREES(ACOS(COS(RADIANS(p.latpoint)) " +
                                    "* COS(RADIANS(z.latitude)) " +
                                    "* COS(RADIANS(p.longpoint) - RADIANS(z.longitude)) " +
                                    "+ SIN(RADIANS(p.latpoint)) " +
                                    "* SIN(RADIANS(z.latitude)))) AS distance_in_km " +
                                "FROM Rooms AS z LEFT OUTER JOIN RoomSongs AS rs ON z.RoomId=rs.RoomId " +
                                    "LEFT OUTER JOIN RoomUsers AS ru ON z.RoomId=ru.RoomId " +
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
                                "GROUP BY z.RoomId ORDER BY distance_in_km";
                }
                else
                {
                    query += " FROM Rooms AS z LEFT OUTER JOIN RoomSongs AS rs ON z.RoomId=rs.RoomId " +
                                    "LEFT OUTER JOIN RoomUsers AS ru ON z.RoomId=ru.RoomId " +
                                    "GROUP BY z.RoomId";
                }
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                while (reader.Read())
                {
                    Room r = new Room();
                    r.RoomId = DataUtils.getInt32(reader, "RoomId");
                    r.RoomName = DataUtils.getString(reader, "RoomName");
                    r.isPrivate = DataUtils.getBool(reader, "isPrivate");
                    r.AccessCode = DataUtils.getInt32(reader, "AccessCode");
                    r.Latitude = DataUtils.getDouble(reader, "Latitude");
                    r.Longitude = DataUtils.getDouble(reader, "Longitude");
                    r.SongCount = DataUtils.getInt32(reader, "SongCount");
                    r.UserCount = DataUtils.getInt32(reader, "UserCount");
                    
                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtils.OK;

                    rs.Add(r);
                }
                connection.Close();
            }

            return rs;
        }

        [ActionName("DefaultAction")]
        public Room Get(string id)
        {
            Room r = new Room();
            r.RoomId = Convert.ToInt32(id);
            r.Status.Code = StatusCode.NotFound;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                if (r.ReadRoom(connection))
                {
                    r.Status.Code = StatusCode.OK;
                }

                connection.Close();
            }

            return r;
        }

        [System.Web.Http.HttpPost]
        public Song AddSong(string id, Song song)
        {
            Song s = new Song();
            s.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * FROM RoomSongs WHERE SourceId='" + song.SourceId + "' AND RoomId=" + id;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                if (reader.Read()) {

                    s.SongId = DataUtils.getInt32(reader, "SongId");
                    s.RoomId = DataUtils.getInt32(reader, "RoomId");
                    s.SourceId = DataUtils.getString(reader, "SourceId");
                    s.Title = DataUtils.getString(reader, "Title");
                    s.Artist = DataUtils.getString(reader, "Artist");
                    s.Album = DataUtils.getString(reader, "Album");
                    s.Thumbnail = DataUtils.getString(reader, "Thumbnail");
                    s.Length = DataUtils.getInt32(reader, "Length");
                    s.PublishedDate = DataUtils.getDateTime(reader, "PublishedDate");
                    s.Tokens = DataUtils.getInt32(reader, "Tokens") + song.Tokens;
                    s.CreatedDate = DataUtils.getDateTime(reader, "CreatedDate");
                    s.Source = DataUtils.getString(reader, "Source");

                    query = "UPDATE RoomSongs SET Tokens=Tokens+" + song.Tokens + " WHERE SongId=" + s.SongId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        s.Status.Code = StatusCode.AlreadyExists;
                        s.Status.Description = "Song already exists in this Room. Points added.";
                    }
                } else {
                    query = "INSERT INTO RoomSongs (RoomId,SourceId,Title,Artist,Album,Length,PublishedDate,Source,Thumbnail,Tokens) VALUES (";
                    query += id + ",'" + song.SourceId + "','" + song.Title + "','" + song.Artist + "',";
                    if (song.Album != null)
                    {
                        query += "'" + song.Album + "',";
                    }
                    else
                    {
                        query += "NULL,";
                    }
                    query += song.Length + ",'" + song.PublishedDate + "','" + song.Source + "',";
                    if (song.Thumbnail != null)
                    {
                        query += "'" + song.Thumbnail + "',";
                    }
                    else
                    {
                        query += "NULL,";
                    }
                    query += song.Tokens + ")";

                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        query = "SELECT * FROM RoomSongs, Rooms WHERE Rooms.RoomId=RoomSongs.RoomId AND SongId=LAST_INSERT_ID()";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                        if (reader.Read())
                        {
                            s.SongId = DataUtils.getInt32(reader, "SongId");
                            s.RoomId = DataUtils.getInt32(reader, "RoomId");
                            s.SourceId = DataUtils.getString(reader, "SourceId");
                            s.Title = DataUtils.getString(reader, "Title");
                            s.Artist = DataUtils.getString(reader, "Artist");
                            s.Album = DataUtils.getString(reader, "Album");
                            s.Thumbnail = DataUtils.getString(reader, "Thumbnail");
                            s.Length = DataUtils.getInt32(reader, "Length");
                            s.PublishedDate = DataUtils.getDateTime(reader, "PublishedDate");
                            s.Tokens = DataUtils.getInt32(reader, "Tokens") + song.Tokens;
                            s.CreatedDate = DataUtils.getDateTime(reader, "CreatedDate");
                            s.Source = DataUtils.getString(reader, "Source");

                            s.Status.Code = StatusCode.OK;
                            s.Status.Description = DataUtils.OK;
                        }
                    }
                }
                connection.Close();
            }

            return s;
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

                        query = "INSERT INTO RoomUsers VALUES (" + r.RoomId + "," + room.HostId + ",100)";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            r.Status.Code = StatusCode.OK;
                            r.Status.Description = "Room created successfully";
                        }
                    }
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
        public Room JoinRoom(string id, User user)
        {
            Room r = new Room();
            r.RoomId = Convert.ToInt32(id);
            r.Status.Code = StatusCode.Error;

            try
            {
                MySqlConnection connection = DataUtils.getConnection();

                if (connection != null && user != null && user.Username != "")
                {
                    string query = "SELECT * FROM RoomUsers WHERE RoomId=" + id + " AND UserId=" + user.UserId;
                    MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (!reader.Read())
                    {
                        query = "INSERT INTO RoomUsers VALUES (" + id + "," + user.UserId + ",100)";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            r.Status.Code = StatusCode.OK;
                            r.Status.Description = DataUtils.OK;
                        }
                        else
                        {
                            r.Status.Code = StatusCode.NotFound;
                            r.Status.Description = "User/Room not found";
                        }
                    }
                    else
                    {
                        r.Status.Code = StatusCode.AlreadyExists;
                        r.Status.Description = "User already in room " + id;
                    }

                    reader.Close();
                    r.ReadRoom(connection);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                r.Status.Description = ex.StackTrace;
            }

            return r;
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
