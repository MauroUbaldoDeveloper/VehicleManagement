namespace MinimalAPI.Domain.ModelViews
{
    public struct Home
    {
        public string Doc { get => "/swagger"; }
        public string Message { get => "Welcome to the Vehicles API – Minimal API"; }
    }
}
