﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RiverWeb.Models
{
    [Table("Rooms")]
    public class Room : BaseModel
    {
        public int RoomId { get; set; }
        public int HostId { get; set; }
        public string RoomName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool isPrivate { get; set; }
        public int AccessCode { get; set; }

        public int SongCount { get; set; }
        public int UserCount { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<RoomUser> Users { get; set; }
    }

    [Table("RoomUsers")]
    public class RoomUser
    {
        [JsonIgnore]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        [JsonIgnore]
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