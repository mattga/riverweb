using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RiverWeb.Models
{
    [Table("RoomSongs")]
    public class Song
    {
        [JsonIgnore]
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
        public int IsPlaying { get; set; }

        [JsonIgnore]
        public virtual Room Room { get; set; }
    }

    public class SongDbContext : DbContext
    {
        public SongDbContext() : base("DefaultConnection")
        {

        }
    }
}