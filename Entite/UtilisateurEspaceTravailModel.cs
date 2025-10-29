using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("utilisateur_espace_travail")]
    public class UtilisateurEspaceTravailModel
    {
        [Key]
        [Column("id_utilisateur_espace_travail")]
        public int Id_utilisateur_espace_travail { get; set; }
        [Column("id_utilisateur")]
        public int Id_utilisateur { get; set; }

        [Column("id_espace_travail")]
        public int Id_espace_travail { get; set; }

        public UtilisateurEspaceTravailModel() { }

        public UtilisateurEspaceTravailModel(int idUserEspaceTravail, int id_utilisateur, int id_espace_travail)
        {
            Id_utilisateur_espace_travail = idUserEspaceTravail;
            Id_utilisateur = id_utilisateur;
            Id_espace_travail = id_espace_travail;
        }
    }
}
