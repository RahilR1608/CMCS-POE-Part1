using System;
using System.ComponentModel.DataAnnotations;   // <-- important

namespace CMCS.Prototype.Models
{
    public class SupportingDocument
    {
        [Key]                                 // <-- PK
        public Guid DocumentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }
        public Claim? Claim { get; set; }

        [Required]
        public string FileName { get; set; } = "";

        [Required]
        public string FilePath { get; set; } = ""; // e.g. /uploads/xyz.pdf

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
