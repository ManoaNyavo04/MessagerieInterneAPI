using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("message")]
    public class MessageModel
    {
        [Key]
        [Column("id_message")]
        public int Id_message { get; set; }
        
        [Column("id_expediteur")]
        public int Id_expediteur { get; set; }
        [Column("id_destinataire")]
        public int Id_destinataire { get; set; }
        [Column("id_groupe_discussion")]
        public int Id_groupe_discussion { get; set; }
        [Column("contenu")]
        public string Contenu { get; set; }
        [Column("date_envoi")]
        public DateTime Date_envoi { get; set; }
        [Column("id_statut_msg")]
        public int Id_statut_msg { get; set; }

        public MessageModel() { }

        public MessageModel(int id_message, int id_expediteur, int id_destinataire, int id_groupe_discussion, string contenu, DateTime date_envoi, int id_statut_msg)
        {
            Id_message = id_message;
            Id_expediteur = id_expediteur;
            Id_destinataire = id_destinataire;
            Id_groupe_discussion = id_groupe_discussion;
            Contenu = contenu;
            Date_envoi = date_envoi;
            Id_statut_msg = id_statut_msg;
        }
    }
}
