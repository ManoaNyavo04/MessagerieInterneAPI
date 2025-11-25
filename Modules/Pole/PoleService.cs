using MessagerieInterneAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MessagerieInterneAPI
{
    public class PoleService
    {
        private readonly AppDbContext _context;

        public PoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PoleModel>> GetAllPole()
        {
            return await _context.Pole.ToListAsync();
        }
    }
}