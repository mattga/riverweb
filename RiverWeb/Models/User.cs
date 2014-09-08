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
    [Table("Users")]
    /*public class User : BaseModel
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
    }*/


    public class User : BaseModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ImageUrl { get; set; }
        public bool IsFaceBook { get; set; }

        public bool ReadUser(MySqlConnection connection)
        {
            string query = "SELECT * FROM Users WHERE UserId = " + this.UserId;
            MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.UserId = DataUtils.getInt32(reader, "UserId");
                    this.Username = DataUtils.getString(reader, "Username");
                    this.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    this.Email = DataUtils.getString(reader, "Email");
                    this.City = DataUtils.getString(reader, "City");
                    this.State = DataUtils.getString(reader, "State");
                    this.Country = DataUtils.getString(reader, "Country");
                    this.ImageUrl = DataUtils.getString(reader, "ImageUrl");
                    this.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                    return true;
                }
            }
            return false;
        }
    }

    public class UserDBContext : DbContext
    {
        public UserDBContext() : base("DefaultConnection")
        {

        }
    }
}