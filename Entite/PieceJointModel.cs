using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_piece_joint")]
    public class PieceJointModel
    {
        [Key]
        [Column("id_piece_jointe")]
        public int Id_piece_jointe { get; set; }
        [Column("id_message")]
        public int Id_message { get; set; }
        [Column("id_type_piece_joint")]
        public int Id_type_piece_jointe { get; set; }
        [Column("chemin")]
        public string Chemin { get; set; }
        [Column("date_ajout")]
        public DateTime Date_ajout { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("nom_original")]
        public string Nom_original { get; set; }

        public PieceJointModel() { }

        public PieceJointModel(int id_piece_jointe, int id_message, int id_type_piece_jointe, String chemin, DateTime date_ajout, String type, String nom_original)
        {
            Id_piece_jointe = id_piece_jointe;
            Id_message = id_message;
            Id_type_piece_jointe = id_type_piece_jointe;
            Chemin = chemin;
            Date_ajout = date_ajout;
            Type = type;
            Nom_original = nom_original;
        }
    }
}
