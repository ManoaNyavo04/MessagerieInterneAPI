using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("message")]
    public class MessageTableModel
    {
        [Key]
        [Column("id_message")]
        public int Id_message { get; set; }
        [Column("id_expediteur")]
        public int Id_expediteur { get; set; }
        [Column("id_destinataire")]
        public int? Id_destinataire { get; set; }
        [Column("id_groupe_discussion")]
        public int? Id_groupe_discussion { get; set; }
        [Column("contenu")]
        public string Contenu { get; set; }
        [Column("date_envoie")]
        public DateTime Date_envoie { get; set; }
        [Column("id_status_msg")]
        public int Id_status_msg { get; set; }
        [Column("id_piece_joint")]
        public int? Id_espace_travail { get; set; }
        [Column("date_modification")]
        public DateTime? Date_modification { get; set; }
        [Column("modifiable_jusqua")]
        public DateTime? Modifiable_jusqua { get; set; }

        public MessageTableModel() { }


        public MessageTableModel(int id_message, int id_expediteur, int? id_destinataire, int? id_groupe_discussion, string contenu, DateTime date_envoie, int id_status_msg, int? id_espace_travail, DateTime? date_modification, DateTime? modifiable_jusqua)
        {
            Id_message = id_message;
            Id_expediteur = id_expediteur;
            Id_destinataire = id_destinataire;
            Id_groupe_discussion = id_groupe_discussion;
            Contenu = contenu;
            Date_envoie = date_envoie;
            Id_status_msg = id_status_msg;
            Id_espace_travail = id_espace_travail;
            Date_modification = date_modification;
            Modifiable_jusqua = modifiable_jusqua;
        }
    }
}
