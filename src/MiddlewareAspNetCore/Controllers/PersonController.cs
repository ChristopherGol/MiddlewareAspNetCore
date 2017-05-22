using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiddlewareAspNetCore.Model;

namespace MiddlewareAspNetCore.Controllers
{
    public class PersonController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string name, string surname, int age)
        {
            Person person = new Person()
            {
                Name = name,
                Surname = surname,
                Age = age
            };

            return View(person);
        }
    }
}
