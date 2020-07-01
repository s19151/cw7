using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia7.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression("^s[0-9]+$")]
        public String IndexNumber { get; set; }
        [Required]
        [MaxLength(100)]
        public String FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public String LastName { get; set; }
        [Required]
        [MaxLength(100)]
        public String BirthDate { get; set; }
        [Required]
        [MaxLength(100)]
        public String Studies { get; set; }
    }
}
