using System;
using System.ComponentModel.DataAnnotations;

namespace CMCS.Prototype.Models
{
    public class ClaimLine
    {
        public Guid ClaimLineId { get; set; }
        [Required] public Guid ClaimId { get; set; }
        public Claim? Claim { get; set; }

        public DateTime Date { get; set; }
        [Range(0, 24)] public decimal Hours { get; set; }
        [Required] public string Activity { get; set; } = "";
        public string? Notes { get; set; }
    }
}
