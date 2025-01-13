using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class Feedback
    {
        public int FeedbackId { get; set; }
        public int CustomerId { get; set; }
        public int HomestayId { get; set; }
        public string? FeedbackContent { get; set; }
        public int? Rating { get; set; }
        public DateTime FeedbackDate { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Homestay Homestay { get; set; } = null!;
    }
}
