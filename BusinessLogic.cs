using Employee.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Linq.Dynamic;

namespace Employee
{
    public class BusinessLogic
    {
        private EmployeeDBContext db = new EmployeeDBContext();

        public IQueryable GetList()
        {
            var employees = db.Employees.Include(e => e.Gender);

            return employees;
        }

        public List<Models.Employee> GetValidEmployees(string json, out List<Models.Employee> notValidList)
        { 
            var serializer = new JavaScriptSerializer();
            var employees = serializer.Deserialize<List<Employee.Models.Employee>>(json);
            var validList = employees.Where(x => IsValid(x));
            notValidList = employees.Where(x => !IsValid(x)).ToList();
            return validList.ToList();
        }
        public StringBuilder CreateOrUpdateEmployees(List<Employee.Models.Employee> employees)
        {
            foreach (var emp in employees)
            {
                Employee.Models.Employee exist = null;
                if (emp.ID != 0)
                {
                    exist = db.Employees.FirstOrDefault(x => x.ID == emp.ID);
                }
                else
                {
                    exist = db.Employees.FirstOrDefault(x => x.LastName == emp.LastName
                                                          && x.IsExternal == emp.IsExternal
                                                          && x.PersonalNumber == emp.PersonalNumber
                                                       );
                }

                if (exist == null)
                {
                    db.Employees.Add(emp);
                    var qqq = db.ChangeTracker.Entries();
                }
                else
                {
                    exist.FirstName = emp.FirstName;
                    exist.MiddleName = emp.MiddleName;
                    exist.LastName = emp.LastName;
                    exist.GenderID = emp.GenderID;
                    exist.BirthDate = emp.BirthDate;
                    exist.IsExternal = emp.IsExternal;
                    exist.PersonalNumber = emp.PersonalNumber;
                }
            }
            
            var log = GetLog();
            db.SaveChanges();
            return log;
        }

        private StringBuilder GetLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Upload Log at {DateTime.Now}");

            var added = db.ChangeTracker.Entries<Models.Employee>().Where(x => x.State == EntityState.Added);
            if (added.Any())
            {
                sb.AppendLine("--- Were added ---");
                foreach (var m in added)
                {
                    sb.AppendLine($"LastName: {m.Entity.LastName}; MiddleName: {m.Entity.MiddleName}; FirstName: {m.Entity.FirstName}; BirthDate: {m.Entity.BirthDate}; GenderID: {m.Entity.GenderID}; IsExternal: {m.Entity.IsExternal}; PersonalNumber: {m.Entity.PersonalNumber}");
                }
            }

            var modified = db.ChangeTracker.Entries<Models.Employee>().Where(x => x.State == EntityState.Modified);
            if (modified.Any())
            {
                sb.AppendLine("--- Were modified ---");
                foreach (var m in modified)
                {
                    sb.AppendLine($"ID: {m.Entity.ID}; LastName: {m.Entity.LastName}; MiddleName: {m.Entity.MiddleName}; FirstName: {m.Entity.FirstName}; BirthDate: {m.Entity.BirthDate}; GenderID: {m.Entity.GenderID}; IsExternal: {m.Entity.IsExternal}; PersonalNumber: {m.Entity.PersonalNumber}");
                }
            }
            if (!added.Any() && !modified.Any())
            {
                sb.AppendLine("Nothing has been done");
            }
            return sb;
        }

        private bool IsValid(Models.Employee model)
        {
            return (model.IsExternal && string.IsNullOrEmpty(model.PersonalNumber))
                 || (!model.IsExternal && !string.IsNullOrEmpty(model.PersonalNumber));
        }
        
        public List<EmployeeVieModel> GetList(int pageIndex, int pageSize, string propertyName, string order, out int total)
        {
            if (propertyName == "Gender")
            {
                propertyName = "Gender.Name";
            }

            total = db.Employees.Include(e => e.Gender).Count();
            var employees = db.Employees.Include(e => e.Gender)
                .OrderBy($"{propertyName} {order}")
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToList()
                .Select(x => new EmployeeVieModel{
                    ID = x.ID,
                    FirstName = x.FirstName,
                    MiddleName = x.MiddleName,
                    LastName = x.LastName,
                    Gender = x.Gender.Name,
                    GenderID = x.Gender.ID,
                    BirthDate = x.BirthDate.ToShortDateString(),
                    IsExternal = x.IsExternal,
                    PersonalNumber = x.PersonalNumber
                });
            
            return employees.ToList();
        }
    }
}