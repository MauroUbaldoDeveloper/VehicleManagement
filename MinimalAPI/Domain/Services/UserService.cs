using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Infrastructure;

namespace MinimalAPI.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;
        public UserService(DatabaseContext dbContext)
        {
            _context = dbContext;
        }

        public List<User> All(int? page)
        {
            var query = _context.users.AsQueryable();
            
            int itensPerPage = 10;

            if (page != null)
                query = query.Skip(((int)page - 1) * itensPerPage).Take(itensPerPage);

            return query.ToList();
        }

        public User Include(User user)
        {
            _context.users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public User? Login(LoginDTO loginDTO)
        {
            var adm = _context.users.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).ToList().FirstOrDefault();
            return adm;   
        }

        public User? FindById(int? id)
        {
            return _context.users.Where(v => v.Id == id).FirstOrDefault();
        }


    }
}
