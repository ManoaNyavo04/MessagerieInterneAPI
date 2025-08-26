using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_utilisateur_groupe_discussion")]
    public class UtilisateurGroupeDiscussionModel
    {
        [Column("id_groupe_discussion")]
        public int Id_groupe_discussion { get; set; }
        [Column("groupe")]
        public String Groupe { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("id_utilisateur")]
        public int Id_utilisateur { get; set; }
        
        [Column("matricule")]
        public String Matricule { get; set; }
        [Column("nom")]
        public String Nom { get; set; }
        [Column("prenom")]
        public String Prenom { get; set; }
        [Column("est_admin")]
        public bool Est_admin { get; set; }
        [Column("statuts")]
        public int Statuts { get; set; }
        [Column("type")]
        public string Type { get; set; }

        public UtilisateurGroupeDiscussionModel() { }
        public UtilisateurGroupeDiscussionModel(int idGroupe,String nomGroupe, String descri, int utilisateur,String matricule,  String nom, String prenom, bool estAdmin, int statuts, String type)
        {
            Id_utilisateur = utilisateur;
            Id_groupe_discussion = idGroupe;
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Groupe = nomGroupe;
            Description = descri;
            Est_admin = estAdmin;
            Statuts = statuts;
            Type = type;
        }
    }
}

