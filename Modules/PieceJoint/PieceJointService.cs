using System.Data;
using MessagerieInterneAPI.Data;
using Npgsql;

namespace MessagerieInterneAPI
{
    public class PieceJointService
    {
        private Connexion connexion = new Connexion();
        public async Task AjouterPieceJointe(NpgsqlConnection liaisonbase, int idMessage, int idType, string chemin)
        {

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                string sql = @"
                INSERT INTO piece_joint (id_message, id_type_piece_joint, chemin, date_ajout)
                VALUES (@idMessage, @idType, @chemin, NOW());";
                using (var cmdCreateur = new NpgsqlCommand(sql, liaisonbase))
                {
                    cmdCreateur.Parameters.AddWithValue("@idMessage", idMessage);
                    cmdCreateur.Parameters.AddWithValue("@idType", idType);
                    cmdCreateur.Parameters.AddWithValue("@chemin", chemin);
                    await cmdCreateur.ExecuteNonQueryAsync();
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

        public async Task<int> GetOrCreateType(NpgsqlConnection liaisonbase, string typeMime, int taille)
        {
            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                // 🔍 Vérifie si le type existe déjà
                string selectSql = @"
            SELECT id_type_piece_joint 
            FROM type_piece_joint 
            WHERE type = @type AND taille = @taille
            LIMIT 1;
        ";

                using (var selectCmd = new NpgsqlCommand(selectSql, liaisonbase))
                {
                    selectCmd.Parameters.AddWithValue("@type", typeMime);
                    selectCmd.Parameters.AddWithValue("@taille", taille);

                    var result = await selectCmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }

                // ➕ Sinon, insère le type
                string insertSql = @"
            INSERT INTO type_piece_joint (type, taille)
            VALUES (@type, @taille)
            RETURNING id_type_piece_joint;
        ";

                using (var insertCmd = new NpgsqlCommand(insertSql, liaisonbase))
                {
                    insertCmd.Parameters.AddWithValue("@type", typeMime);
                    insertCmd.Parameters.AddWithValue("@taille", taille);

                    var insertedId = await insertCmd.ExecuteScalarAsync();
                    return Convert.ToInt32(insertedId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur GetOrCreateType : " + ex.Message);
                return -1;
            }
            finally
            {
                if (liaisonbase != null)
                    liaisonbase.Close();
            }
        }

        public async Task<List<PieceJointModel>> GetPieceJointeParMessage(NpgsqlConnection liaisonbase, int idMessage)
        {
            string sql = @"
        select * from v_piece_joint
        where id_message = @idmessage;";

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            List<PieceJointModel> pieces = new List<PieceJointModel>();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, liaisonbase);
                cmd.Parameters.AddWithValue("@idmessage", idMessage);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PieceJointModel pj = new PieceJointModel
                    {
                        Id_piece_jointe = reader.GetInt32(0),
                        Id_message = reader.GetInt32(1),
                        Id_type_piece_jointe = reader.GetInt32(2),
                        Chemin = reader.GetString(3),
                        Date_ajout = reader.GetDateTime(4),
                        Type = reader.GetString(5)
                    };

                    pieces.Add(pj);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur GetPieceJointeParMessage : " + e.Message);
            }
            finally
            {
                if (liaisonbase != null)
                {
                    liaisonbase.Close();
                }
            }

            return pieces;
        }


    }
}
