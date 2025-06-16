using Microsoft.EntityFrameworkCore;
using MessagerieInterneAPI.Entite;

namespace MessagerieInterneAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UtilisateurModel> Utilisateur { get; set; }
    }
}
