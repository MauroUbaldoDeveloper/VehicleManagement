using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entities;

namespace MinimalAPI.Domain.Interfaces
{
    public interface IUserService
    {
        User? Login(LoginDTO loginDTO);
        User Include(User user);
        User? FindById(int? id);
        List<User> All(int? page);

    }
}
