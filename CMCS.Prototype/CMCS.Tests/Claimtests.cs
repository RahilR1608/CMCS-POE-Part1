using CMCS.Prototype.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CMCS.Tests
{
    public class ClaimTests
    {
        [Fact]
        public void TotalAmount_ComputedCorrectly()
        {
            var c = new Claim { HoursWorked = 12m, HourlyRate = 300m };
            Assert.Equal(3600m, c.HoursWorked * c.HourlyRate);
        }

        [Fact]
        public void Hours_CannotBeNegative()
        {
            var model = new Claim { LecturerId = Guid.NewGuid(), Month = 9, Year = 2025, HoursWorked = -1m, HourlyRate = 300m };
            var ctx = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(model, ctx, results, true);
            Assert.False(valid);
        }

        [Fact]
        public void Status_Updates_OnDecision()
        {
            var c = new Claim { Status = DecisionStatus.Pending };
            c.Status = DecisionStatus.Approved;
            Assert.Equal(DecisionStatus.Approved, c.Status);
        }
    }
}
