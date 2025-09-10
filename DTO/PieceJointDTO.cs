using System.ComponentModel.DataAnnotations;

namespace MessagerieInterneAPI
{
    public class PieceJointDTO
    {
        [Required]
        public IFormFile Fichier { get; set; }

        [Required]
        public int IdMessage { get; set; }
    }
}
