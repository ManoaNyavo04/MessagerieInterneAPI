using MessagerieInterneAPI.Data;
using Npgsql;

namespace MessagerieInterneAPI
{
    public class ResetService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public ResetService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task ResetMessagerieAsync(int? actifUser)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT reset_messagerie(@current_user_id);", connection);
            command.Parameters.AddWithValue("current_user_id", actifUser ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
