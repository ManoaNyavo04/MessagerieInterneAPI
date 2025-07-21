using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MessagerieInterneAPI
{
    public class UtilisateurService
    {
        private readonly AppDbContext _context;
        private Connexion connexion = new Connexion();

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

        public List<UtilisateurModel> GetAllUtilisateurs(NpgsqlConnection liaisonbase)
        {
            List<UtilisateurModel> allUtilisateurs = new List<UtilisateurModel>();

            String sql = "SELECT * FROM utilisateur";
            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liaisonbase);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while(reader.Read()){
                    UtilisateurModel user = new UtilisateurModel();
                    user.Id_utilisateur = (reader.GetInt32(0));
                    user.Nom = (reader.GetString(1));
                    user.Prenom = (reader.GetString(2));
                    user.Matricule = (reader.GetString(3));
                    user.Id_role = (reader.GetInt32(4));
                    user.Mdp = (reader.GetString(5));
                    allUtilisateurs.Add(user);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            finally
            {
                if (liaisonbase != null)
                {
                    liaisonbase.Close();
                }
            }
            return allUtilisateurs;
        }
    }
}
