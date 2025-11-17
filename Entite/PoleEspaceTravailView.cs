using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_pole_espace_travail")]
    public class PoleEspaceTravailView
    {
        [Column("id_espace_travail")]
        public int? IdEspaceTravail { get; set; }
        [Column("nom")]
        public string Nom { get; set; }
        [Column("id_pole")]
        public int? IdPole { get; set; }
        [Column("id_admin")]
        public int? IdAdmin { get; set; }
        [Column("pole")]
        public string Pole { get; set; }

        public PoleEspaceTravailView() { }
        public PoleEspaceTravailView( int? idespacetravail, string nom, int? idpole, int? admin, string pole)
        {
            IdPole = idpole;
            Pole = pole;
            IdEspaceTravail = idespacetravail;
            Nom = nom;
            IdAdmin = admin;
        }
    }
}
