using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.Interfaces;

namespace MinimalAPITest.Mocks
{
    public class VehicleServiceMock : IVehicleService
    {

        public static List<Vehicle> _vehicles = new List<Vehicle>()
        {
            new Vehicle
            {
                Id = 1,
                Name = "ZX-4R",
                Mark = "Kawasaki",
                year = 2025
            },

            new Vehicle
            {
                Id = 2,
                Name = "S1000RR",
                Mark = "BMW",
                year = 2025
            },
        };

        public List<Vehicle> All(int? page, string? name = null, string? mark = null)
        {
            //Items per page
            const int pageSize = 10;
            var query = _vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(v => v.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(mark))
                query = query.Where(v => v.Mark.Contains(mark, StringComparison.OrdinalIgnoreCase));

            if (page.HasValue && page > 0)
                query = query.Skip((page.Value - 1) * pageSize).Take(pageSize);
            else
                query = query.Take(pageSize);

            return query.ToList();
        }

        public void Delete(Vehicle vehicle)
        {
            _vehicles.Remove(vehicle);
        }

        public Vehicle? FindById(int id)
        {
            return _vehicles.Find(a => a.Id == id);
        }

        public void Include(Vehicle vehicle)
        {
            vehicle.Id = _vehicles.Count() + 1;
            _vehicles.Add(vehicle);
        }

        public void Refresh(Vehicle vehicle)
        {
            var vehicleToRefresh = _vehicles.Find(a => a.Id == vehicle.Id);
            vehicleToRefresh = vehicle;
        }
    }
}
