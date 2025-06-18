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
            var configJwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(configJwtKey))
            {
                throw new Exception("fichier de configuration Token invalide");
            }
            var key = Encoding.ASCII.GetBytes(configJwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(480),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string userToken = tokenHandler.WriteToken(token);
            return userToken;
            
        }
    }
}
