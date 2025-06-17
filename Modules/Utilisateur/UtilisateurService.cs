using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.EntityFrameworkCore;

namespace MessagerieInterneAPI
{
    public class UtilisateurService
    {
        private readonly AppDbContext _context;

        public UtilisateurService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UtilisateurModel?> VerifUtilisateur(string matricule, string motDePasse)
        {
            var user = await _context.Utilisateur
                .FirstOrDefaultAsync(u => u.Matricule == matricule); 
            if (user == null) return null;

            var isValid = await _context
                .Utilisateur
                .FromSqlRaw("SELECT * FROM utilisateur WHERE matricule = {0} AND mdp = crypt({1}, mdp)", matricule, motDePasse)
                .AnyAsync();

            return isValid ? user : null;
        }


        public UtilisateurModel GetProfilUtilisateur(UtilisateurModel user)

        {
            return new UtilisateurModel
            {
                Id_utilisateur = user.Id_utilisateur,
                Matricule = user.Matricule,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Id_role = user.Id_role
            };
        }
    }
}
