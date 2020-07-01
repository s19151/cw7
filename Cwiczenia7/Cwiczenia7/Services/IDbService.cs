using Cwiczenia7.DTOs.Requests;
using Cwiczenia7.DTOs.Responses;
using Cwiczenia7.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cwiczenia7.Services
{
    public interface IDbService
    {
        public bool CheckIfStudentExists(string index);
        public List<Student> GetStudents();
        public Student GetStudent(string index);
        public Enrollment PromoteStudents(PromoteStudentsRequest request);
        public Enrollment EnrollStudent(EnrollStudentRequest request);

        public bool CreatePassword(LoginRequest request);
        public TokenResponse Login(LoginRequest request);
        public TokenResponse RefreshToken(RefreshTokenRequest request);

        public static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2
                (
                    password: value,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 40_000,
                    numBytesRequested: 256 / 8
                );

            return Convert.ToBase64String(valueBytes);
        }
        public static bool Validate(string value, string salt, string hash)
            => Create(value, salt) == hash;
        public static string CreateSalt()
        {
            var randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
