using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utilities;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private IActionResult ValidateModel<T>(T model, params string[] requiredProperties)
        {
            var errors = new List<string>();
            foreach (var propName in requiredProperties)
            {
                var prop = typeof(T).GetProperty(propName);
                if (prop.GetValue(model) == null || IsDefaultValue(prop.GetValue(model)))
                    errors.Add(propName);
            }
            return errors.Count > 0 ? BadRequest($"Missing required fields: {string.Join(", ", errors)}") : null;
        }

        private bool IsDefaultValue(object value)
        {
            return value.Equals(GetDefaultValue(value.GetType()));
        }

        private object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        [Route("login")]
        public IActionResult login([FromBody] LoginRequest loginreq)
        {
            var ConnectionString = _configuration.GetConnectionString("EMDBConn");
            if (String.IsNullOrEmpty(ConnectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found in appsettings.json");
            }

            // Verify the Employee
            bool verify = SqlHelper.verifyEmployee(ConnectionString, loginreq.employee_id, loginreq.password);


            if (verify)
            {
                // Update isactive to 1
                string updateQuery = "UPDATE Employees SET isactive = 1 WHERE employee_id = @employee_id";
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@employee_id", loginreq.employee_id));

                int rowsAffected = SqlHelper.ExecuteNonQuery(ConnectionString, updateQuery, parameters);

                // generate token
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("employee_id", loginreq.employee_id.ToString()),
                    new Claim("password", loginreq.password.ToString())

                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn
                    );
                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { Token = tokenValue, Employee = new { loginreq.employee_id, loginreq.password } });

                //return Ok($"Employee Found : Employee_name = {row["employee_name"]}");
            }
            else
            {
                return Unauthorized("Invalid credentials");
                //return Ok("Invalid Employee");
            }

        }

        [HttpPost]
        [Route("addemployee")]
        [Authorize]
        public IActionResult AddEmployee([FromBody] Employee employee)
        {
            var connectionString = _configuration.GetConnectionString("EMDBConn");
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found");
            }

            // Check if employee already exists
            bool employeeExists = SqlHelper.EmployeeExists(connectionString, employee.employee_id);

            if (employeeExists)
            {
                return Ok("error : Employee ID already Exists");
            }

            // Validate required fields
            var validate = ValidateModel<Employee>(employee, nameof(employee.employee_id), nameof(employee.employee_name),
                nameof(employee.gender), nameof(employee.dob), nameof(employee.email), nameof(employee.password));
            if (validate != null)
            {
                return validate;
            }

            if (employee.isactive == null)
            {
                employee.isactive = 0;
            }


            var (columns, parameters) = DynamicSqlBuilder.BuildDynamicInsertClauses(employee, new List<string>
            {
                nameof(employee.employee_id),
                nameof(employee.employee_name),
                nameof(employee.gender),
                nameof(employee.dob),
                nameof(employee.password),
                nameof(employee.email),
                nameof(employee.phone),
                nameof(employee.salary),
                nameof(employee.role),
                nameof(employee.job_starting_date),
                nameof(employee.isactive)
            });

            // Build query
            string query = $"INSERT INTO Employees ({string.Join(", ", columns)}) " +
                           $"VALUES ({string.Join(", ", columns.Select(c => "@" + c))})";

            int rowsAffected = SqlHelper.ExecuteNonQuery(connectionString, query, parameters);
            return rowsAffected > 0 ? Ok("Employee Added") : StatusCode(500, "No rows affected");
            
        }

        [HttpGet]
        [Route("employeeslist")]
        [Authorize]
        public IActionResult GetEmployees(
        [FromQuery] int? employee_id = null,
        [FromQuery] string employee_name = null,
        [FromQuery] string gender = null,
        [FromQuery] string email = null,
        [FromQuery] DateOnly? dob = null) 
        {
            var connectionString = _configuration.GetConnectionString("EMDBConn");
            // Check if connectionString is NULL
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found");
            }

            // Build dynamic WHERE clause
            var (whereClause, parameters) = DynamicSqlBuilder.BuildWhereClause(employee_id, employee_name, gender, email, dob);

            string query = $"SELECT * FROM Employees {whereClause}";
            var dt = SqlHelper.ExecuteQuery(connectionString, query, parameters);
            var employees = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                employees.Add(new
                {
                    employee_id = row.Field<int>("employee_id"),
                    employee_name = row.Field<string>("employee_name"),
                    password = row.Field<string?>("password"),
                    gender = row.Field<string>("gender"),
                    dob = row.Field<DateTime?>("dob") ,
                    email = row.Field<string>("email"),
                    phone = row.Field<string>("phone"),
                    salary = row.Field<decimal?>("salary"),
                    role = row.Field<string?>("role"),
                    job_starting_date = row.Field<DateTime?>("job_starting_date") ,
                    isactive = row.Field<int?>("isactive")
                });
            }

            return Ok(employees);
        }

        [HttpGet]
        [Route("GetallRoles")]
        [Authorize]
        public IActionResult GetRoles()
        {
            // check connectionString
            var connectionString = _configuration.GetConnectionString("EMDBConn");
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found");
            }

            string query = "SELECT role, priority FROM roles";
            var parameters = new List<SqlParameter>();
            DataTable dt = SqlHelper.ExecuteQuery(connectionString, query, parameters);
            if (dt.Rows.Count > 0)
            {
                var roles = new List<object>();
                foreach (DataRow dr in dt.Rows)
                {
                    roles.Add( new
                    {
                        role = dr.Field<string>("role"),
                        priority = dr.Field<int>("priority")
                    });

                }
                return Ok(roles);

            } else
            {
                return BadRequest("roles Table is Empty");
            }
        }


        [HttpPut]
        [Route("updateemployee")]
        [Authorize]
        public IActionResult UpdateEmployee([FromBody] Employee employee)
        {
            // Validate required fields
            if (employee.employee_id == 0)
            {
                return BadRequest("Employee ID is required");
            }

            // check connectionString
            var connectionString = _configuration.GetConnectionString("EMDBConn");
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found");
            }

            // Check if employee exists
            bool employeeExists = SqlHelper.EmployeeExists(connectionString, employee.employee_id);
            
            if (!employeeExists)
            {
                return NotFound("Employee not found");
            }

            // Build dynamic UPDATE query
            var (setClauses, parameters) = DynamicSqlBuilder.BuildDynamicUpdateClauses(employee, new List<string>
            {
                nameof(employee.employee_id),
                nameof(employee.employee_name),
                nameof(employee.gender),
                nameof(employee.dob),
                nameof(employee.password),
                nameof(employee.email),
                nameof(employee.phone),
                nameof(employee.salary),
                nameof(employee.role),
                nameof(employee.job_starting_date),
                nameof(employee.isactive)
            });

            
            if (setClauses.Count == 0)
            {
                return BadRequest("No fields provided for update");
            }

            string query = $"UPDATE Employees SET {string.Join(", ", setClauses)} WHERE employee_id = @employee_id";

            int rowsAffected = SqlHelper.ExecuteNonQuery(connectionString, query, parameters);
            return rowsAffected > 0 ? Ok("Employee Updated") : StatusCode(500, "No changes made");
            
        }


        [HttpDelete]
        [Route("deleteemployee")]
        [Authorize]
        public IActionResult DeleteEmployee([FromBody] DeleteEmployee request)
        {
            // Validate required fields
            if (request.employee_id == 0)
            {
                return BadRequest("Employee ID is required");
            }

            // check connectionString
            var connectionString = _configuration.GetConnectionString("EMDBConn");
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Connection string 'EMDBConn' not found");
            }

            // Check if employee exists
            bool employeeExists = SqlHelper.EmployeeExists(connectionString, request.employee_id);

            if (!employeeExists)
            {
                return NotFound("Employee not found");
            }

            // Perform deletion
            string deleteQuery = "DELETE FROM Employees WHERE employee_id = @employee_id";
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@employee_id", request.employee_id));
            int rowsAffected = SqlHelper.ExecuteNonQuery(connectionString, deleteQuery, parameters);
            return rowsAffected > 0 ? Ok("Employee Deleted") : StatusCode(500, "Deletion failed");
        }


    }
}
