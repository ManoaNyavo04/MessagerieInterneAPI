using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI.Entite
{
    [Table("role")]
    public class RoleModel
    {
        [Key]
        [Column("id_role")]
        public int Id_role { get; set; }

        [Column("role")]
        public string Role { get; set; }

        public RoleModel() { }

        public RoleModel(int idRole, string role)
        {
            Id_role = idRole;
            Role = role;
        }
    }
}
