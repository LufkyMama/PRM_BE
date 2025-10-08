using Microsoft.AspNetCore.Identity;
using PRM_BE.Model.Enums;

namespace PRM_BE.Model
{
    public class User
    {
        public int Id { get; set; }
        public String UserName { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public String FirstName { get; set; }   
        public String LastName { get; set; }
        public String PhoneNumber { get; set; }    
        public String Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
