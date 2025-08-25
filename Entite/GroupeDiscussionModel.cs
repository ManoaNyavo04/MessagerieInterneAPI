using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI.Entite
{
    [Table("groupe_discussion")]
    public class GroupeDiscussionModel
    {
        [Key]
        [Column("id_groupe_discussion")]
        public int Id_groupe_discussion { get; set; }
        [Column("nom")]
        public string Nom { get; set; }
        [Column("id_espace_travail")]
        public int Id_espace_travail { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("date_creation")]
        public DateTime Date_creation { get; set; }
        [Column("id_createur")]
        public int Id_createur { get; set; }

        public GroupeDiscussionModel() { }

        public GroupeDiscussionModel(int id_groupe_discussion, string nom, int id_espace_travail, string description, DateTime date_creation, int id_createur)
        {
            Id_groupe_discussion = id_groupe_discussion;
            Nom = nom;
            Id_espace_travail = id_espace_travail;
            Description = description;
            Date_creation = date_creation;
            Id_createur = id_createur;
        }

    }
}
