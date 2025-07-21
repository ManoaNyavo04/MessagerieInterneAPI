using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("message")]
    public class MessageModel
    {
        public int Id_message { get; set; }
        
        public int Id_expediteur { get; set; }
        public int Id_destinataire { get; set; }
        public int Id_groupe_discussion { get; set; }
        public string Contenu { get; set; }
        public DateTime Date_envoi { get; set; }
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
