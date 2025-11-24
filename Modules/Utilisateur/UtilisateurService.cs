using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MessagerieInterneAPI
{
    public class UtilisateurService
    {
        private readonly AppDbContext _context;
        private Connexion connexion = new Connexion();
        private readonly HttpClient _httpClient;

        public UtilisateurService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            // _httpClient = new HttpClient();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
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

        public List<UtilisateurModel> SearchAllUtilisateur(NpgsqlConnection liasonBase, string searchTerm)
        {
            List<UtilisateurModel> results = new List<UtilisateurModel>();
            string sql = @"
                SELECT * 
                FROM v_info_utilisateur
                WHERE (
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

        public List<UtilisateurModel> SearchDynamicUtilisateur(NpgsqlConnection liasonBase, string searchTerm, int? idEspaceTravail, string role)
        {
            List<UtilisateurModel> results = new List<UtilisateurModel>();
            string sql;

            if (role == "m_1")
            {
                // ADMIN → PAS DE FILTRE PAR ESPACE
                sql = @"
            SELECT * 
            FROM v_info_utilisateur
            WHERE (
                LOWER(nom) LIKE LOWER(@searchTerm)
                OR LOWER(prenom) LIKE LOWER(@searchTerm)
                OR LOWER(matricule) LIKE LOWER(@searchTerm)
            );
        ";
            }
            else
            {
                // UTILISATEUR → FILTRE PAR ESPACE
                sql = @"
            SELECT * 
            FROM v_utilisateur_espace_travail
            WHERE id_espace_travail = @idEspaceTravail
            AND (
                LOWER(nom) LIKE LOWER(@searchTerm)
                OR LOWER(prenom) LIKE LOWER(@searchTerm)
                OR LOWER(matricule) LIKE LOWER(@searchTerm)
            );
        ";
            }

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
                    cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                    if (role != "m_1") // seulement si utilisateur
                        cmd.Parameters.AddWithValue("@idEspaceTravail", idEspaceTravail ?? (object)DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new UtilisateurModel
                            {
                                Id_utilisateur = reader.GetInt32(reader.GetOrdinal("id_utilisateur")),
                                Matricule = reader.GetString(reader.GetOrdinal("matricule")),
                                Nom = reader.GetString(reader.GetOrdinal("nom")),
                                Prenom = reader.GetString(reader.GetOrdinal("prenom")),
                            });
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

        public async Task<List<EmployeDTO>> GetAllEmployesApi()
        {
            string url = "https://10.5.100.7:8888/api/Employe";
            string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkxhbGFtYnlDYW50aW5lIiwiTG9naXN0aXF1ZSI6IkFwcGxpY2F0aW9uIiwibmJmIjoxNzM0MzQ3MjEyLCJleHAiOjE3NjU4ODMyMTIsImlhdCI6MTczNDM0NzIxMiwiaXNzIjoieW91dENvbXBhbnlJc3N1ZXIuY29tIiwiYXVkIjoieW91dENvbXBhbnlJc3N1ZXIuY29tIn0.3jJnu2fs_QjrACjBa20_KJCVKIoTaxdnctxBlv6wzBQ"; // ton token

            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var employes = JsonConvert.DeserializeObject<List<EmployeDTO>>(json);

            return employes ?? new List<EmployeDTO>();
        }

        public async Task SynchroniserUtilisateursAsync()
        {
            var utilisateurs = await GetAllEmployesApi();

            var passwordHasher = new PasswordHasher<UtilisateurTableModel>();

            foreach (var user in utilisateurs)
            {
                if (string.IsNullOrWhiteSpace(user.Pole))
                    continue;

                // ➕ Insérer le pôle s’il n’existe pas encore
                var pole = await _context.Pole.FirstOrDefaultAsync(p => p.Pole == user.Pole);
                if (pole == null)
                {
                    pole = new PoleModel { Pole = user.Pole };
                    _context.Pole.Add(pole);
                    await _context.SaveChangesAsync();

                    var espace = new EspaceTravailModel
                    {
                        Nom = $"{user.Pole} - Espace de travail",
                        Id_pole = pole.Id_pole,
                        Id_admin = null
                    };
                    _context.EspaceTravail.Add(espace);
                    await _context.SaveChangesAsync();
                }

                // 🔎 Recherche l’utilisateur
                var existingUser = await _context.UtilisateurTable
                    .FirstOrDefaultAsync(u => u.Matricule == user.Matricule);


                if (existingUser == null)
                {
                    // ➕ Créer un nouvel utilisateur
                    var newUser = new UtilisateurTableModel
                    {
                        Nom = user.Nom,
                        Prenom = user.Prenom,
                        Matricule = user.Matricule,
                        Id_role = 2,
                        Mdp = passwordHasher.HashPassword(null, "pareramada*")
                    };

                    _context.UtilisateurTable.Add(newUser);
                    await _context.SaveChangesAsync();

                    // 🔗 Lier à l’espace de travail
                    var espace = await _context.EspaceTravail.FirstOrDefaultAsync(e => e.Id_pole == pole.Id_pole);
                    if (espace != null)
                    {
                        var lien = new UtilisateurEspaceTravailModel
                        {
                            Id_utilisateur = newUser.Id_utilisateur,
                            Id_espace_travail = espace.Id_espace_travail
                        };

                        _context.UtilisateurEspaceTravail.Add(lien);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    bool updated = false;

                    // 🧾 Vérifier si nom ou prénom a changé
                    if (existingUser.Nom != user.Nom)
                    {
                        existingUser.Nom = user.Nom;
                        updated = true;
                    }

                    if (existingUser.Prenom != user.Prenom)
                    {
                        existingUser.Prenom = user.Prenom;
                        updated = true;
                    }

                    // 🔒 Vérifier si le mot de passe est déjà haché
                    if (!existingUser.Mdp.StartsWith("$2") && !existingUser.Mdp.StartsWith("AQAAAA"))
                    {
                        // On suppose que le mot de passe n’est pas encore haché
                        existingUser.Mdp = passwordHasher.HashPassword(null, existingUser.Mdp);
                        updated = true;
                    }

                    if (updated)
                    {
                        _context.UtilisateurTable.Update(existingUser);
                        await _context.SaveChangesAsync();

                    }
                }
            }
        }

    }
}
