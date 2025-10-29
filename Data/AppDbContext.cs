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
        public DbSet<MessageModel> Message { get; set; }
        public DbSet<RoleModel> Role { get; set; }
        public DbSet<GroupeDiscussionModel> GroupeDiscussion { get; set; }
        public DbSet<EspaceTravailModel> EspaceTravail { get; set; }
        public DbSet<PoleModel> Pole { get; set; }
        public DbSet<UtilisateurEspaceTravailModel> UtilisateurEspaceTravail { get; set; }
        public DbSet<UtilisateurTableModel> UtilisateurTable { get; set; }
        public DbSet<UtilisateurEspaceTravailView> UtilisateurEspaceTravailView { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UtilisateurGroupeDiscussionModel>().HasNoKey().ToView("v_utilisateur_groupe_discussion");
        }

    }
}
