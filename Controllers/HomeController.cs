using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private Context dbContext;

        public HomeController(Context context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                return RedirectToAction("Login");
            }
            return View("Index");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginUser(Login user)
        {
            if (ModelState.IsValid)
            {
                User dbUser = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                if (dbUser != null)
                {
                    PasswordHasher<Login> Hasher = new PasswordHasher<Login>();
                    if ((Hasher.VerifyHashedPassword(user, dbUser.Password, user.Password)) != 0)
                    {
                        HttpContext.Session.SetInt32("UserId", dbUser.UserId);
                        return RedirectToAction("Account");
                    }
                }
            }
            ModelState.AddModelError("Email", "Invalid Email/Password");
            return View("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Account()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                ViewBag.LoggedUser = dbContext.Users
                    .Include(user => user.CreatedTransactions)
                    .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("UserId"));

                ViewBag.Balance = 0;
                foreach (var transaction in ViewBag.LoggedUser.CreatedTransactions)
                {
                    ViewBag.Balance += transaction.Amount;
                }

                return View();
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult CreateTransaction(Transaction transaction)
        {
            if (ModelState.IsValid && transaction.UserId == HttpContext.Session.GetInt32("UserId"))
            {
                Console.WriteLine("\n\n\n PASSED FORM VALIDATION\n\n");


                User LoggedUser = dbContext.Users
                    .Include(user => user.CreatedTransactions)
                    .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("UserId"));

                decimal Balance = 0;
                foreach (var entry in LoggedUser.CreatedTransactions)
                {
                    Balance += entry.Amount;
                }

                if(Balance + transaction.Amount >= 0)
                {
                    Console.WriteLine("\n\n\n PASSED BALANCE VALIDATION\n\n");
                    dbContext.Transactions.Add(transaction);
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
