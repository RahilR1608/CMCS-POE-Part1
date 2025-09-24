using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace CMCS.Prototype.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public UserRole Role { get; set; }
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}
