using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Infrastructure;

namespace MinimalAPI.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly DatabaseContext _context;

        public VehicleService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Vehicle> All(int? page = 1, string? name = null, string? mark = null)
        {
            var query = _context.vehicles.AsQueryable();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name.ToLower()}%"));

            int itensPerPage = 10;

            if (page != null)
                query = query.Skip(((int)page - 1) * itensPerPage).Take(itensPerPage);

            return query.ToList();
        }

        public void Delete(Vehicle vehicle)
        {
            _context.vehicles.Remove(vehicle);
            _context.SaveChanges();
        }

        public Vehicle? FindById(int id)
        {
            return _context.vehicles.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Include(Vehicle vehicle)
        {
            _context.vehicles.Add(vehicle);
            _context.SaveChanges();

        }

        public void Refresh(Vehicle vehicle)
        {
            _context.vehicles.Update(vehicle);
            _context.SaveChanges();
        }
    }
}
