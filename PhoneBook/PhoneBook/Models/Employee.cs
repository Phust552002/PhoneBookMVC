using System.Collections.Generic;

namespace PhoneBook.Models
{


    public class Employee
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? WorkingPhone { get; set; }
        public string? HandPhone { get; set; }
        public string? HomePhone { get; set; }
        public int Status { get; set; }
    }
}
