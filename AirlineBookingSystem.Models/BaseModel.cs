using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AirlineBookingSystem.Models
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }
    }
}
