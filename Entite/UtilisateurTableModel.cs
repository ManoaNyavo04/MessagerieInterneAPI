using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("utilisateur")]
    public class UtilisateurTableModel
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
        [Column("id_role")]
        public int Id_role { get; set; }
        
        [Column("mdp")]
        public string Mdp { get; set; }

        public UtilisateurTableModel() { }
        public UtilisateurTableModel(int idUtilisateur, string nom, string prenom, string matricule, int id_role, string mdp)
        {
            Id_utilisateur = idUtilisateur;
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Id_role = id_role;
            Mdp = mdp;
        }
    }
}
