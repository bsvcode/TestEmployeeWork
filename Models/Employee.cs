using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Employee.Models
{
    public class Employee
    {
        public int ID { get; set; }

        [MaxLength(50)]        
        public string FirstName { get; set; }

        [MaxLength(50)]        
        public string MiddleName { get; set; }

        [MaxLength(50)]
        [Index("UQ_IsExternal_PersonalNumber", 3, IsUnique = true)]
        public string LastName { get; set; }
        public int GenderID { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }

        [Index("UQ_IsExternal_PersonalNumber", 2, IsUnique = true)]
        public bool IsExternal { get; set; }

        [Index("UQ_IsExternal_PersonalNumber", 1, IsUnique = true)]
        [MaxLength(10)]
        [RequiredIfNot("IsExternal", true, ErrorMessage = "Required ONLY if not External")]
        public string PersonalNumber { get; set; }
    }
}
