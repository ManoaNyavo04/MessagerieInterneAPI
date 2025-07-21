using System.Data;
using MessagerieInterneAPI.Data;
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
                while(reader.Read()){
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
    }
}
