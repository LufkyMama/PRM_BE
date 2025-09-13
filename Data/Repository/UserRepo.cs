namespace PRM_BE.Data.Repository
{
    public class UserRepo
    {
       private readonly Model.AppDbContext _context;
        public UserRepo(Model.AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Model.User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public Model.User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }
        public void AddUser(Model.User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public void UpdateUser(Model.User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
