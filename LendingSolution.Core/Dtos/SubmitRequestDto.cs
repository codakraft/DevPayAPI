using System;
using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class SubmitRequestDto
    {
        [Required]
        public double Amount { get; set; }
        [Required]
        public int Tenor { get; set; }
    }
}
