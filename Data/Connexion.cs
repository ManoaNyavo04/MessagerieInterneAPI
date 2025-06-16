using Npgsql;
using System;

namespace MessagerieInterneAPI.Data
{
    public class Connexion
    {
        public NpgsqlConnection ConnectPostgres()
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=pareramada;Database=messagerie;";
            try
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la connexion à PostgreSQL : {ex.Message}");
            }
            return null;
        }
    }
}
