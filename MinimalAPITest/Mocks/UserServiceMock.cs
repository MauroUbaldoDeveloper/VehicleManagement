using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.Interfaces;

namespace MinimalAPITest.Mocks
{
    public class UserServiceMock : IUserService
    {

        private static List<User> _users = new List<User>() { 
            new User 
            { 
                Id = 1, 
                Email = "adm@test.com", 
                Password ="123456", 
                Profile = "ADM"
            },
            new User
            {
                Id = 2,
                Email = "editor@test.com",
                Password ="123456",
                Profile = "EDITOR"
            }
        };

        public List<User> All(int? page)
        {
            //Items per page
            const int pageSize = 10;
            var query = _users.AsQueryable();

            if (page.HasValue && page > 0)
                query = query.Skip((page.Value - 1) * pageSize).Take(pageSize);
            else
                query = query.Take(pageSize);

            return query.ToList();
        }

        public User? FindById(int? id)
        {
            return _users.Find(a => a.Id == id);
        }

        public User Include(User user)
        {
            user.Id = _users.Count() + 1;
            _users.Add(user);
            return user;
        }

        public User? Login(LoginDTO loginDTO)
        {
            return _users.Find(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password);           
        }
    }
}
