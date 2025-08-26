using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI.Entite
{
    [Table("v_info_utilisateur")]
    public class UtilisateurModel
    {
        [Key]
        [Column("id_utilisateur")]
        public int Id_utilisateur { get; set; }
        [Column("nom")]
        public string Nom { get; set; }
        [Column("prenom")]
        public string Prenom { get; set; }
        [Column("matricule")]
        public string Matricule { get; set; }
        
        [Column("mdp")]
        public string Mdp { get; set; }
        [Column("id_role")]
        public int Id_role { get; set; }
        [Column("role")]
        public string Role { get; set; } // Added to hold the role name

        public UtilisateurModel()
        {


        }

        public UtilisateurModel(int user, string nom, string prenom, string matricule,string mdp,  int role, string roleName)
        {
            Id_utilisateur = user;
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Mdp = mdp;
            Id_role = role;
            Role = roleName;
        
        }


    }
}
