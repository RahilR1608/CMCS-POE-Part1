using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMCS.Prototype.Models
{
    public class Claim
    {
        public Guid ClaimId { get; set; }
        [Required] public Guid LecturerId { get; set; }
        public User? Lecturer { get; set; }

        [Range(1, 12)] public int Month { get; set; }
        public int Year { get; set; }
        [Range(0, 5000)] public decimal HoursWorked { get; set; }
        [Range(0, 5000)] public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public DecisionStatus Status { get; set; } = DecisionStatus.Pending;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<ClaimLine> Lines { get; set; } = new List<ClaimLine>();
        public ICollection<SupportingDocument> Documents { get; set; } = new List<SupportingDocument>();
        public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}
