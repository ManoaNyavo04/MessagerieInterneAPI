using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("pole")]
    public class PoleModel
    {
        [Key]
        [Column("id_pole")]
        public int Id_pole { get; set; }

        [Column("pole")]
        public string Pole { get; set; }

        public PoleModel() { }

        public PoleModel(int id_pole, string pole)
        {
            Id_pole = id_pole;
            Pole = pole;
        }
    }
}
