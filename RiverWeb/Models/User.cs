﻿using System;
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
    public class User : BaseModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ImageUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string spUsername { get; set; }
        public string spCanonicalUsername { get; set; }
        public string spEmail { get; set; }

        public bool ReadUser(MySqlConnection connection)
        {
            string query = "SELECT * FROM Users WHERE Users.UserId = " + this.UserId;
            MySqlDataReader reader = (MySqlDataReader)DataUtility.executeQuery(connection, query);

            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.UserId = DataUtility.getInt32(reader, "UserId");
                    this.Username = DataUtility.getString(reader, "Username");
                    this.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    this.Email = DataUtility.getString(reader, "Email");
                    this.City = DataUtility.getString(reader, "City");
                    this.State = DataUtility.getString(reader, "State");
                    this.Country = DataUtility.getString(reader, "Country");
                    this.ImageUrl = DataUtility.getString(reader, "ImageUrl");
                    this.spUsername = DataUtility.getString(reader, "spUsername");

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