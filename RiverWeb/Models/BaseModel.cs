using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiverWeb.Models
{

    public enum StatusCode
    {
        OK = 0,
        NotFound,
        AlreadyExists,
        Error = 9
    }

    public class BaseModel
    {
        private ModelStatus status = new ModelStatus();

        [NotMapped]
        public ModelStatus Status
        {
            get { return status; }
            set { status = value; }
        }

    }

    public class ModelStatus
    {
        [NotMapped]
        public StatusCode Code { get; set; }
        [NotMapped]
        public String Description { get; set; }
        [NotMapped]
        public String StackTrace { get; set; }
    }
}
