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

        public MessageService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public MessageService()
        {
        }

        public async Task<IEnumerable<MessageModel>> GetMessages(int exp, int dest, string type)
        {
            string sql;
            // int exp, int dest

            if (type == "groupe")
            {
                sql = @"SELECT * FROM v_utilisateur_message 
                WHERE id_groupe_discussion = @id 
                ORDER BY id_message ASC"; // 🔹 Afficher tous les messages du groupe
            }
            else if (type == "prive")
            {
                sql = @"SELECT * FROM v_utilisateur_message 
                WHERE (id_expediteur = @userId AND id_destinataire = @id)
                   OR (id_expediteur = @id AND id_destinataire = @userId)
                ORDER BY id_message ASC"; // 🔹 Messages privés entre 2 personnes
            }
            else
            {
                throw new ArgumentException("Type de discussion inconnu");
            }

            using var conn = new Connexion().ConnectPostgres();
            conn.Open();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dest);
            if (type == "prive")
                cmd.Parameters.AddWithValue("@userId", exp); // à ajuster selon ton contexte

            var messages = new List<MessageModel>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new MessageModel
                {
                    Id_message = reader.GetInt32(0),
                    Id_expediteur = reader.GetInt32(1),
                    Nom_expediteur = reader.GetString(2),
                    Id_destinataire = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Id_groupe_discussion = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Contenu = reader.GetString(5),
                    Date_envoie = reader.GetDateTime(6),
                    Id_status_msg = reader.GetInt32(7)
                });
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
                id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoie, id_status_msg
            )
            VALUES (
                @id_expediteur, @id_destinataire, @id_groupe_discussion, @contenu, @date_envoie, @id_status_msg
            )
            RETURNING id_message";  // 🔴 Ajout RETURNING

            using var liasonBase = new Connexion().ConnectPostgres();
            liasonBase.Open();

            using var cmd = new NpgsqlCommand(sql, liasonBase);
            cmd.Parameters.AddWithValue("@id_expediteur", message.Id_expediteur);
            cmd.Parameters.AddWithValue("@id_destinataire", (object?)message.Id_destinataire ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_groupe_discussion", (object?)message.Id_groupe_discussion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contenu", message.Contenu);
            cmd.Parameters.AddWithValue("@date_envoie", message.Date_envoie);
            cmd.Parameters.AddWithValue("@id_status_msg", message.Id_status_msg);

            // 🔽 Lire l'ID inséré
            var id = await cmd.ExecuteScalarAsync();
            message.Id_message = Convert.ToInt32(id); // Assure-toi que le champ existe dans MessageModel

            return message;
        }



        /*public async Task SendMessage(MessageModel message)
        {
            const string sql = "INSERT INTO message (id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoie, id_status_msg) VALUES (@id_expediteur, @id_destinataire, @id_groupe_discussion, @contenu, @date_envoie, @id_status_msg)";

            using var liasonBase = new Connexion().ConnectPostgres();
            liasonBase.Open();

            using var cmd = new NpgsqlCommand(sql, liasonBase);
            cmd.Parameters.AddWithValue("@id_expediteur", message.Id_expediteur);
            cmd.Parameters.AddWithValue("@id_destinataire", (object?)message.Id_destinataire ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_groupe_discussion", (object?)message.Id_groupe_discussion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contenu", message.Contenu);
            cmd.Parameters.AddWithValue("@date_envoie", message.Date_envoie);
            cmd.Parameters.AddWithValue("@id_status_msg", message.Id_status_msg);

            await cmd.ExecuteNonQueryAsync();
        }*/



        /*public async Task SendMessage(NpgsqlConnection liasonBase, MessageModel message)
        {
            Console.WriteLine("ato amin'ny sendMessage");

            Console.WriteLine("==> Début SendMessage");

            // Afficher le contenu de l'objet message
            Console.WriteLine("Contenu du MessageModel :");
            Console.WriteLine($"Id_expediteur: {message.Id_expediteur}");
            Console.WriteLine($"Id_destinataire: {message.Id_destinataire}");
            Console.WriteLine($"Id_groupe_discussion: {message.Id_groupe_discussion}");
            Console.WriteLine($"Contenu: {message.Contenu}");
            Console.WriteLine($"Date_envoie: {message.Date_envoie}");
            Console.WriteLine($"Id_status_msg: {message.Id_status_msg}");
            String sql = "INSERT INTO message (id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoie, id_status_msg) VALUES (@id_expediteur, @id_destinataire, @id_groupe_discussion, @contenu, @date_envoie, @id_status_msg)";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@id_expediteur", message.Id_expediteur);
                cmd.Parameters.AddWithValue("@id_destinataire", (object?)message.Id_destinataire ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id_groupe_discussion", (object?)message.Id_groupe_discussion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@contenu", message.Contenu);
                cmd.Parameters.AddWithValue("@date_envoie", message.Date_envoie);

                cmd.Parameters.AddWithValue("@id_status_msg", message.Id_status_msg);

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("❌ Erreur SQL lors de l'insertion du message : " + e.Message);
                Console.WriteLine(e.StackTrace);
                throw; // important ! sinon SignalR croit que tout s'est bien passé
            }

            finally
            {
                if (liasonBase != null)
                {
                    liasonBase.Close();
                }
            }

        }*/

        public List<DiscussionModel> GetGrpDiscussionByUser(int userId, NpgsqlConnection liasonBase)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();
            String sql = "SELECT id_groupe_discussion, groupe, 'groupe' AS type FROM v_utilisateur_groupe_discussion WHERE id_utilisateur = @userId";

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


        public List<DiscussionModel> GetDiscussionIndividuelleByUser(int userId, NpgsqlConnection liasonBase)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();
            String sql = @"SELECT 
                    CASE 
                        WHEN id_expediteur = @userId THEN id_destinataire
                        ELSE id_expediteur
                    END AS id_autre_utilisateur,
                    
                    CASE 
                        WHEN id_expediteur = @userId THEN nom_destinataire
                        ELSE nom_expediteur
                    END AS nom_autre_utilisateur,
                    
                    'prive' AS type
                FROM v_discussions_individuelles
                WHERE id_expediteur = @userId OR id_destinataire = @userId
                GROUP BY id_autre_utilisateur, nom_autre_utilisateur";

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


        /*public async Task<DiscussionModel> VerifOuCreeDiscussionIndividuelle(NpgsqlConnection liasonBase, int idExpediteur, int idDestinataire)
        {
            var existe = await _context.Message
             .AnyAsync(m =>
                 m.Id_groupe_discussion == null &&
                 ((m.Id_expediteur == idExpediteur && m.Id_destinataire == idDestinataire) ||
                 (m.Id_expediteur == idDestinataire && m.Id_destinataire == idExpediteur))
             );

            if (existe != null)
            {
                var discussions = GetDiscussionIndividuelleByUser(idExpediteur, liasonBase);
                return discussions.FirstOrDefault();
            }
            else
            {
                var newDiscussion = new DiscussionModel
                {
                    Id = m.Id_destinataire,
                    Nom = "Nouvelle Discussion",
                    Type = "prive"
                };
                return newDiscussion;
            }
            
        }*/

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

        public async Task<DiscussionModel> VerifOuCreeDiscussionIndividuelle(NpgsqlConnection connexion,
            int idExpediteur, DiscussionModel discussion)
        {

            Console.WriteLine(discussion.Id + " " + discussion.Nom);
            // var messages = GetDiscussionIndividuelleByUser(idExpediteur, connexion);
            var nouvelleDiscussion = searchDiscussion(connexion, idExpediteur, discussion.Id);

            if (nouvelleDiscussion.Any())
            {

                Console.WriteLine("✅ Discussion trouvée, récupération des messages...");
                var messages = GetDiscussionIndividuelleByUser(idExpediteur, connexion);
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
                SELECT 
                    id_expediteur AS id,
                    'prive' AS type,
                    COUNT(*) AS unread_count
                FROM message
                WHERE id_destinataire = @idUser
                AND id_status_msg = 1
                GROUP BY id_expediteur

                UNION ALL

                SELECT 
                    m.id_groupe_discussion AS id,
                    'groupe' AS type,
                    COUNT(*) AS unread_count
                FROM message msg
                JOIN utilisateur_groupe_discussion m ON m.id_groupe_discussion = msg.id_groupe_discussion
                WHERE m.id_utilisateur = @idUser
                AND msg.id_status_msg = 1
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


        /*public async Task<Dictionary<int, int>> GetUnreadCounts(int idUser)
        {
            const string sql = @"
                SELECT 
                    COALESCE(id_groupe_discussion, id_expediteur) AS id_discussion,
                    COUNT(*) AS unread_count
                FROM message
                WHERE id_destinataire = @idUser
                AND id_status_msg = 1
                GROUP BY COALESCE(id_groupe_discussion, id_expediteur)";

            const string sql = @"
                SELECT id_expediteur AS id_discussion, COUNT(*) AS unread_count
                FROM message
                WHERE id_destinataire = @idUser
                AND id_status_msg = 1
                GROUP BY id_expediteur

                UNION ALL

                SELECT m.id_groupe_discussion AS id_discussion, COUNT(*) AS unread_count
                FROM message msg
                JOIN utilisateur_groupe_discussion m ON m.id_groupe_discussion = msg.id_groupe_discussion
                WHERE m.id_utilisateur = @idUser
                AND msg.id_status_msg = 1
                GROUP BY m.id_groupe_discussion;
            ";

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUser", idUser);

            using var reader = await cmd.ExecuteReaderAsync();
            var result = new Dictionary<int, int>();

            while (await reader.ReadAsync())
            {
                result[reader.GetInt32(0)] = reader.GetInt32(1);
            }

            return result;
        }*/

        public async Task MarkMessagesAsRead(int idUser, int idDiscussion, string type)
        {
            string sql;

            if (type == "prive")
            {
                sql = @"
                    UPDATE message
                SET id_status_msg = 3
                WHERE id_status_msg = 1
                AND id_expediteur = @idDiscussion
                AND id_destinataire = @idUser";
            }
            else if (type == "groupe")
            {
                sql = @"
                    UPDATE message
                    SET id_status_msg = 3
                    WHERE id_groupe_discussion = @idDiscussion
                    AND id_status_msg = 1
                    AND id_expediteur != @idUser"; // facultatif si l'expéditeur ne compte pas ses propres messages
            }
            else
            {
                throw new ArgumentException("Type de discussion inconnu");
            }

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idDiscussion", idDiscussion);

            await cmd.ExecuteNonQueryAsync();
        }


        /*public async Task MarkMessagesAsRead(int idUser, int idDiscussion)
        {
            const string sql = @"
                UPDATE message
                SET id_status_msg = 3
                WHERE id_destinataire = @idUser
                AND COALESCE(id_groupe_discussion, id_expediteur) = @idDiscussion
                AND id_status_msg = 1";

            using var conn = new Connexion().ConnectPostgres();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idDiscussion", idDiscussion);

            await cmd.ExecuteNonQueryAsync();
        }*/

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





    }

}
