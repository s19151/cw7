using Cwiczenia7.DTOs.Requests;
using Cwiczenia7.DTOs.Responses;
using Cwiczenia7.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cwiczenia7.Services
{
    public class StudentsDbService : IDbService
    {
        private readonly string _dbConString = "Data Source=db-mssql;Initial Catalog=s19151;Integrated Security=True";
        private readonly IConfiguration _configuration;

        public StudentsDbService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CheckIfStudentExists(string index)
        {
            bool exists = false;

            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT 1 FROM Student WHERE IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);

                var dr = com.ExecuteReader();
                if (dr.Read())
                    exists = true;
            }

            return exists;
        }

        public Student GetStudent(string index)
        {
            Student student = null;

            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT * FROM Student WHERE IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);

                var dr = com.ExecuteReader();
                if (dr.Read())
                {
                    student = new Student();

                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    student.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());
                }
            }

            return student;
        }

        public List<Student> GetStudents()
        {
            var studentsList = new List<Student>();

            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT * FROM Student";

                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var student = new Student();

                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    student.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());

                    studentsList.Add(student);
                }
            }

            return studentsList;
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request)
        {
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s19151;Integrated Security=True"))
            using (var com = con.CreateCommand())
            {
                con.Open();
                var tran = con.BeginTransaction("PromoteStudentTrans");

                com.Connection = con;
                com.Transaction = tran;

                com.CommandText = $"SELECT 1 FROM Enrollment " +
                    $"INNER JOIN Studies ON Studies.IdStudy = Enrollment.IdStudy " +
                    $"WHERE Name = @studies AND semester = @semester";
                com.Parameters.AddWithValue("studies", request.Studies);
                com.Parameters.AddWithValue("semester", request.Semester);

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    tran.Rollback();

                    return null;
                }
                dr.Close();

                com.CommandText = "promotestudents @studies, @semester";

                var returnParam = new SqlParameter("returnVal", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;
                com.Parameters.Add(returnParam);

                com.ExecuteNonQuery();

                int idEnroll = Convert.ToInt32(returnParam.Value);

                com.CommandText = "SELECT * FROM Enrollment WHERE IdEnrollment = @idEnroll";
                com.Parameters.AddWithValue("idEnroll", idEnroll);

                dr = com.ExecuteReader();
                Enrollment enrollment = new Enrollment();

                enrollment.IdEnrollment = idEnroll;
                enrollment.Semester = Int32.Parse(dr["Semester"].ToString());
                enrollment.IdStudy = Int32.Parse(dr["IdStudy"].ToString());
                enrollment.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                tran.Commit();

                return enrollment;
            }
        }

        public Enrollment EnrollStudent(EnrollStudentRequest request)
        {
            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                con.Open();
                var tran = con.BeginTransaction("EnrollStudentTrans");

                com.Transaction = tran;
                com.Connection = con;

                com.CommandText = "SELECT IdStudy FROM Studies WHERE Name = @studies";
                com.Parameters.AddWithValue("studies", request.Studies);

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    tran.Rollback();

                    throw new Exception("Studia nie istnieją");
                }
                int idStudies = (int)dr["IdStudy"];
                dr.Close();


                com.CommandText = "SELECT 1 FROM Student WHERE IndexNumber = @index";
                com.Parameters.AddWithValue("index", request.IndexNumber);

                dr = com.ExecuteReader();
                if (dr.Read())
                {
                    dr.Close();
                    tran.Rollback();

                    throw new Exception("Podano zły numer indeksu");
                }
                dr.Close();

                com.CommandText = "SELECT * FROM Enrollment WHERE IdStudy = @idStudy AND Semester = 1";
                com.Parameters.AddWithValue("IdStudy", idStudies);

                Enrollment en = new Enrollment();

                dr = com.ExecuteReader();
                if (dr.Read())
                {
                    en.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());
                    en.Semester = Int32.Parse(dr["Semester"].ToString());
                    en.IdStudy = Int32.Parse(dr["IdStudy"].ToString());
                    en.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                }
                else
                {
                    dr.Close();

                    com.CommandText = "SELECT Max(IdEnrollment) FROM Enrollment";
                    dr = com.ExecuteReader();

                    en.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString()) + 1;
                    en.Semester = 1;
                    en.IdStudy = idStudies;
                    en.StartDate = DateTime.Now.Date;

                    com.CommandText = "INSERT INTO Enrollment VALUES(@idEnroll, @semester, @idStudy, Convert(date, @startdate, 103))";
                    com.Parameters.AddWithValue("idEnroll", en.IdEnrollment);
                    com.Parameters.AddWithValue("semester", en.Semester);
                    com.Parameters.AddWithValue("startdate", en.StartDate);

                    com.ExecuteNonQuery();
                }
                dr.Close();

                com.CommandText = "INSERT INTO Student VALUES(@index, @fName, @lName, Convert(date, @bDate, 103), @idEnroll)";
                com.Parameters.AddWithValue("fName", request.FirstName);
                com.Parameters.AddWithValue("lName", request.LastName);
                com.Parameters.AddWithValue("bDate", request.BirthDate);

                com.ExecuteNonQuery();
                tran.Commit();

                return en;
            }
        }

        public bool CreatePassword(LoginRequest request)
        {
            string salt = IDbService.CreateSalt();
            string password = IDbService.Create(request.Password, salt);

            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                con.Open();
                var tran = con.BeginTransaction("CreatePassword");

                com.Connection = con;
                com.Transaction = tran;

                try
                {
                    com.CommandText = $"UPDATE Student SET Salt = @salt, Password = @password " +
                        "WHERE IndexNumber = @index";
                    com.Parameters.AddWithValue("salt", salt);
                    com.Parameters.AddWithValue("password", password);
                    com.Parameters.AddWithValue("index", request.Index);

                    com.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();

                    return false;
                }

                return true;
            }
        }

        public TokenResponse Login(LoginRequest request)
        {
            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT * FROM Student WHERE IndexNumber = @index";
                com.Parameters.AddWithValue("index", request.Index);

                string password;
                string salt;
                string role;

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();

                    return null;
                }

                password = dr["Password"].ToString();
                salt = dr["Salt"].ToString();
                role = dr["Role"].ToString();

                dr.Close();

                var checkPassword = IDbService.Validate(request.Password, salt, password);
                if (!checkPassword)
                    return null;

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, request.Index),
                    new Claim(ClaimTypes.Role, role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwtToken = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                var response = new TokenResponse();
                response.JWTToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                response.RefreshToken = Guid.NewGuid();

                com.CommandText = "UPDATE Student SET RefreshToken = @token where IndexNumber = @index";
                com.Parameters.AddWithValue("token", response.RefreshToken);
                com.ExecuteNonQuery();

                return response;
            }

        }

        public TokenResponse RefreshToken(RefreshTokenRequest request)
        {
            using (var con = new SqlConnection(_dbConString))
            using (var com = new SqlCommand())
            {
                con.Open();
                var tran = con.BeginTransaction("RefreshToken");

                com.Connection = con;
                com.Transaction = tran;

                com.CommandText = "SELECT IndexNumber, Role FROM Student WHERE RefreshToken = @token";
                com.Parameters.AddWithValue("token", request.refreshToken);

                string index;
                string role;

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();

                    return null;
                }

                index = dr["IndexNumber"].ToString();
                role = dr["Role"].ToString();

                dr.Close();

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, index),
                    new Claim(ClaimTypes.Role, role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwtToken = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                var response = new TokenResponse();
                response.JWTToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                response.RefreshToken = Guid.NewGuid();

                com.CommandText = "UPDATE Student SET RefreshToken = @token where IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);
                com.ExecuteNonQuery();

                return response;
            }
        }
    }
}
