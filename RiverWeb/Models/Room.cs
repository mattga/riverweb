using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RiverWeb.Utils;
using MySql.Data.MySqlClient;

namespace RiverWeb.Models
{
    [Table("Rooms")]
    public class Room : BaseModel
    {
        public int RoomId { get; set; }
        public int HostId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string RoomName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool isPrivate { get; set; }
        public int AccessCode { get; set; }
        public bool AllowYT { get; set; }
        public bool AllowSP { get; set; }
        public int CurrentSong { get; set; }
        public bool isPlaying { get; set; }
        public int CurrentSongTime { get; set; }

        [NotMapped]
        public int SongCount { get; set; }
        [NotMapped]
        public int UserCount { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<RoomUser> Users { get; set; }

        public bool ReadRoom(MySqlConnection connection)
        {
            string query = "SELECT * FROM Rooms WHERE RoomId = " + this.RoomId;
            MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.RoomId = DataUtility.getInt32(reader, "RoomId");
                    this.RoomName = DataUtility.getString(reader, "RoomName");
                    this.CreatedDate = DataUtility.getDateTime(reader, "CreatedDate");
                    this.HostId = DataUtility.getInt32(reader, "HostId");
                    this.Latitude = DataUtility.getDouble(reader, "Latitude");
                    this.Longitude = DataUtility.getDouble(reader, "Longitude");
                    this.isPrivate = DataUtility.getBool(reader, "IsPrivate");
                    this.AccessCode = DataUtility.getInt32(reader, "AccessCode");
                    this.AllowSP = DataUtility.getBool(reader, "AllowSP");
                    this.AllowYT = DataUtility.getBool(reader, "AllowYT");
                    this.CurrentSong = DataUtility.getInt32(reader, "CurrentSong");
                    this.isPlaying = DataUtility.getBool(reader, "IsPlaying");
                    this.CurrentSongTime = DataUtility.getInt32(reader, "CurrentSongTime");
                    this.Users = new List<RoomUser>();
                    this.Songs = new List<Song>();

                    query = "SELECT * FROM RoomUsers WHERE RoomId = " + this.RoomId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);
                    while (reader.Read())
                    {
                        MySqlConnection connection2 = DataUtility.getConnection();
                        User u = new User();
                        u.UserId = DataUtility.getInt32(reader, "UserId");
                        u.ReadUser(connection2);
                        RoomUser ru = new RoomUser();
                        ru.User = u;
                        ru.Tokens = DataUtility.getInt32(reader, "Tokens");
                        this.Users.Add(ru);
                        connection2.Close();
                    }

                    query = "SELECT * FROM RoomSongs WHERE RoomId = " + this.RoomId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);
                    while (reader.Read())
                    {
                        MySqlConnection connection2 = DataUtility.getConnection();
                        Song s = new Song();
                        s.SongId = DataUtility.getInt32(reader, "SongId");
                        s.ReadSong(connection2);
                        this.Songs.Add(s);
                        connection2.Close();
                    }

                    return true;
                }
            }
            return false;
        }
    }

    [Table("RoomUsers")]
    public class RoomUser
    {
        [JsonIgnore]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public int Tokens { get; set; }

        [JsonIgnore]
        public virtual Room Room { get; set; }
        public virtual User User { get; set; }
    }

    public class RoomDBContext : DbContext
    {
        public RoomDBContext() : base("DefaultConnection")
        {

        }
    }
}