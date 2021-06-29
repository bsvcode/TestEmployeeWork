using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Employee.Models
{
    public class EmployeeVieModel
    {
        public int ID { get; set; }        
        public string FirstName { get; set; }        
        public string MiddleName { get; set; }                
        public string LastName { get; set; }
        public int GenderID { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }        
        public bool IsExternal { get; set; }
        public string PersonalNumber { get; set; }
    }
}