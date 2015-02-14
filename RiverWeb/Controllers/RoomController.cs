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

            MySqlConnection connection = DataUtility.getConnection();

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
                MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                while (reader.Read())
                {
                    Room r = new Room();
                    r.RoomId = DataUtility.getInt32(reader, "RoomId");
                    r.RoomName = DataUtility.getString(reader, "RoomName");
                    r.isPrivate = DataUtility.getBool(reader, "isPrivate");
                    r.AccessCode = DataUtility.getInt32(reader, "AccessCode");
                    r.Latitude = DataUtility.getDouble(reader, "Latitude");
                    r.Longitude = DataUtility.getDouble(reader, "Longitude");
                    r.SongCount = DataUtility.getInt32(reader, "SongCount");
                    r.UserCount = DataUtility.getInt32(reader, "UserCount");
                    
                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtility.OK;

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
            r.Status.Description = "Room not found.";

            MySqlConnection connection = DataUtility.getConnection();

            if (connection != null)
            {
                if (r.ReadRoom(connection))
                {
                    r.Status.Code = StatusCode.OK;
                    r.Status.Description = DataUtility.OK;
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
            MySqlConnection connection = DataUtility.getConnection();

            try
            {
                if (connection != null)
                {
                    string query = "SELECT * FROM RoomSongs WHERE SourceId='" + song.SourceId + "' AND RoomId=" + id;
                    MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);
                    if (reader.Read())
                    {

                        s.SongId = DataUtility.getInt32(reader, "SongId");
                        s.RoomId = DataUtility.getInt32(reader, "RoomId");
                        s.SourceId = DataUtility.getString(reader, "SourceId");
                        s.Title = DataUtility.getString(reader, "Title");
                        s.Artist = DataUtility.getString(reader, "Artist");
                        s.Album = DataUtility.getString(reader, "Album");
                        s.Thumbnail = DataUtility.getString(reader, "Thumbnail");
                        s.Length = DataUtility.getInt32(reader, "Length");
                        s.PublishedDate = DataUtility.getDateTime(reader, "PublishedDate");
                        s.Tokens = DataUtility.getInt32(reader, "Tokens") + song.Tokens;
                        s.CreatedDate = DataUtility.getDateTime(reader, "CreatedDate");
                        s.Source = DataUtility.getString(reader, "Source");

                        query = "UPDATE RoomSongs SET Tokens=Tokens+" + song.Tokens + " WHERE SongId=" + s.SongId;
                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            s.Status.Code = StatusCode.AlreadyExists;
                            s.Status.Description = "Song already exists in this Room. Points added.";
                        }
                    }
                    else
                    {
                        query = "INSERT INTO RoomSongs (RoomId,SourceId,Title,Artist,Album,Length,PublishedDate,Source,Thumbnail,Thumbnail_Lg,Tokens) VALUES (";
                        query += id + ",'" + song.SourceId + "','" + song.Title + "','" + song.Artist + "',";
                        if (song.Album != null)
                        {
                            query += "'" + song.Album + "',";
                        }
                        else
                        {
                            query += "NULL,";
                        }
                        query += song.Length + ",'" + song.PublishedDate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + song.Source + "',";
                        if (song.Thumbnail != null)
                        {
                            query += "'" + song.Thumbnail + "',";
                        }
                        else
                        {
                            query += "NULL,";
                        }
                        if (song.SourceId == "YT")
                        {
                            query += "'http://img.youtube.com/vi/" + song.SourceId + "/mqdefault.jpg',";
                        }
                        else
                        {
                            if (song.Thumbnail != null)
                            {
                                query += "'" + song.Thumbnail + "',";
                            }
                            else
                            {
                                query += "NULL,";
                            }
                        }
                        query += song.Tokens + ")";

                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            query = "SELECT * FROM RoomSongs, Rooms WHERE Rooms.RoomId=RoomSongs.RoomId AND SongId=LAST_INSERT_ID()";
                            reader.Close();
                            reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                            if (reader.Read())
                            {
                                s.SongId = DataUtility.getInt32(reader, "SongId");
                                s.RoomId = DataUtility.getInt32(reader, "RoomId");
                                s.SourceId = DataUtility.getString(reader, "SourceId");
                                s.Title = DataUtility.getString(reader, "Title");
                                s.Artist = DataUtility.getString(reader, "Artist");
                                s.Album = DataUtility.getString(reader, "Album");
                                s.Thumbnail = DataUtility.getString(reader, "Thumbnail");
                                s.Length = DataUtility.getInt32(reader, "Length");
                                s.PublishedDate = DataUtility.getDateTime(reader, "PublishedDate");
                                s.Tokens = DataUtility.getInt32(reader, "Tokens") + song.Tokens;
                                s.CreatedDate = DataUtility.getDateTime(reader, "CreatedDate");
                                s.Source = DataUtility.getString(reader, "Source");

                                s.Status.Code = StatusCode.OK;
                                s.Status.Description = DataUtility.OK;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                s.Status.Description = ex.StackTrace;
            }

            return s;
        }
        
        [ActionName("DefaultAction")]
        public Room Post(Room room)
        { 
            Room r = new Room();
            r.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            try
            {
                if (connection != null && room != null && room.RoomName != "")
                {

                    string query = "INSERT INTO Rooms " +
                                    "(HostId,RoomName,isPrivate" + (room.isPrivate ? ",AccessCode," : ",") + "Latitude,Longitude) " +
                                    "VALUES (" + room.HostId + ",'" + room.RoomName + "'," + room.isPrivate +
                                    (room.isPrivate ? "," + room.AccessCode : "") + "," + room.Latitude + "," + room.Longitude + ")";
                    MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (reader.RecordsAffected > -1)
                    {
                        r.RoomName = room.RoomName;

                        query = "SELECT LAST_INSERT_ID()";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);
                        if (reader.Read())
                        {
                            r.RoomId = reader.GetInt32(0);

                            query = "INSERT INTO RoomUsers VALUES (" + r.RoomId + "," + room.HostId + ",100)";
                            reader.Close();
                            reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

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
            }
            catch (Exception ex)
            {
                r.Status.Description = ex.StackTrace;
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
                MySqlConnection connection = DataUtility.getConnection();

                if (connection != null && user != null && user.Username != "")
                {
                    string query = "SELECT * FROM RoomUsers WHERE RoomId=" + id + " AND UserId=" + user.UserId;
                    MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (!reader.Read())
                    {
                        query = "INSERT INTO RoomUsers VALUES (" + id + "," + user.UserId + ",100)";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            r.Status.Code = StatusCode.OK;
                            r.Status.Description = DataUtility.OK;
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
        public BaseModel JoinNearest(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtility.getConnection();

            try
            {
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
                                        "SELECT " + user.Latitude + " AS latpoint, " + user.Longitude + " AS longpoint, " +
                                            "50.0 AS radius, 69.0 AS distance_unit " +
                                    ") AS p ON 1=1 " +
                                    "WHERE z.latitude " +
                                        "BETWEEN p.latpoint  - (p.radius / p.distance_unit) " +
                                            "AND p.latpoint  + (p.radius / p.distance_unit) " +
                                    "AND z.longitude " +
                                        "BETWEEN p.longpoint - (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                            "AND p.longpoint + (p.radius / (p.distance_unit * COS(RADIANS(p.latpoint)))) " +
                                    "ORDER BY distance_in_km";

                    MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

                    if (reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                        {
                            bm.Status.Code = StatusCode.OK;
                            bm.Status.Description = DataUtility.OK;
                        }
                        else
                        {
                            bm.Status.Code = StatusCode.NotFound;
                            bm.Status.Description = "Room not found";
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                bm.Status.Description = ex.StackTrace;
            }

            return bm;
        }

    }
}
