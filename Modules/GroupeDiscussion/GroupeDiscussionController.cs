using System.Security.Claims;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.DTO;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupeDiscussionController : ControllerBase
    {
        private Connexion connexion = new Connexion();

        private readonly GroupeDiscussionService _service;

        public GroupeDiscussionController(GroupeDiscussionService service)
        {
            _service = service;
        }

        [HttpGet("mesGrpDiscu")]
        public async Task<IActionResult> GetGrpDiscuByUser()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany: " + idUtilisateur);
            var liaisonBase = connexion.ConnectPostgres();
            var mesGrp = _service.GetUtilisateurGrpDiscu(liaisonBase, idUtilisateur);

            return Ok(mesGrp);
        }

        [HttpPost("creerGroupe")]
        public async Task<IActionResult> CreateGroupeDiscussion([FromBody] GroupeDiscussionDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Le groupe de discussion ne peut pas être null.");
            }
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (groupe discussion): " + idUtilisateur);

            await _service.CreerGroupe(connexion.ConnectPostgres(), dto, idUtilisateur);
            return Ok(new { message = "Groupe créé avec succès ", dto.Nom });

        }

        [HttpGet("getMembresGroupeDiscussion")]
        public async Task<IActionResult> GetMembresGroupeDiscussion(int idGroupeDiscussion)
        {
            if (idGroupeDiscussion <= 0)
            {
                return BadRequest("L'ID du groupe de discussion est invalide.");
                
            }
            var membres = await _service.GetMembresGroupe(connexion.ConnectPostgres(), idGroupeDiscussion);

            return Ok(membres);
        }

        [HttpPost("ajouterNouveauMembre")]
        public async Task<IActionResult> AjouterNouveauMembre(int idGroupe, [FromBody] GroupeDiscussionDTO dto) {
            if (dto.Utilisateurs == null || !dto.Utilisateurs.Any())
            {
                return BadRequest("Aucun membre à ajouter.");
            }

            try
            {
                await _service.AjouterNouveauxMembres(connexion.ConnectPostgres(), idGroupe, dto.Utilisateurs);
                return Ok(new { message = "Membres ajoutés avec succès." });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Erreur lors de l'ajout des membres.");
            }
        }
    }
}
