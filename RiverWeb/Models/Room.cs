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

        public int SongCount { get; set; }
        public int UserCount { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<RoomUser> Users { get; set; }

        public bool ReadRoom(MySqlConnection connection)
        {
            string query = "SELECT * FROM Rooms WHERE RoomId = " + this.RoomId;
            MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.RoomId = DataUtils.getInt32(reader, "RoomId");
                    this.RoomName = DataUtils.getString(reader, "RoomName");
                    this.CreatedDate = DataUtils.getDateTime(reader, "CreatedDate");
                    this.HostId = DataUtils.getInt32(reader, "HostId");
                    this.Latitude = DataUtils.getDouble(reader, "Latitude");
                    this.Longitude = DataUtils.getDouble(reader, "Longitude");
                    this.isPrivate = DataUtils.getBool(reader, "IsPrivate");
                    this.AccessCode = DataUtils.getInt32(reader, "AccessCode");
                    this.Users = new List<RoomUser>();
                    this.Songs = new List<Song>();

                    query = "SELECT * FROM RoomUsers WHERE RoomId = " + this.RoomId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    while (reader.Read())
                    {
                        User u = new User();
                        u.UserId = DataUtils.getInt32(reader, "UserId");
                        u.ReadUser(DataUtils.getConnection());
                        RoomUser ru = new RoomUser();
                        ru.User = u;
                        ru.Tokens = DataUtils.getInt32(reader, "Tokens");
                        this.Users.Add(ru);
                    }

                    query = "SELECT * FROM RoomSongs WHERE RoomId = " + this.RoomId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    while (reader.Read())
                    {
                        Song s = new Song();
                        s.SongId = DataUtils.getInt32(reader, "SongId");
                        s.ReadSong(DataUtils.getConnection());
                        this.Songs.Add(s);
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