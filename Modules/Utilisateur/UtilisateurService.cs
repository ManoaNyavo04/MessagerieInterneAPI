using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

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

        /*public async Task<UtilisateurModel?> VerifUtilisateur(string matricule, string motDePasse)
        {
            var user = await _context.Utilisateur
                .FirstOrDefaultAsync(u => u.Matricule == matricule);
            if (user == null) return null;

            var isValid = await _context
                .Utilisateur
                .FromSqlRaw("SELECT * FROM utilisateur WHERE matricule = {0} AND mdp = crypt({1}, mdp)", matricule, motDePasse)
                .AnyAsync();

            return isValid ? user : null;
        }*/

        public async Task<UtilisateurModel?> VerifUtilisateur(string matricule, string motDePasse)
        {
            var user = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Matricule == matricule);
            if (user == null) return null;

            bool isValid = false;

            // 🔍 Détecter le type de hash
            if (user.Mdp.StartsWith("$2a$") || user.Mdp.StartsWith("$2b$") || user.Mdp.StartsWith("$2y$"))
            {
                // 🧩 Cas des anciens mots de passe PostgreSQL bcrypt
                isValid = BCrypt.Net.BCrypt.Verify(motDePasse, user.Mdp);
            }
            else if (user.Mdp.StartsWith("AQAAAA"))
            {
                // 🧩 Cas des nouveaux mots de passe ASP.NET Identity
                var passwordHasher = new PasswordHasher<UtilisateurModel>();
                var result = passwordHasher.VerifyHashedPassword(user, user.Mdp, motDePasse);
                isValid = result == PasswordVerificationResult.Success;
            }
            else
            {
                // 🧩 Cas d'un mot de passe en clair (au cas où)
                isValid = user.Mdp == motDePasse;
            }

            return isValid ? user : null;
        }

        public void InsertUtilisateur(NpgsqlConnection liasonBase, UtilisateurModel utilisateur)
        {
            String sql = @"INSERT INTO utilisateur (nom, prenom, matricule,  id_role,mdp) 
                VALUES (@nom, @prenom, @matricule, @id_role, @mdp)";

            String defaultMdp = "pareramada*";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(defaultMdp);
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@matricule", utilisateur.Matricule);

                cmd.Parameters.AddWithValue("@nom", utilisateur.Nom);
                cmd.Parameters.AddWithValue("@prenom", utilisateur.Prenom);
                cmd.Parameters.AddWithValue("@id_role", utilisateur.Id_role);
                cmd.Parameters.AddWithValue("@mdp", hashedPassword);

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("❌ Erreur SQL lors de l'insertion de l'utilisateur : " + e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }

            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }
        }

        public async Task<bool> VerifMatricule(NpgsqlConnection liasonBase, UtilisateurModel utilisateur)
        {
            var user = await _context.Utilisateur
                .FirstOrDefaultAsync(u => u.Matricule == utilisateur.Matricule);
            if (user != null)
            {
                // Matricule existe déjà
                return false;
            }


            InsertUtilisateur(liasonBase, utilisateur);
            return true;

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

            String sql = "SELECT * FROM v_info_utilisateur";
            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liaisonbase);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UtilisateurModel user = new UtilisateurModel();
                    user.Id_utilisateur = (reader.GetInt32(0));
                    user.Nom = (reader.GetString(1));
                    user.Prenom = (reader.GetString(2));
                    user.Matricule = (reader.GetString(3));
                    user.Mdp = (reader.GetString(4));
                    user.Id_role = (reader.GetInt32(5));
                    user.Role = (reader.GetString(6));

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

        public List<UtilisateurModel> SearchUtilisateur(NpgsqlConnection liasonBase, string searchTerm, int? idEspaceTravail)
        {
            List<UtilisateurModel> results = new List<UtilisateurModel>();
            string sql = @"
                SELECT * 
                FROM v_utilisateur_espace_travail
                WHERE id_espace_travail = @idEspaceTravail
                AND (
                    LOWER(nom) LIKE LOWER(@searchTerm)
                    OR LOWER(prenom) LIKE LOWER(@searchTerm)
                    OR LOWER(matricule) LIKE LOWER(@searchTerm)
                );
            ";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                using (var cmd = new NpgsqlCommand(sql, liasonBase))
                {
                    cmd.Parameters.AddWithValue("@idEspaceTravail", idEspaceTravail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UtilisateurModel
                            {
                                Id_utilisateur = reader.GetInt32(reader.GetOrdinal("id_utilisateur")),
                                Matricule = reader.GetString(reader.GetOrdinal("matricule")),
                                Nom = reader.GetString(reader.GetOrdinal("nom")),
                                Prenom = reader.GetString(reader.GetOrdinal("prenom")),
                                // Si ta vue ne contient pas Mdp, Role, etc., ne les lis pas
                            };

                            results.Add(user);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur SQL : " + e.Message);
            }
            finally
            {
                liasonBase?.Close();
            }

            return results;
        }

        public async Task<UtilisateurModel> GetUtilisateurId(int idUtilisateur)
        {
            return await _context.Utilisateur
                .Where(u => u.Id_utilisateur == idUtilisateur)
                .FirstOrDefaultAsync();
        }

    }
}
