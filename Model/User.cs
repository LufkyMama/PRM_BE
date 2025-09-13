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
        public DateTime DateOfBirth { get; set; }
        public user_role Role { get; set; }    

    }
    public enum user_role
    {
        Admin=1,
        User=2,
        Staff=3
    }
}
