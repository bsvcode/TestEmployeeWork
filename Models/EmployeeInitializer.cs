using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Employee.Models
{
    public class EmployeeInitializer : DropCreateDatabaseIfModelChanges<EmployeeDBContext>
    {
        protected override void Seed(EmployeeDBContext context)
        {
            var genders = new List<Gender>
            {
                new Gender { ID = 1, Name = "Male" },
                new Gender { ID = 2, Name = "Female" }
            };

            genders.ForEach(x => context.Genders.Add(x));
            context.SaveChanges();

            var initialEmployee = new List<Employee>
            {
                new Employee
                {
                    FirstName = "Ivan",
                    MiddleName = "Petrovich",
                    LastName = "Sydorenko",
                    BirthDate = new DateTime(1970, 05, 27),
                    GenderID = 1,
                    IsExternal = false,
                    PersonalNumber = "S1"
                }                
            };
            initialEmployee.ForEach(x => context.Employees.Add(x));
            context.SaveChanges();
        }
    }
}