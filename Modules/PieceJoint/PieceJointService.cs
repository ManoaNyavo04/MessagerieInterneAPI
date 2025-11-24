using System.Data;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Modules.Discussion;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MessagerieInterneAPI
{
    public class PieceJointService
    {
        private Connexion connexion = new Connexion();
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly MessageService _serviceMessage;
        public PieceJointService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task AjouterPieceJointe(NpgsqlConnection liaisonbase, int idMessage, int idType, string chemin, string nomOriginal)
        {

            if (liaisonbase == null || liaisonbase.State == ConnectionState.Closed)
            {
                liaisonbase = connexion.ConnectPostgres();
                liaisonbase.Open();
            }

            try
            {
                string sql = @"
                INSERT INTO piece_joint (id_message, id_type_piece_joint, chemin, date_ajout, nom_original)
                VALUES (@idMessage, @idType, @chemin, NOW(), @nomOriginal);";
                using (var cmdCreateur = new NpgsqlCommand(sql, liaisonbase))
                {
                    cmdCreateur.Parameters.AddWithValue("@idMessage", idMessage);
                    cmdCreateur.Parameters.AddWithValue("@idType", idType);
                    cmdCreateur.Parameters.AddWithValue("@chemin", chemin);
                    cmdCreateur.Parameters.AddWithValue("@nomOriginal", nomOriginal);
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

        public async Task<string> UploadGeneric(PieceJointDTO dto, string nomOriginal, string dossier)
        {
            const long maxSize = 25 * 1024 * 1024; // 25 Mo

            if (dto.Fichier.Length > maxSize)
                throw new Exception("Le fichier dépasse la taille maximale autorisée de 25 Mo.");
            var uploadsFolder = Path.Combine(_env.WebRootPath, dossier);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.Fichier.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Fichier.CopyToAsync(stream);
            }

            this.AjouterPieceJointe(
                connexion.ConnectPostgres(),
                dto.IdMessage,
                dto.IdType,
                fileName,
                nomOriginal
            );

            return fileName;
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
                        Type = reader.GetString(5),
                        Nom_original = reader.GetString(6)
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

        public async Task<PieceJointModel> GetPieceJointeAsyncId(int id)
        {
            return await _context.PieceJoint
            .Where(p => p.Id_piece_jointe == id)
            .FirstOrDefaultAsync();

        }

        public async Task<(byte[] bytes, string contentType, string fileName)> GetFileForDownload(int id)
        {
            var pj = await GetPieceJointeAsyncId(id);

            if (pj == null || string.IsNullOrEmpty(pj.Chemin))
                return (null, null, null);

            string dossier = "Uploads";
            string rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            string filePath = Path.Combine(rootPath, dossier, pj.Chemin);

            if (!File.Exists(filePath))
                return (null, null, null);

            var bytes = await File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);

            // 🔥 RÉCUPÉRER EXTENSION DU FICHIER RÉEL
            string ext = Path.GetExtension(pj.Chemin);

            // 🔥 SI nom_original n'a PAS d'extension → on l'ajoute automatiquement
            string fileName = pj.Nom_original;

            if (!fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                fileName += ext;  // ex: "rapport" → "rapport.lsp"

            return (bytes, contentType, fileName);
        }



        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".pdf", "application/pdf"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".txt", "text/plain"}
        };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.GetValueOrDefault(ext, "application/octet-stream");
        }




    }
}
