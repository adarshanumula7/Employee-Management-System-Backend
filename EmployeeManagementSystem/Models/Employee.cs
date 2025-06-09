namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int employee_id { get; set; }
        public string employee_name { get; set; } = "";
        public string? password { get; set; }
        public string gender { get; set; } = "";
        public DateTime dob { get; set; }
        public string email { get; set; } = "";
        public string? phone { get; set; } 
        public decimal? salary { get; set; } 
        public string? role { get; set; } 
        public DateTime? job_starting_date { get; set; } 
        public int? isactive { get; set; } 
    }
}
