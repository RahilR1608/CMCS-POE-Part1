using System;

namespace CMCS.Prototype.Models
{
    public class Approval
    {
        public Guid ApprovalId { get; set; }
        public Guid ClaimId { get; set; }
        public Claim? Claim { get; set; }

        public Guid ApproverId { get; set; }
        public User? Approver { get; set; }

        public DecisionStatus Decision { get; set; } = DecisionStatus.Pending;
        public DateTime DecisionDate { get; set; } = DateTime.UtcNow;
        public string? Comment { get; set; }
    }
}
