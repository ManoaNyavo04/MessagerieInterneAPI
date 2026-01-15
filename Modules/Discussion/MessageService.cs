using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MessagerieInterneAPI.Modules.Discussion
{
    public class MessageService
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly AppDbContext _context;
        private readonly int _editDelayMinutes;

        public MessageService(NpgsqlDataSource dataSource, IConfiguration config, AppDbContext context)
        {
            _dataSource = dataSource;
            _editDelayMinutes = config.GetValue<int>("MessageSettings:EditDelayMinutes");
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public MessageService()
        {
        }

        public async Task<IEnumerable<MessageModel>> GetMessages(int exp, int dest, string type)
        {
            string sql;

            if (type == "groupe")
            {
                sql = @"
        SELECT 
            v.*, 
            EXISTS (
                SELECT 1 
                FROM message_utilisateur_statut mus 
                WHERE mus.id_message = v.id_message 
                  AND mus.id_utilisateur != @userId
                  AND mus.id_status_msg = 3
            )::boolean AS est_lu,
            ARRAY(
                SELECT u.prenom::text
                FROM message_utilisateur_statut mus
                JOIN utilisateur u ON u.id_utilisateur = mus.id_utilisateur
                WHERE mus.id_message = v.id_message 
                  AND mus.id_status_msg = 3
            )::text[] AS liste_utilisateur_vu
        FROM v_utilisateur_message v
        WHERE v.id_groupe_discussion = @id
        ORDER BY v.id_message ASC";
            }
            else if (type == "prive")
            {
                sql = @"
        SELECT 
            v.*,
            CASE 
                WHEN v.id_expediteur = @userId 
                    THEN v.matricule_destinataire
                ELSE v.matricule_expediteur
            END AS matricule_autre,
            EXISTS (
                SELECT 1 
                FROM message_utilisateur_statut mus 
                WHERE mus.id_message = v.id_message 
                  AND mus.id_utilisateur = @dest 
                  AND mus.id_status_msg = 3
            )::boolean AS est_lu
        FROM v_utilisateur_message v
        WHERE 
            (v.id_expediteur = @userId AND v.id_destinataire = @id)
         OR (v.id_expediteur = @id AND v.id_destinataire = @userId)
        ORDER BY v.id_message ASC";
            }
            else
            {
                throw new ArgumentException("Type de discussion inconnu");
            }

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dest);
            cmd.Parameters.AddWithValue("@userId", exp);
            if (type == "prive")
                cmd.Parameters.AddWithValue("@dest", dest);

            var messages = new List<MessageModel>();

            using var reader = await cmd.ExecuteReaderAsync();

            // Index communs (v.*)
            int idxIdMessage = reader.GetOrdinal("id_message");
            int idxIdExp = reader.GetOrdinal("id_expediteur");
            int idxNomExp = reader.GetOrdinal("nom_expediteur");
            int idxMatExp = reader.GetOrdinal("matricule_expediteur");
            int idxIdDest = reader.GetOrdinal("id_destinataire");
            int idxNomDest = reader.GetOrdinal("nom_destinataire");
            int idxIdGroupe = reader.GetOrdinal("id_groupe_discussion");
            int idxContenu = reader.GetOrdinal("contenu");
            int idxDate = reader.GetOrdinal("date_envoie");
            int idxStatus = reader.GetOrdinal("id_status_msg");
            int idxIdPJ = reader.GetOrdinal("id_piece_joint");
            int idxChemin = reader.GetOrdinal("chemin");
            int idxNomOriginal = reader.GetOrdinal("nom_original");
            int idxEspace = reader.GetOrdinal("id_espace_travail");
            int idxDateModif = reader.GetOrdinal("date_modification");
            int idxModifiable = reader.GetOrdinal("modifiable_jusqua");

            // Index spécifiques
            int idxEstLu = reader.GetOrdinal("est_lu");
            int idxMatAutre = type == "prive" ? reader.GetOrdinal("matricule_autre") : -1;
            int idxListeVu = type == "groupe" ? reader.GetOrdinal("liste_utilisateur_vu") : -1;

            while (await reader.ReadAsync())
            {
                var msg = new MessageModel
                {
                    Id_message = reader.GetInt32(idxIdMessage),
                    Id_expediteur = reader.GetInt32(idxIdExp),
                    Nom_expediteur = reader.GetString(idxNomExp),
                    Matricule_expediteur = reader.GetString(idxMatExp),

                    Id_destinataire = reader.IsDBNull(idxIdDest) ? null : reader.GetInt32(idxIdDest),
                    Nom_destinataire = reader.IsDBNull(idxNomDest) ? null : reader.GetString(idxNomDest),

                    Id_groupe_discussion = reader.IsDBNull(idxIdGroupe) ? null : reader.GetInt32(idxIdGroupe),
                    Contenu = reader.GetString(idxContenu),
                    Date_envoie = reader.GetDateTime(idxDate),
                    Id_status_msg = reader.GetInt32(idxStatus),

                    Id_piece_jointe = reader.IsDBNull(idxIdPJ) ? null : reader.GetInt32(idxIdPJ),
                    Chemin = reader.IsDBNull(idxChemin) ? null : reader.GetString(idxChemin),
                    Nom_original = reader.IsDBNull(idxNomOriginal) ? null : reader.GetString(idxNomOriginal),
                    Id_espace_travail = reader.IsDBNull(idxEspace) ? null : reader.GetInt32(idxEspace),

                    Date_modification = reader.IsDBNull(idxDateModif) ? null : reader.GetDateTime(idxDateModif),
                    Modifiable_jusqua = reader.IsDBNull(idxModifiable) ? null : reader.GetDateTime(idxModifiable),

                    Est_lu = reader.GetBoolean(idxEstLu)
                };

                if (type == "prive")
                {
                    msg.Matricule_autre = reader.GetString(idxMatAutre);
                }

                if (type == "groupe")
                {
                    msg.Liste_utilisateur_vu = reader.IsDBNull(idxListeVu)
                        ? new List<string>()
                        : reader.GetFieldValue<string[]>(idxListeVu).ToList();
                }

                messages.Add(msg);
            }

            return messages;
        }




        public List<MessageModel> GetMessagesByGroupId(NpgsqlConnection liasonBase, int groupId)
        {
            List<MessageModel> messages = new List<MessageModel>();
            Console.WriteLine($"📥 Lecture des messages pour le groupe {groupId}");
            String sql = "SELECT * FROM message WHERE id_groupe_discussion = @id_groupe_discussion ORDER BY date_envoie";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@id_groupe_discussion", groupId);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MessageModel message = new MessageModel
                    {
                        Id_message = reader.GetInt32(0),
                        Id_expediteur = reader.GetInt32(1),
                        Id_destinataire = reader.GetInt32(2),
                        Id_groupe_discussion = reader.GetInt32(3),
                        Contenu = reader.GetString(4),
                        Date_envoie = reader.GetDateTime(5),
                        Id_status_msg = reader.GetInt32(6)
                    };
                    Console.WriteLine($"[DEBUG] Message lu: {message.Id_message} - {message.Contenu}");
                    messages.Add(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

            return messages;
        }

        public List<MessageModel> GetIndividualMessage(NpgsqlConnection liasonBase, int exp, int dest)
        {
            Console.WriteLine($"[DEBUG] GetIndividualMessage: exp={exp}, dest={dest}");

            List<MessageModel> messages = new List<MessageModel>();
            string sql = @"
                SELECT *
                FROM message
                WHERE id_groupe_discussion IS NULL
                AND (
                    (id_expediteur = @exp AND id_destinataire = @dest) OR
                    (id_expediteur = @dest AND id_destinataire = @exp)
                )
                ORDER BY date_envoie;
            ";

            // Vérifie l'état de la connexion
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
                    cmd.Parameters.AddWithValue("@exp", exp);
                    cmd.Parameters.AddWithValue("@dest", dest);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new MessageModel
                            {
                                Id_message = reader.GetInt32(0),
                                Id_expediteur = reader.GetInt32(1),
                                Id_destinataire = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                Id_groupe_discussion = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                Contenu = reader.GetString(4),
                                Date_envoie = reader.GetDateTime(5),
                                Id_status_msg = reader.GetInt32(6)
                            };

                            messages.Add(message);
                            Console.WriteLine($"[DEBUG] Message trouvé : {message.Id_message} - {message.Contenu}");
                        }
                    }
                }

                Console.WriteLine($"[DEBUG] Total messages trouvés: {messages.Count}");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERREUR] Exception lors de la récupération des messages : " + e.Message);
            }
            finally
            {
                if (liasonBase != null && liasonBase.State != ConnectionState.Closed)
                {
                    liasonBase.Close();
                }
            }

            return messages;
        }
        public async Task<MessageModel> SendMessage(MessageModel message)
        {
            const string sql = @"
        INSERT INTO message (
            id_expediteur,
            id_destinataire,
            id_groupe_discussion,
            contenu,
            date_envoie,
            id_status_msg,
            id_espace_travail,
            modifiable_jusqua,
            date_modification
        )
        VALUES (
            @id_expediteur,
            @id_destinataire,
            @id_groupe_discussion,
            @contenu,
            @date_envoie,
            @id_status_msg,
            @id_espace_travail,
            @modifiable_jusqua,
            NULL
        )
        RETURNING
            id_message,
            date_envoie,
            modifiable_jusqua;
    ";

            using var connexion = new Connexion().ConnectPostgres();
            await connexion.OpenAsync();

            // 🔐 Toujours en UTC
            var now = DateTime.UtcNow;
            var modifiableJusqua = now.AddMinutes(5);

            using var cmd = new NpgsqlCommand(sql, connexion);
            cmd.Parameters.AddWithValue("@id_expediteur", message.Id_expediteur);
            cmd.Parameters.AddWithValue("@id_destinataire", (object?)message.Id_destinataire ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_groupe_discussion", (object?)message.Id_groupe_discussion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contenu", message.Contenu ?? "");
            cmd.Parameters.AddWithValue("@date_envoie", now);
            cmd.Parameters.AddWithValue("@id_status_msg", message.Id_status_msg);

            if (message.Id_espace_travail.HasValue)
                cmd.Parameters.AddWithValue("@id_espace_travail", message.Id_espace_travail.Value);
            else
                cmd.Parameters.AddWithValue("@id_espace_travail", DBNull.Value);

            cmd.Parameters.AddWithValue("@modifiable_jusqua", modifiableJusqua);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                message.Id_message = reader.GetInt32(reader.GetOrdinal("id_message"));
                message.Date_envoie = reader.GetDateTime(reader.GetOrdinal("date_envoie"));
                message.Modifiable_jusqua = reader.GetDateTime(reader.GetOrdinal("modifiable_jusqua"));
            }

            return message;
        }


        public List<DiscussionModel> GetGrpDiscussionByUser(int userId, int idEspaceTravail, NpgsqlConnection liasonBase)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();
            string sql = @"
                SELECT id_groupe_discussion, groupe, 'groupe' AS type, id_espace_travail
                FROM v_utilisateur_groupe_discussion
                WHERE id_utilisateur = @userId
                AND id_espace_travail = @idEspaceTravail"; // ✅ filtre ajouté

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@idEspaceTravail", idEspaceTravail);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DiscussionModel discussion = new DiscussionModel
                    {
                        Id = reader.GetInt32(0),
                        Nom = reader.GetString(1),
                        Type = reader.GetString(2)
                    };
                    discussions.Add(discussion);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

            return discussions;
        }


        public List<DiscussionModel> GetDiscussionIndividuelleByUser(int userId, int idEspaceTravail, NpgsqlConnection liasonBase)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();
            string sql = @"
