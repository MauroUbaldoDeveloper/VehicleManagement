namespace MinimalAPI.Domain.DTOs
{
    public record VehicleDTO
    {
        public string Name { get; set; } = default!;
        public string Mark { get; set; } = default!;
        public int year { get; set; } = default!;

    }
}
