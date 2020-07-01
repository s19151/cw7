using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia7.DTOs.Responses
{
    public class TokenResponse
    {
        public string JWTToken { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
