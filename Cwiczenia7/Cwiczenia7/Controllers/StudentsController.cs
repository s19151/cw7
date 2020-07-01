using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Cwiczenia7.DTOs.Requests;
using Cwiczenia7.DTOs.Responses;
using Cwiczenia7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cwiczenia7.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            var studentsList = _dbService.GetStudents();

            if (studentsList.Count() == 0)
                return NoContent();

            return Ok(studentsList);
        }

        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            var student = _dbService.GetStudent(index);

            if (student == null)
                return NotFound();

            return Ok(student);
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            var response = _dbService.Login(request);

            if (response == null)
                return Unauthorized();

            return Ok(response);
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken(RefreshTokenRequest request)
        {
            var response = _dbService.RefreshToken(request);

            if (response == null)
                return Unauthorized();

            return Ok(response);
        }

        [HttpPost("createPassword")]
        public IActionResult CreatePassword(LoginRequest request)
        {
            var isCreated = _dbService.CreatePassword(request);

            if (isCreated)
                return Ok();

            return NotFound();
        }
    }
}