using Employee.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Employee.Controllers
{
    public class HomeController : Controller
    {
        private EmployeeDBContext db = new EmployeeDBContext();        
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetList(string sort, string order)
        {
            var pageIndex = int.Parse(Request["page"]);
            var pageSize = int.Parse(Request["rows"]);
            var total = 0;            
            var bl = new BusinessLogic();
            var employees = bl.GetList(pageIndex, pageSize, sort, order, out total);
            
            return Json(new { total, rows = employees }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGenders()
        {
            var genders = db.Genders
                .Select(x => new
                {
                    Text = x.Name,
                    Value = x.ID
                }).ToArray();

            return Json(genders, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,FirstName,MiddleName,LastName,GenderID,BirthDate,IsExternal,PersonalNumber")] Models.Employee employee)
        {
            if (!IsValid(employee))
            {
                ModelState.AddModelError("PersonalNumber", "Required ONLY if not External");
            }

            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                bool changesMade = db.ChangeTracker.HasChanges();
                var changeCounts = db.ChangeTracker.Entries().Count();
                var entries = db.ChangeTracker.Entries();
                return Json(new { Result = 0 }, JsonRequestBehavior.AllowGet);
            }
            var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            var errors = ModelState.Keys.Where(x => ModelState[x].Errors.Count > 0).
                Select(x => new { propertyName = x, errorMessage = ModelState[x].Errors[0].ErrorMessage });
            var errorsString = string.Join("\r\n", errors);

            return Json(new { Result = 1, Message = $"Errrors: {errorsString}" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "ID,FirstName,MiddleName,LastName,GenderID,BirthDate,IsExternal,PersonalNumber")] Models.Employee employee)
        {
            if (!IsValid(employee))
            {
                ModelState.AddModelError("PersonalNumber", "Required ONLY if not External");
            }

            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                return Json(new { Result = 0 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Result = 1, Message = "Errror" }, JsonRequestBehavior.AllowGet);
        }

        private bool IsValid(Models.Employee model)
        {
            if (model.IsExternal)
                model.PersonalNumber = null;

            return (model.IsExternal && string.IsNullOrEmpty(model.PersonalNumber))
                 || (!model.IsExternal && !string.IsNullOrEmpty(model.PersonalNumber));
        }

        public ActionResult UploadFile()
        {
            try
            {
                var file = Request.Files["file1"];
                StreamReader reader = new StreamReader(file.InputStream);
                string text = reader.ReadToEnd();

                List<Models.Employee> notValidList = null;

                var bl = new BusinessLogic();
                var validEmployees = bl.GetValidEmployees(text, out notValidList);
                                
                StringBuilder log = bl.CreateOrUpdateEmployees(validEmployees);
                if (notValidList.Any())
                {
                    log.AppendLine("--- Not valid: ---");
                    foreach (var m in notValidList)
                    {
                        log.AppendLine($"LastName: {m.LastName}; MiddleName: {m.MiddleName}; FirstName: {m.FirstName}; BirthDate: {m.BirthDate}; GenderID: {m.GenderID}; IsExternal: {m.IsExternal}; PersonalNumber: {m.PersonalNumber}");
                    }
                }
                
                byte[] fileBytes = Encoding.UTF8.GetBytes(log.ToString());
                var token = Guid.NewGuid();
                var fileName = $"uploadLog_{token}.txt";
                
                var assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                var logsFolder = Path.Combine(assemblyFolder, "Logs");
                if (!Directory.Exists(logsFolder))
                {
                    Directory.CreateDirectory(logsFolder);
                }

                var filePath = Path.Combine(logsFolder, fileName);
                System.IO.File.WriteAllBytes(filePath, fileBytes);
                return Json(new { Result = 0, Message = token}, JsonRequestBehavior.AllowGet);                
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, Message = $"Errror: {ex.ToString()}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLogFile(Guid id)
        {
            var assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var logsFolder = Path.Combine(assemblyFolder, "Logs");
            var fileName = $"uploadLog_{id}.txt";
            var filePath = Path.Combine(logsFolder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes;
                try
                {
                    fileBytes = System.IO.File.ReadAllBytes(filePath);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error with reading file: {ex.ToString()}";
                    return RedirectToAction("Index");
                }
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
            {
                TempData["ErrorMessage"] = $"File not found: {filePath}";
                return RedirectToAction("Index");
            }
        }

    }
}