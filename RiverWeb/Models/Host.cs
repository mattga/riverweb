using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RiverWeb.Utils;

namespace RiverWeb.Models
{
    [Table("Hosts")]
    public class Host : BaseModel
    {
        public int UserId { get; set; }
        public string spUserName { get; set; }
        public string spPassword { get; set; }
    }

    public class HostDBContext : DbContext
    {
        public HostDBContext()
            : base("DefaultConnection")
        {

        }
    }
}
