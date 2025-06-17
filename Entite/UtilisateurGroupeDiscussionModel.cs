using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_utilisateur_groupe_discussion")]
    public class UtilisateurGroupeDiscussionModel
    {
        [Column("id_utilisateur")]
        public int Id_utilisateur { get; set; }
        [Column("id_groupe_discussion")]
        public int Id_groupe_discussion { get; set; }
        [Column("nom")]
        public String Nom { get; set; }
        [Column("prenom")]
        public String Prenom { get; set; }
        [Column("matricule")]
        public String Matricule { get; set; }
        [Column("groupe")]
        public String Groupe { get; set; }

        public UtilisateurGroupeDiscussionModel() { }
        public UtilisateurGroupeDiscussionModel(int utilisateur, int idGroupe, String nom, String prenom, String matricule, String nomGroupe)
        {
            Id_utilisateur = utilisateur;
            Id_groupe_discussion = idGroupe;
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Groupe = nomGroupe;
        }
    }
}

