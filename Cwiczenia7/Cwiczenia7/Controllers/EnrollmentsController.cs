using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cwiczenia7.DTOs.Requests;
using Cwiczenia7.Models;
using Cwiczenia7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia7.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public EnrollmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            var response = _dbService.PromoteStudents(request);

            if (response == null)
                return NotFound();

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            Enrollment en;

            try
            {
                en = _dbService.EnrollStudent(request);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            return Ok(en);
        }
    }
}