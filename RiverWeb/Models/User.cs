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
    [Table("Users")]
    public class User : BaseModel
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public String Username { get; set; }

    }

    public class UserDBContext : DbContext
    {
        public UserDBContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<User> Users { get; set; }
    }
}