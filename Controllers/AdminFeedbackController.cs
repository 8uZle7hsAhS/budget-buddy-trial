using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using System;
using System.Linq;

namespace budget_buddy_trial.Controllers
{
    public class AdminFeedbackController : Controller
    {
        private readonly BudgetDbContext _context;

        public AdminFeedbackController(BudgetDbContext context)
        {
            _context = context;
        }

        // GET: Admin Feedback and Announcements Page
        public IActionResult Index()
        {
            var viewModel = new AdminAnnouncementViewModel
            {
                // Get all feedbacks with user details
                Feedbacks = _context.UserFeedbacks
                    .Include(f => f.User)
                    .Select(f => new UserFeedbackViewModel
                    {
                        FeedbackId = f.FeedbackId,
                        Title = f.Title,
                        Content = f.Content,
                        Username = f.User.Username,
                        Email = f.User.Email,
                        SubmissionDate = f.SubmissionDate,
                        Status = f.Status
                    })
                    .OrderByDescending(f => f.SubmissionDate)
                    .ToList(),

                // Get all announcements
                Announcements = _context.AdminAnnouncements
                    .OrderByDescending(a => a.PostedDate)
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateAnnouncement(string title, string content)
        {
            var announcement = new AdminAnnouncement
            {
                Title = title,
                Content = content,
                PostedDate = DateTime.Now,
                IsActive = true
            };

            _context.AdminAnnouncements.Add(announcement);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Announcement created successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateFeedbackStatus(int feedbackId, FeedbackStatus status)
        {
            var feedback = _context.UserFeedbacks.Find(feedbackId);
            if (feedback == null)
            {
                return NotFound();
            }

            feedback.Status = status;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Feedback status updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteAnnouncement(int announcementId)
        {
            var announcement = _context.AdminAnnouncements.Find(announcementId);
            if (announcement == null)
            {
                return NotFound();
            }

            _context.AdminAnnouncements.Remove(announcement);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Announcement deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}