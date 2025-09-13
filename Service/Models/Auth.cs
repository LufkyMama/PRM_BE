using PRM_BE.Model;

namespace PRM_BE.Service.Models
{
    public class Auth
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
