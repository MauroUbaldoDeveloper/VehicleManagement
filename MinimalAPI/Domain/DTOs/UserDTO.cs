using MinimalAPI.Domain.DTOs.Enums;

namespace MinimalAPI.Domain.DTOs
{
    public class UserDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Profile? Profile { get; set; } = default!;
    }
}
