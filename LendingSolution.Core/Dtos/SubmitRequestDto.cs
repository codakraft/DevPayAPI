using System;
using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class SubmitRequestDto
    {
        [Required]
        public Guid LoanId { get; set; }
    }
}
