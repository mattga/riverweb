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
    [Table("RoomSongs")]
    public class Song
    {
        public int SongId { get; set; }
        [JsonIgnore]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public string ProviderId { get; set; }
        public string SongName { get; set; }
        public string SongArtist { get; set; }
        public string SongAlbum { get; set; }
        public string AlbumArtURL { get; set; }
        public int SongLength { get; set; }
        public int SongYear { get; set; }
        public int Tokens { get; set; }
        public bool IsPlaying { get; set; }
        public DateTime? CreatedDate { get; set; }

        [JsonIgnore]
        public virtual Room Room { get; set; }

        public bool ReadSong(MySqlConnection connection)
        {
            string query = "SELECT * FROM RoomSongs WHERE SongId = " + this.SongId;
            MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.SongId = DataUtils.getInt32(reader, "SongId");
                    this.RoomId = DataUtils.getInt32(reader, "RoomId");
                    this.ProviderId = DataUtils.getString(reader, "ProviderId");
                    this.CreatedDate = DataUtils.getDateTime(reader, "CreatedDate");
                    this.SongName = DataUtils.getString(reader, "SongName");
                    this.SongArtist = DataUtils.getString(reader, "SongArtist");
                    this.SongAlbum = DataUtils.getString(reader, "SongAlbum");
                    this.SongLength = DataUtils.getInt32(reader, "Country");
                    this.SongYear = DataUtils.getInt32(reader, "SongYear");
                    this.Tokens = DataUtils.getInt32(reader, "Tokens");
                    this.IsPlaying = DataUtils.getBool(reader, "IsPlaying");

                    return true;
                }
            }
            return false;
        }
    }

    public class SongDbContext : DbContext
    {
        public SongDbContext() : base("DefaultConnection")
        {

        }
    }
}