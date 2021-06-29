using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Employee.Models
{
    public class Gender
    {
        public int ID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}