SELECT DISTINCT ON (id_autre_utilisateur)
    id_autre_utilisateur AS id,
    nom_autre_utilisateur AS nom,
    'prive' AS type,
    matricule_autre AS matricule
FROM (
    SELECT 
        CASE 
            WHEN id_expediteur = @userId THEN id_destinataire_user
            ELSE id_expediteur_user
        END AS id_autre_utilisateur,

        CASE 
            WHEN id_expediteur = @userId THEN nom_destinataire
            ELSE nom_expediteur
        END AS nom_autre_utilisateur,

        CASE 
            WHEN id_expediteur = @userId THEN matricule_destinataire
            ELSE matricule_expediteur
        END AS matricule_autre

    FROM v_discussions_individuelles
    WHERE id_expediteur = @userId 
       OR id_destinataire = @userId
) sub
ORDER BY id_autre_utilisateur;


";




            Console.WriteLine("SQL Query: " + sql);
            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@userId", userId);
                // cmd.Parameters.AddWithValue("@idEspaceTravail", idEspaceTravail);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    discussions.Add(new DiscussionModel
                    {
                        Id = reader.GetInt32(0),
                        Nom = reader.GetString(1),

                        Type = reader.GetString(2),
                        Matricule = reader.IsDBNull(3) ? null : reader.GetString(3),
                    });
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

            return discussions;
        }
        public List<DiscussionModel> searchDiscussion(NpgsqlConnection liasonBase, int idExpediteur, int idDestinataire)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();
            String sql = @"
                SELECT *
                FROM v_discussions_individuelles
                WHERE (
                    (id_expediteur = @id1 AND id_destinataire = @id2) OR
                    (id_expediteur = @id2 AND id_destinataire = @id1)
                )";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@id1", idExpediteur);
                cmd.Parameters.AddWithValue("@id2", idDestinataire);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int rowId = reader.GetOrdinal("id_destinataire");
                    int rowNom = reader.GetOrdinal("nom_destinataire");
                    DiscussionModel discussion = new DiscussionModel
                    {
                        Id = reader.GetInt32(rowId),
                        Nom = reader.GetString(rowNom),
                        Type = "prive"
                    };
                    discussions.Add(discussion);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

            return discussions;
        }

        public async Task<DiscussionModel> VerifOuCreeDiscussionIndividuelle(NpgsqlConnection connexion, int espaceActifId,
            int idExpediteur, DiscussionModel discussion)
        {

            Console.WriteLine(discussion.Id + " " + discussion.Nom);
            // var messages = GetDiscussionIndividuelleByUser(idExpediteur, connexion);
            var nouvelleDiscussion = searchDiscussion(connexion, idExpediteur, discussion.Id);

            if (nouvelleDiscussion.Any())
            {

                Console.WriteLine("✅ Discussion trouvée, récupération des messages...");
                var messages = GetDiscussionIndividuelleByUser(idExpediteur, espaceActifId, connexion);
                return messages.FirstOrDefault();
            }
            else
            {
                Console.WriteLine("❌ Discussion inexistante, création en cours..." + nouvelleDiscussion.FirstOrDefault()?.Id);
                // ❌ Discussion inexistante → on la crée
                // var nouvelleDiscussion = CreateNewDiscussion(connexion, idExpediteur, idDestinataire);
                var discuss = new DiscussionModel(discussion.Id, discussion.Nom, "prive");

                return discuss;
            }

        }

        public async Task<List<ComptageMsgNonLuDTO>> GetUnreadCounts(int idUser)
        {
            const string sql = @"
                -- Messages privés non lus
                SELECT 
                    m.id_expediteur AS id,
                    'prive' AS type,
                    COUNT(*) AS unread_count
                FROM message m
                LEFT JOIN message_utilisateur_statut mus 
                    ON mus.id_message = m.id_message AND mus.id_utilisateur = @idUser
                WHERE m.id_destinataire = @idUser 
                AND mus.id_message_utilisateur_statut IS NULL
                GROUP BY m.id_expediteur

                UNION ALL

                -- Messages de groupe non lus
                SELECT 
                    m.id_groupe_discussion AS id,
                    'groupe' AS type,
                    COUNT(*) AS unread_count
                FROM message m
                JOIN utilisateur_groupe_discussion ugd 
                    ON ugd.id_groupe_discussion = m.id_groupe_discussion
                LEFT JOIN message_utilisateur_statut mus 
                    ON mus.id_message = m.id_message AND mus.id_utilisateur = @idUser
                WHERE ugd.id_utilisateur = @idUser 
                AND m.id_expediteur != @idUser
                AND mus.id_message_utilisateur_statut IS NULL
                GROUP BY m.id_groupe_discussion;

            ";

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUser", idUser);


            var result = new List<ComptageMsgNonLuDTO>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ComptageMsgNonLuDTO
                {
                    Id = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    Count = reader.GetInt32(2)
                });
            }

            return result;
        }

        public async Task MarkMessagesAsRead(int idUser, int idDiscussion, string type)
        {
            string sql = type == "prive" ? @"
                INSERT INTO message_utilisateur_statut (id_message, id_utilisateur, id_status_msg)
                SELECT id_message, @idUser, 3
                FROM message
                WHERE id_expediteur = @idDiscussion
                AND id_destinataire = @idUser
                AND id_message NOT IN (
                    SELECT id_message FROM message_utilisateur_statut 
                    WHERE id_utilisateur = @idUser AND id_status_msg = 3
                )
                ON CONFLICT (id_message, id_utilisateur) DO UPDATE 
                SET id_status_msg = 3;
            " : @"
                INSERT INTO message_utilisateur_statut (id_message, id_utilisateur, id_status_msg)
                SELECT m.id_message, @idUser, 3
                FROM message m
                JOIN utilisateur_groupe_discussion ugd ON ugd.id_groupe_discussion = m.id_groupe_discussion
                WHERE m.id_groupe_discussion = @idDiscussion
                AND ugd.id_utilisateur = @idUser
                AND m.id_expediteur != @idUser
                AND m.id_message NOT IN (
                    SELECT id_message FROM message_utilisateur_statut 
                    WHERE id_utilisateur = @idUser AND id_status_msg = 3
                )
                ON CONFLICT (id_message, id_utilisateur) DO UPDATE 
                SET id_status_msg = 3;
            ";

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idDiscussion", idDiscussion);

            await cmd.ExecuteNonQueryAsync();
        }



        public async Task<string> GetNomExpediteur(NpgsqlConnection liasonBase, int idExpediteur, int idDestinataire)
        {
            string nomExpediteur = null;

            string sql = @"
                SELECT nom_expediteur 
                FROM v_discussions_individuelles
                WHERE id_expediteur = @idExp AND id_destinataire = @idDest
                LIMIT 1;
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
                    cmd.Parameters.AddWithValue("@idExp", idExpediteur);
                    cmd.Parameters.AddWithValue("@idDest", idDestinataire);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int indexNom = reader.GetOrdinal("nom_expediteur");
                            nomExpediteur = reader.GetString(indexNom);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur GetNomExpediteur : " + ex.Message);
            }
            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

            return nomExpediteur;
        }
        public List<DiscussionModel> SearchUtilisateurEtGroupe(
            NpgsqlConnection liasonBase,
            int idUtilisateur,
            string searchTerm,
            int idEspaceActif,
            string role)
        {
            List<DiscussionModel> results = new List<DiscussionModel>();

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                // ===============================
                // 🔎 1) Recherche UTILISATEURS
                // ===============================

                string sqlUtilisateurs;

                if (role == "m_1")
                {
                    // ⭐ ADMIN : pas de filtre par espace
                    sqlUtilisateurs = @"
                SELECT DISTINCT u.id_utilisateur AS id, 
                       CONCAT(u.prenom, ' ', u.nom) AS nom, 
                       'utilisateur' AS type, u.matricule
                FROM utilisateur u
                WHERE (LOWER(u.nom) LIKE LOWER(@searchTerm)
                    OR LOWER(u.prenom) LIKE LOWER(@searchTerm)
                    OR LOWER(u.matricule) LIKE LOWER(@searchTerm))
                  AND u.id_utilisateur <> @idUtilisateur
                ORDER BY nom ASC";
                }
                else
                {
                    // ⭐ UTILISATEUR NORMAL : filtrage par espace
                    sqlUtilisateurs = @"
                SELECT DISTINCT u.id_utilisateur AS id, 
                       CONCAT(u.prenom, ' ', u.nom) AS nom, 
                       'utilisateur' AS type, u.matricule
                FROM utilisateur u
                JOIN utilisateur_espace_travail uet ON u.id_utilisateur = uet.id_utilisateur
                WHERE uet.id_espace_travail = @idEspaceActif
                  AND (LOWER(u.nom) LIKE LOWER(@searchTerm)
                       OR LOWER(u.prenom) LIKE LOWER(@searchTerm)
                       OR LOWER(u.matricule) LIKE LOWER(@searchTerm))
                  AND u.id_utilisateur <> @idUtilisateur
                ORDER BY nom ASC";
                }

                using (var cmd = new NpgsqlCommand(sqlUtilisateurs, liasonBase))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    cmd.Parameters.AddWithValue("@idUtilisateur", idUtilisateur);

                    if (role != "m_1")
                        cmd.Parameters.AddWithValue("@idEspaceActif", idEspaceActif);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new DiscussionModel
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Type = reader.GetString(2),
                                Matricule = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }
                    }
                }

                // ===============================
                // 🔎 2) Recherche GROUPES
                // ===============================

                string sqlGroupes;

                if (role == "m_1")
                {
                    // ⭐ ADMIN : aucun filtre espace, ni appartenance au groupe
                    sqlGroupes = @"
                SELECT g.id_groupe_discussion AS id, 
                       g.nom, 
                       'groupe' AS type
                FROM groupe_discussion g
                WHERE LOWER(g.nom) LIKE LOWER(@searchTerm)
                ORDER BY g.nom ASC";
                }
                else
                {
                    // ⭐ UTILISATEUR NORMAL : groupes du même espace + à lesquels il appartient
                    sqlGroupes = @"
                SELECT g.id_groupe_discussion AS id, 
                       g.nom, 
                       'groupe' AS type
                FROM groupe_discussion g
                JOIN utilisateur_groupe_discussion ug 
                    ON ug.id_groupe_discussion = g.id_groupe_discussion
                WHERE g.id_espace_travail = @idEspaceActif
                  AND ug.id_utilisateur = @idUtilisateur
                  AND LOWER(g.nom) LIKE LOWER(@searchTerm)
                ORDER BY g.nom ASC";
                }

                using (var cmd = new NpgsqlCommand(sqlGroupes, liasonBase))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                    if (role != "m_1")
                    {
                        cmd.Parameters.AddWithValue("@idUtilisateur", idUtilisateur);
                        cmd.Parameters.AddWithValue("@idEspaceActif", idEspaceActif);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new DiscussionModel
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Type = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur recherche : " + e.Message);
            }
            finally
            {
                liasonBase?.Close();
            }

            return results;
        }

        public async Task<MessageModel> GetMessageIdAsync(int idMessage)
        {
            return await _context.Message.FindAsync(idMessage);
        }

        public async Task UpdateMessageContent(int idMessage, string newContent, int idStatus)
        {
            const string sql = @"
                UPDATE message
                SET contenu = @contenu,
                id_status_msg = @id_status_msg,
                    date_modification = @date_modification
                WHERE id_message = @id_message
            ";

            using var connect = new Connexion().ConnectPostgres();
            connect.Open();

            using var cmd = new NpgsqlCommand(sql, connect);
            cmd.Parameters.AddWithValue("@contenu", newContent);
            cmd.Parameters.AddWithValue("@id_status_msg", idStatus); // statut modifié
            cmd.Parameters.AddWithValue("@date_modification", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@id_message", idMessage);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MessageModel> GetMessageById(NpgsqlConnection db, int idMessage)
        {
            const string sql = "SELECT * FROM v_utilisateur_message WHERE id_message = @id_message";

            using var cmd = new NpgsqlCommand(sql, db);
            cmd.Parameters.AddWithValue("@id_message", idMessage);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MessageModel
                {
                    Id_message = reader.GetInt32(reader.GetOrdinal("id_message")),
                    Id_expediteur = reader.GetInt32(reader.GetOrdinal("id_expediteur")),
                    Nom_expediteur = reader.GetString(reader.GetOrdinal("nom_expediteur")),
                    Id_destinataire = reader["id_destinataire"] as int?,
                    Id_groupe_discussion = reader["id_groupe_discussion"] as int?,
                    Contenu = reader.GetString(reader.GetOrdinal("contenu")),
                    Date_envoie = reader.GetDateTime(reader.GetOrdinal("date_envoie")),
                    Id_status_msg = reader.GetInt32(reader.GetOrdinal("id_status_msg")),
                    Chemin = reader["chemin"] as string,
                    Nom_original = reader["nom_original"] as string,
                    Id_espace_travail = reader["id_espace_travail"] as int?,
                    Modifiable_jusqua = reader["modifiable_jusqua"] as DateTime?,
                    Date_modification = reader["date_modification"] as DateTime?
                };
            }

            return null;
        }

        public async Task SupprimerMessageAsync(int idMessage)
        {
            const string sql = @"
                UPDATE message
                SET id_status_msg = 5,
                    date_modification = @date_modification
                WHERE id_message = @id_message
            ";

            using var connect = new Connexion().ConnectPostgres();
            connect.Open();

            using var cmd = new NpgsqlCommand(sql, connect);
            cmd.Parameters.AddWithValue("@id_message", idMessage);
            cmd.Parameters.AddWithValue("@date_modification", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<MessageModel>> SearchMessages(
    int exp,
    int dest,
    string type,
    string search
)
        {
            string sql;

            if (type == "groupe")
            {
                sql = @"
        SELECT
            v.id_message,
            v.id_expediteur,
            v.nom_expediteur,
            v.matricule_expediteur,
            v.id_destinataire,
            v.nom_destinataire,
            v.matricule_destinataire,
            v.id_groupe_discussion,
            v.contenu,
            v.date_envoie,
            v.id_status_msg,
            v.id_piece_joint,
            v.chemin,
            v.nom_original,
            v.id_espace_travail,
            v.date_modification,
            v.modifiable_jusqua,

            EXISTS (
                SELECT 1
                FROM message_utilisateur_statut mus
                WHERE mus.id_message = v.id_message
                  AND mus.id_utilisateur != @userId
                  AND mus.id_status_msg = 3
            ) AS est_lu,

            ARRAY(
                SELECT u.prenom::text
                FROM message_utilisateur_statut mus
                JOIN utilisateur u ON u.id_utilisateur = mus.id_utilisateur
                WHERE mus.id_message = v.id_message
                  AND mus.id_status_msg = 3
            ) AS liste_utilisateur_vu
        FROM v_utilisateur_message v
        WHERE v.id_groupe_discussion = @id
          AND (
                COALESCE(v.contenu, '') ILIKE @search
             OR COALESCE(v.nom_original, '') ILIKE @search
          )
        ORDER BY v.id_message ASC;";
            }
            else if (type == "prive")
            {
                sql = @"
        SELECT
            v.id_message,
            v.id_expediteur,
            v.nom_expediteur,
            v.matricule_expediteur,
            v.id_destinataire,
            v.nom_destinataire,
            v.matricule_destinataire,
            v.id_groupe_discussion,
            v.contenu,
            v.date_envoie,
            v.id_status_msg,
            v.id_piece_joint,
            v.chemin,
            v.nom_original,
            v.id_espace_travail,
            v.date_modification,
            v.modifiable_jusqua,

            EXISTS (
                SELECT 1
                FROM message_utilisateur_statut mus
                WHERE mus.id_message = v.id_message
                  AND mus.id_utilisateur = @dest
                  AND mus.id_status_msg = 3
            ) AS est_lu
        FROM v_utilisateur_message v
        WHERE (
                (v.id_expediteur = @userId AND v.id_destinataire = @id)
             OR (v.id_expediteur = @id AND v.id_destinataire = @userId)
              )
          AND (
                COALESCE(v.contenu, '') ILIKE @search
             OR COALESCE(v.nom_original, '') ILIKE @search
          )
        ORDER BY v.id_message ASC;";
            }
            else
            {
                throw new ArgumentException("Type de discussion inconnu");
            }

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dest);
            cmd.Parameters.AddWithValue("@userId", exp);
            cmd.Parameters.AddWithValue("@search", $"%{search}%");

            if (type == "prive")
                cmd.Parameters.AddWithValue("@dest", dest);

            var messages = new List<MessageModel>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var msg = new MessageModel
                {
                    Id_message = reader.GetInt32(reader.GetOrdinal("id_message")),
                    Id_expediteur = reader.GetInt32(reader.GetOrdinal("id_expediteur")),

                    Nom_expediteur = reader.IsDBNull(reader.GetOrdinal("nom_expediteur"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("nom_expediteur")),

                    Matricule_expediteur = reader.IsDBNull(reader.GetOrdinal("matricule_expediteur"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("matricule_expediteur")),

                    Id_destinataire = reader.IsDBNull(reader.GetOrdinal("id_destinataire"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("id_destinataire")),

                    Nom_destinataire = reader.IsDBNull(reader.GetOrdinal("nom_destinataire"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("nom_destinataire")),

                    Matricule_destinataire = reader.IsDBNull(reader.GetOrdinal("matricule_destinataire"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("matricule_destinataire")),

                    Id_groupe_discussion = reader.IsDBNull(reader.GetOrdinal("id_groupe_discussion"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("id_groupe_discussion")),

                    Contenu = reader.IsDBNull(reader.GetOrdinal("contenu"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("contenu")),

                    Date_envoie = reader.GetDateTime(reader.GetOrdinal("date_envoie")),
                    Id_status_msg = reader.GetInt32(reader.GetOrdinal("id_status_msg")),

                    Id_piece_jointe = reader.IsDBNull(reader.GetOrdinal("id_piece_joint"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("id_piece_joint")),

                    Chemin = reader.IsDBNull(reader.GetOrdinal("chemin"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("chemin")),

                    Nom_original = reader.IsDBNull(reader.GetOrdinal("nom_original"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("nom_original")),

                    Id_espace_travail = reader.IsDBNull(reader.GetOrdinal("id_espace_travail"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("id_espace_travail")),

                    Date_modification = reader.IsDBNull(reader.GetOrdinal("date_modification"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("date_modification")),

                    Modifiable_jusqua = reader.IsDBNull(reader.GetOrdinal("modifiable_jusqua"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("modifiable_jusqua")),

                    Est_lu = reader.GetBoolean(reader.GetOrdinal("est_lu"))
                };

                if (type == "groupe")
                {
                    msg.Liste_utilisateur_vu =
                        reader.IsDBNull(reader.GetOrdinal("liste_utilisateur_vu"))
                        ? new List<string>()
                        : reader.GetFieldValue<string[]>(
                            reader.GetOrdinal("liste_utilisateur_vu")
                          ).ToList();
                }

                messages.Add(msg);
            }

            return messages;
        }



    }

}
