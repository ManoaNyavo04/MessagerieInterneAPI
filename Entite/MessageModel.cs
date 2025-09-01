using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_utilisateur_message")]
    public class MessageModel
    {
        [Key]
        [Column("id_message")]
        public int Id_message { get; set; }
        
        [Column("id_expediteur")]
        public int Id_expediteur { get; set; }
        [Column("nom_expediteur")]
        public string Nom_expediteur { get; set; }
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

        public bool Est_lu { get; set; }

        public MessageModel() { }

        public MessageModel(int id_message, int id_expediteur, String nomExpediteur, int id_destinataire, int id_groupe_discussion, string contenu, DateTime date_envoi, int id_status_msg, bool estLu)
        {
            Id_message = id_message;
            Id_expediteur = id_expediteur;
            Nom_expediteur = nomExpediteur;
            Id_destinataire = id_destinataire;
            Id_groupe_discussion = id_groupe_discussion;
            Contenu = contenu;
            Date_envoie = date_envoi;
            Id_status_msg = id_status_msg;
            Est_lu = estLu;
        }
    }
}
