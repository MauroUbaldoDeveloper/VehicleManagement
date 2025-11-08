using MinimalAPI.Domain.Entities;

namespace MinimalAPI.Domain.Interfaces
{
    public interface IVehicleService
    {
        List<Vehicle> All(int? page = 1, string? name = null, string? mark = null);
        Vehicle? FindById(int id);
        void Include(Vehicle vehicle);
        void Refresh(Vehicle vehicle);
        void Delete(Vehicle vehicle);

    }
}
