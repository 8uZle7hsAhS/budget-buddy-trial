using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace budget_buddy_trial.Models
{
    public class UserFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required(ErrorMessage = "Feedback title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Feedback content is required")]
        [StringLength(1000, ErrorMessage = "Feedback cannot be longer than 1000 characters")]
        public string Content { get; set; }

        // Foreign key for User
        public int UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public LogModel User { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    }

    public class AdminAnnouncement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Announcement title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Announcement content is required")]
        [StringLength(2000, ErrorMessage = "Announcement cannot be longer than 2000 characters")]
        public string Content { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }

    public enum FeedbackStatus
    {
        Pending,
        InProgress,
        Resolved,
        Closed
    }

    // View Models
    public class UserFeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime SubmissionDate { get; set; }
        public FeedbackStatus Status { get; set; }
    }

    public class AdminAnnouncementViewModel
    {
        public List<AdminAnnouncement> Announcements { get; set; }
        public List<UserFeedbackViewModel> Feedbacks { get; set; }
    }
}