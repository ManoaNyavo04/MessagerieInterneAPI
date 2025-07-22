using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Npgsql;

namespace MessagerieInterneAPI.Modules.Discussion
{
    public class MessageService
    {
        public List<MessageModel> GetMessagesByGroupId(NpgsqlConnection liasonBase, int groupId)
        {
            List<MessageModel> messages = new List<MessageModel>();
            String sql = "SELECT * FROM message WHERE id_groupe_discussion = @id_groupe_discussion";

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
                        Date_envoi = reader.GetDateTime(5),
                        Id_statut_msg = reader.GetInt32(6)
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
            List<MessageModel> messages = new List<MessageModel>();
            String sql = @"SELECT *
                            FROM message
                            WHERE id_groupe_discussion IS NULL
                            AND (
                                (id_expediteur = @exp AND id_destinataire = @dest) OR
                                (id_expediteur = @dest AND id_destinataire = @exp)
                            )
                            ORDER BY date_envoie;
                            ";

            if (liasonBase == null || liasonBase.State == ConnectionState.Closed)
            {
                Connexion connexion = new Connexion();
                liasonBase = connexion.ConnectPostgres();
                liasonBase.Open();
            }

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liasonBase);
                cmd.Parameters.AddWithValue("@exp", exp);
                cmd.Parameters.AddWithValue("@dest", dest);
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
                        Date_envoi = reader.GetDateTime(5),
                        Id_statut_msg = reader.GetInt32(6)
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

        public void SendMessage(NpgsqlConnection liasonBase, MessageModel message)
        {
            String sql = "INSERT INTO message (id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoi, id_statut_msg) VALUES (@id_expediteur, @id_destinataire, @id_groupe_discussion, @contenu, @date_envoi, @id_statut_msg)";

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
                cmd.Parameters.AddWithValue("@id_destinataire", message.Id_destinataire);

                cmd.Parameters.AddWithValue("@id_groupe_discussion", message.Id_groupe_discussion);
                cmd.Parameters.AddWithValue("@contenu", message.Contenu);
                cmd.Parameters.AddWithValue("@date_envoi", message.Date_envoi);

                cmd.Parameters.AddWithValue("@id_statut_msg", message.Id_statut_msg);

                cmd.ExecuteNonQuery();
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

        }

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
            String sql = "SELECT id_utilisateur, nom, 'prive' AS type FROM v_discussions_individuelles WHERE id_utilisateur = @userId";

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
