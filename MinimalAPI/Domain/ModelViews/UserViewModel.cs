using MinimalAPI.Domain.DTOs.Enums;

namespace MinimalAPI.Domain.ModelViews
{
    public record UserViewModel
    {
        public int Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Profile { get; set; } = default!;
    }
}
