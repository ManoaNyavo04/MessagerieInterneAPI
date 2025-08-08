using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MessagerieInterneAPI.Modules.Role
{
    
    public class RoleService
    {
        private Connexion connexion = new Connexion();

        public List<RoleModel> GetAllRoles(NpgsqlConnection liaisonbase)
        {
            List<RoleModel> allRoles = new List<RoleModel>();

            String sql = "SELECT * FROM role";
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
                    RoleModel role = new RoleModel();
                    role.Id_role = (reader.GetInt32(0));
                    role.Role = (reader.GetString(1));
                    allRoles.Add(role);
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

            return allRoles;
        }
    }
}
