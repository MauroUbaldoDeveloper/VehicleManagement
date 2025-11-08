namespace MinimalAPI.Domain.ModelViews
{
    public class UserLogged
    {
        public string Token { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Profile { get; set; } = default!;
    }
}
