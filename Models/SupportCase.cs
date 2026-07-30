using System;

namespace SmartKart.Models
{
    public class SupportCase
    {
        public int Id { get; set; }

        public string CaseTitle { get; set; } = string.Empty;

        public string CaseType { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Open";

        public string Resolution { get; set; } = string.Empty;

        public string Owner { get; set; } = "Support Team";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

