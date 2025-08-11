using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Npgsql;

namespace MessagerieInterneAPI.Modules.Discussion
{
    public class MessageService
    {
        private readonly NpgsqlDataSource _dataSource;

        public MessageService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public MessageService()
        {
        }

        public List<MessageModel> GetMessagesByGroupId(NpgsqlConnection liasonBase, int groupId)
        {
            List<MessageModel> messages = new List<MessageModel>();
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
                                Id_destinataire = reader.GetInt32(2),
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

        public async Task SendMessage(MessageModel message)
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
        }



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
    }
}
