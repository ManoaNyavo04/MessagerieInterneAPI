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
        public DbSet<UtilisateurGroupeDiscussionModel> UtilisateurGrpDiscu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UtilisateurGroupeDiscussionModel>().HasNoKey().ToView("v_utilisateur_groupe_discussion");
        }

    }
}
