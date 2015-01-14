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
    public class Song : BaseModel
    {
        public int SongId { get; set; }
        [JsonIgnore]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public string SourceId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Thumbnail { get; set; }
        public int Length { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int Tokens { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Source { get; set; }

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
                    this.SourceId = DataUtils.getString(reader, "SourceId");
                    this.Title = DataUtils.getString(reader, "Title");
                    this.Artist = DataUtils.getString(reader, "Artist");
                    this.Album = DataUtils.getString(reader, "Album");
                    this.Thumbnail = DataUtils.getString(reader, "Thumbnail");
                    this.Length = DataUtils.getInt32(reader, "Length");
                    this.PublishedDate = DataUtils.getDateTime(reader, "PublishedDate");
                    this.Tokens = DataUtils.getInt32(reader, "Tokens");
                    this.Source = DataUtils.getString(reader, "Source");
                    this.CreatedDate = DataUtils.getDateTime(reader, "CreatedDate");

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