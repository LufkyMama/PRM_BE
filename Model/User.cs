namespace PRM_BE.Model
{
    public class User
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public String FirstName { get; set; }   
        public String LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public String Role { get; set; }    

    }
}
