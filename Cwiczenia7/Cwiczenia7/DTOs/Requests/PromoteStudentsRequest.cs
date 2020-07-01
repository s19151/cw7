using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia7.DTOs.Requests
{
    public class PromoteStudentsRequest
    {
        public String Studies { get; set; }
        public int Semester { get; set; }
    }
}
