using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace budget_buddy_trial.Controllers
{
    public class LogController : Controller
    {
        private readonly string _connectionString = "Data Source=BudgetBuddy.db";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string email, string password)
        {
            // Check if the user is the admin
            if (email == "admin@budgetbuddy.com" && password == "thebudgetbuddyadmin")
            {
                // Redirect to the admin dashboard
                return RedirectToAction("BudgetAnalysis", "Admin");
            }

            // Validate if the user exists and credentials are correct
            if (ValidateUser(email, password))
            {
                // Fetch the UserId
                int userId = GetUserId(email, password);

                if (userId > 0)
                {
                    // Store UserId and Email in session
                    HttpContext.Session.SetInt32("UserId", userId);
                    HttpContext.Session.SetString("Email", email);

                    // Redirect to the user dashboard
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle unexpected issues (e.g., missing UserId in database)
                    ModelState.AddModelError("", "An error occurred. Please try again.");
                    return View("Index");
                }
            }
            else
            {
                // If credentials are invalid, return to the login page with an error
                ModelState.AddModelError("", "Invalid email or password.");
                return View("Index");
            }
        }

        private int GetUserId(string email, string password)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT UserId
                    FROM Users
                    WHERE Email = @email AND Password = @password";
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);

                var result = command.ExecuteScalar();
                return result == null ? -1 : Convert.ToInt32(result);
            }
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            if (UserExists(email))
            {
                ModelState.AddModelError("", "Email already exists. Please choose a different email.");
                return View("Index");
            }

            if (RegisterUser(username, email, password))
            {
                TempData["SuccessMessage"] = "Registration successful! Please sign in.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View("Index");
            }
        }

        private bool ValidateUser(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    SELECT COUNT(1) 
                    FROM Users 
                    WHERE Email = @email AND Password = @password";

                        var emailParam = command.CreateParameter();
                        emailParam.ParameterName = "@email";
                        emailParam.Value = email;
                        command.Parameters.Add(emailParam);

                        var passwordParam = command.CreateParameter();
                        passwordParam.ParameterName = "@password";
                        passwordParam.Value = password;
                        command.Parameters.Add(passwordParam);

                        if (command.Parameters.Count != 2)
                        {
                            throw new InvalidOperationException("Parameters not properly added");
                        }

                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                return false;
            }
        }

        private bool UserExists(string email)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(1) 
                    FROM Users 
                    WHERE Email = @email";
                command.Parameters.AddWithValue("@email", email);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private bool RegisterUser(string username, string email, string password)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Users (Username, Email, Password) 
                    VALUES (@username, @email, @password)";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);

                return command.ExecuteNonQuery() > 0;
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Clear the session to log out the user
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Index");
        }
    }
}