using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using System;
using System.Linq;

namespace budget_buddy_trial.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly BudgetDbContext _context;

        public FeedbackController(BudgetDbContext context)
        {
            _context = context;
        }

        // GET: Feedback and Announcements Page
        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Log");
            }

            // Fetch active announcements
            var announcements = _context.AdminAnnouncements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.PostedDate)
                .Take(10)
                .ToList();

            // Fetch user's previous feedback
            var userFeedback = _context.UserFeedbacks
                .Where(f => f.UserId == userId.Value)
                .OrderByDescending(f => f.SubmissionDate)
                .ToList();

            var viewModel = new AdminAnnouncementViewModel
            {
                Announcements = announcements,
                Feedbacks = userFeedback.Select(f => new UserFeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    Title = f.Title,
                    Content = f.Content,
                    SubmissionDate = f.SubmissionDate,
                    Status = f.Status
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SubmitFeedback(string title, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Log");
            }

            var user = _context.Users.Find(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Log");
            }

            // Check if the user has already submitted 3 feedbacks today
            var today = DateTime.Today;
            var feedbackCountToday = _context.UserFeedbacks
                .Where(f => f.UserId == userId.Value && f.SubmissionDate >= today)
                .Count();

            if (feedbackCountToday >= 3)
            {
                TempData["ErrorMessage"] = "You can only submit up to 3 feedbacks per day.";
                return RedirectToAction("Index");
            }

            var feedback = new UserFeedback
            {
                Title = title,
                Content = content,
                UserId = userId.Value,
                SubmissionDate = DateTime.Now,
                Status = FeedbackStatus.Pending
            };

            _context.UserFeedbacks.Add(feedback);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your feedback has been submitted successfully!";
            return RedirectToAction("Index");
        }
    }
}