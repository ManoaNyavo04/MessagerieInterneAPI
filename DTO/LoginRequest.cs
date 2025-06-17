using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MessagerieInterneAPI.Entite;
using Microsoft.IdentityModel.Tokens;

namespace MessagerieInterneAPI
{
    public class LoginRequest
    {
        public string Matricule { get; set; }
        public string Mdp { get; set; }

        public LoginRequest()
        {

        }

        public LoginRequest(String mlle, String motPasse)
        {
            Matricule = mlle;
            Mdp = motPasse;
        }

        public string GenererToken(UtilisateurModel utilisateur, IConfiguration _config)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id_utilisateur.ToString()),
                new Claim(ClaimTypes.Email, utilisateur.Matricule),
                new Claim(ClaimTypes.Role, utilisateur.Id_role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
