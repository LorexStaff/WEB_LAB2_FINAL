using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB_LAB2_FINAL.Models.Entities;
using WEB_LAB2_FINAL.Models.ViewModel;

namespace WEB_LAB2_FINAL.Controllers
{
    public class LAB2Controller : Controller
    {
        // GET: LAB2
        [AllowAnonymous]
        public ActionResult ListOfPeople()
        {
            List<Person> people = new List<Person>();
            using (var db = new WEB_LAB5Entities())
            {
                people = db.Person.OrderByDescending(x => x.Age)
                    .ThenBy(x => x.LastName)
                    .ThenBy(x => x.FirstName).ToList();
            }
            return View(people);
        }
        [HttpGet]
        [Authorize]
        public ActionResult PersonDetails(Guid personId)
        {
            Person model = new Person();
            using (var db = new WEB_LAB5Entities())
            {
                model = db.Person.Find(personId);
            }
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CreatePerson()
        {
            ViewBag.Genders = new SelectList(GetGendersList(), "Item1", "“Item2");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePerson(PersonVM newPerson)
        {
            if (ModelState.IsValid)
            {
                using (var context = new WEB_LAB5Entities())
                {
                    Guid salt = Guid.NewGuid();
                    string RandomPassword = GenerateRandomPassword(5);
                    // Создание записи в таблице User
                    User user = new User
                    {
                        ID = Guid.NewGuid(),
                        Login = GenerateRandomLogin(6),
                        PasswordHash = ReturnHashCode(RandomPassword + salt.ToString().ToUpper()),
                        Salt = salt,
                        UserRole = 2
                    };
                    context.User.Add(user);
                    context.SaveChanges();

                    // Получение ID только что созданной записи в таблице User
                    Guid userId = user.ID;

                    // Создание записи в таблице Person с использованием userId
                    Person person = new Person
                    {
                        Id = Guid.NewGuid(),
                        LastName = newPerson.LastName,
                        FirstName = newPerson.FirstName,
                        Patronymic = newPerson.Patronymic,
                        Gender = newPerson.Gender,
                        Age = newPerson.Age,
                        HasJob = newPerson.HasJob,
                        Birthday = (DateTime)newPerson.Birthday,
                        InsertedDateTime = (DateTime)newPerson.InsertedDateTime,
                        UserId = userId // Использование userId для связи с таблицей User
                    };
                    context.Person.Add(person);
                    context.SaveChanges();
                }
                return RedirectToAction("ListOfPeople");
            }
            return View(newPerson);
        }
        List<Tuple<string, string>> GetGendersList()

        {
            List<Tuple<string, string>> genders = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("ж", "Женский"),
                new Tuple<string, string>("м", "Мужской")
            };
            return genders;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditPerson(Guid personID)
        {
            PersonVM model;
            using (var context = new WEB_LAB5Entities())
            {
                Person person = context.Person.Find(personID);
                model = new PersonVM
                {
                    Id = person.Id,
                    LastName = person.LastName,
                    FirstName = person.FirstName,
                    Patronymic = person.Patronymic,
                    Gender = person.Gender,
                    Age = person.Age,
                    HasJob = person.HasJob,
                    Birthday = (DateTime)person.Birthday,
                    InsertedDateTime = (DateTime)person.InsertedDateTime,
                    UserId = person.UserId,
                };
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditPerson(PersonVM model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new WEB_LAB5Entities())
                {
                    // Получить редактируемый объект Person из базы данных
                    Person editedPerson = context.Person.FirstOrDefault(p => p.Id == model.Id);

                    // Установить новые значения полей
                    editedPerson.LastName = model.LastName;
                    editedPerson.FirstName = model.FirstName;
                    editedPerson.Patronymic = model.Patronymic;
                    editedPerson.Gender = model.Gender;
                    editedPerson.Age = model.Age;
                    editedPerson.HasJob = model.HasJob;
                    editedPerson.Birthday = (DateTime)model.Birthday;
                    editedPerson.InsertedDateTime = (DateTime)model.InsertedDateTime;

                    // Получить пользователя, которому принадлежит редактируемый объект Person
                    User user = context.User.FirstOrDefault(u => u.ID == model.UserId);

                    // Проверить, существует ли пользователь с заданным ID
                    if (user != null)
                    {
                        // Установить внешний ключ UserId для связи с таблицей User
                        editedPerson.UserId = user.ID;
                    }

                    // Сохранить изменения в базе данных
                    context.SaveChanges();
                }

                return RedirectToAction("ListOfPeople");
            }

            return View(model);
        }
        [HttpGet]
        public ActionResult DeletePerson(Guid personID)
        {
            Person personToDelete;
            using (var context = new WEB_LAB5Entities())
            {
                personToDelete = context.Person.Find(personID);
            }
            return View(personToDelete);
        }
        [HttpPost, ActionName("DeletePerson")]
        public ActionResult DeleteConfirmed(Guid personID)
        {
            using (var context = new WEB_LAB5Entities())
            {
                Person personToDelete = new Person
                {
                    Id = personID
                };
                context.Entry(personToDelete).State = System.Data.Entity.EntityState.Deleted;
                context.SaveChanges();
            }
            return RedirectToAction("ListOfPeople");
        }
        [ChildActionOnly]
        public ActionResult QuestionAnswered(Guid personID)
        {
            string message = "";
            using (var context = new WEB_LAB5Entities())
            {
                int questionAnsweredNumber = context.Person.Find(personID).Answer.Count;
                message = $"Вопросов отвечено: {questionAnsweredNumber}.";
            }
            return PartialView("QuestionAnswered", message);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserVM webUser)
        {
            if (ModelState.IsValid)
            {
                using (WEB_LAB5Entities context = new WEB_LAB5Entities())
                {
                    User user = null;
                    user = context.User.Where(u => u.Login == webUser.Login).FirstOrDefault();
                    if (user != null)
                    {
                        string passwordHash = ReturnHashCode(webUser.Password + user.Salt.ToString().ToUpper());
                        if (passwordHash == user.PasswordHash)
                        {
                            string userRole = "";
                            switch (user.UserRole)
                            {
                                case 1:
                                    userRole = "Admin";
                                    break;
                                case 2:
                                    userRole = "Participant";
                                    break;
                            }
                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                            1,
                            user.Login,
                            DateTime.Now,
                            DateTime.Now.AddDays(1),
                            false,
                            userRole
                            );

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));
                        }
                        return RedirectToAction("ListOfPeople", "LAB2");
                    }
                }
            }
            ViewBag.Error = "Пользователя с таким логином и паролем не существует, попробуйте еще";
            return View(webUser);
        }
        string ReturnHashCode(string loginAndSalt)
        {
            string hash = "";
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(loginAndSalt));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                { sBuilder.Append(data[i].ToString("x2")); }
                hash = sBuilder.ToString().ToUpper();
            }    
            return hash;
        }
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }
        static string GenerateRandomLogin(int length)
        {
            Random random = new Random();
            char[] result = new char[length];
            string allowedChars = "1234567890_#$";
            for (int i = 0; i < length; i++)
            {
                result[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }
            string final = "User" + new String(result);
            return final;
        }
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("ListOfPeople", "LAB2");
        }
       
    }
}