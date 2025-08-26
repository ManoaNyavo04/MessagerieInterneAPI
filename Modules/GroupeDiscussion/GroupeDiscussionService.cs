using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.DTO;
using MessagerieInterneAPI.Entite;
using Npgsql;

namespace MessagerieInterneAPI
{
    public class GroupeDiscussionService
    {
        private Connexion connexion = new Connexion();

        public List<UtilisateurGroupeDiscussionModel> GetUtilisateurGrpDiscu(NpgsqlConnection liaisonbase, int idUtilisateur)
        {
            String sql = "SELECT * FROM v_utilisateur_groupe_discussion WHERE id_utilisateur = @iduser";
            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            List<UtilisateurGroupeDiscussionModel> listUser = new List<UtilisateurGroupeDiscussionModel>();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liaisonbase);
                cmd.Parameters.AddWithValue("@iduser", idUtilisateur);


                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UtilisateurGroupeDiscussionModel user = new UtilisateurGroupeDiscussionModel();
                    user.Id_utilisateur = (reader.GetInt32(0));
                    user.Id_groupe_discussion = (reader.GetInt32(1));
                    user.Nom = (reader.GetString(2));

                    user.Prenom = (reader.GetString(3));
                    user.Matricule = (reader.GetString(4));
                    user.Groupe = (reader.GetString(5));

                    listUser.Add(user);
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
            return listUser;
        }

        public async Task<int> CreateGroupeDiscussion(NpgsqlConnection liaisonbase, GroupeDiscussionModel groupe)
        {
            string sql = @"
                INSERT INTO groupe_discussion 
                (nom, id_espace_travail, description, date_creation, id_createur)
                VALUES (@nom, @id_espace_travail, @description, now(), @id_createur)
                RETURNING id_groupe_discussion";

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                using var cmd = new NpgsqlCommand(sql, liaisonbase);
                cmd.Parameters.AddWithValue("@nom", groupe.Nom);
                cmd.Parameters.AddWithValue("@id_espace_travail", 1);
                cmd.Parameters.AddWithValue("@description", groupe.Description);
                cmd.Parameters.AddWithValue("@id_createur", groupe.Id_createur);

                // Récupérer l'ID du groupe créé
                int groupeId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                groupe.Id_groupe_discussion = groupeId;


                return groupeId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            finally
            {
                if (liaisonbase != null)
                    liaisonbase.Close();
            }
        }

        public async Task AddMembreGrpDiscussion(NpgsqlConnection liaisonbase, GroupeDiscussionDTO dto, GroupeDiscussionModel groupe)
        {
            // ⚠️ On crée une liste des membres SANS le créateur
            var membres = dto.Utilisateurs?.Distinct().Where(id => id != groupe.Id_createur).ToList() ?? new List<int>();

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                // ✅ 1. Ajouter le créateur du groupe comme admin
                string sqlCreateur = @"
            INSERT INTO utilisateur_groupe_discussion 
            (id_groupe_discussion, id_utilisateur, est_admin)
            VALUES (@id_groupe_discussion, @id_utilisateur, @est_admin)";
                using (var cmdCreateur = new NpgsqlCommand(sqlCreateur, liaisonbase))
                {
                    cmdCreateur.Parameters.AddWithValue("@id_groupe_discussion", groupe.Id_groupe_discussion);
                    cmdCreateur.Parameters.AddWithValue("@id_utilisateur", groupe.Id_createur);
                    cmdCreateur.Parameters.AddWithValue("@est_admin", true);
                    await cmdCreateur.ExecuteNonQueryAsync();
                }

                // ✅ 2. Ajouter les autres membres
                foreach (var userId in membres)
                {
                    string sqlMembre = @"
                INSERT INTO utilisateur_groupe_discussion 
                (id_groupe_discussion, id_utilisateur, est_admin)
                VALUES (@id_groupe_discussion, @id_utilisateur, @est_admin)";
                    using var cmd = new NpgsqlCommand(sqlMembre, liaisonbase);
                    cmd.Parameters.AddWithValue("@id_groupe_discussion", groupe.Id_groupe_discussion);
                    cmd.Parameters.AddWithValue("@id_utilisateur", userId);
                    cmd.Parameters.AddWithValue("@est_admin", false); // Pas admin
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                liaisonbase?.Close();
            }
        }


        public async Task CreerGroupe(NpgsqlConnection liaisonbase, GroupeDiscussionDTO dto, int currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Créer le groupe
            var groupe = new GroupeDiscussionModel
            {
                Nom = dto.Nom,
                Id_espace_travail = 1,
                Description = dto.Description,
                Id_createur = currentUser
            };

            int groupeId = await CreateGroupeDiscussion(liaisonbase, groupe);

            if (groupeId > 0)
            {
                // Ajouter les membres au groupe
                await AddMembreGrpDiscussion(liaisonbase, dto, groupe);
            }

        }

        public async Task<List<UtilisateurGroupeDiscussionModel>> GetMembresGroupe(NpgsqlConnection liaisonbase, int idGroupe)
        {
            var membres = new List<UtilisateurGroupeDiscussionModel>();

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                string sql = @"
                    select * from v_utilisateur_groupe_discussion
                    WHERE id_groupe_discussion = @id_groupe_discussion
                    and statuts = 0";

                using (var cmd = new NpgsqlCommand(sql, liaisonbase))
                {
                    cmd.Parameters.AddWithValue("@id_groupe_discussion", idGroupe);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var membre = new UtilisateurGroupeDiscussionModel
                            {
                                Id_groupe_discussion = reader.GetInt32(0),
                                Groupe = reader.GetString(1),
                                Description = reader.GetString(2),
                                Id_utilisateur = reader.GetInt32(3),
                                Matricule = reader.GetString(4),
                                Nom = reader.GetString(5),
                                Prenom = reader.GetString(6),
                                Est_admin = reader.GetBoolean(7),
                                Statuts = reader.GetInt32(8)
                            };
                            membres.Add(membre);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                liaisonbase?.Close();
            }

            return membres;
        }

        public async Task AjouterNouveauxMembres(NpgsqlConnection liaisonbase, int idGroupe, List<int> nouveauxMembres)
        {
            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                foreach (var userId in nouveauxMembres.Distinct()) // Supprimer doublons
                {
                    string sqlMembre = @"
                        INSERT INTO utilisateur_groupe_discussion 
                        (id_groupe_discussion, id_utilisateur, est_admin)
                        VALUES (@id_groupe_discussion, @id_utilisateur, @est_admin)";
                    using var cmd = new NpgsqlCommand(sqlMembre, liaisonbase);
                    cmd.Parameters.AddWithValue("@id_groupe_discussion", idGroupe);
                    cmd.Parameters.AddWithValue("@id_utilisateur", userId);
                    cmd.Parameters.AddWithValue("@est_admin", false); 
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                liaisonbase?.Close();
            }
        }


    }
}
