namespace MessagerieInterneAPI.Entite
{
    public class UtilisateurModel
    {
        public int Id_utilisateur { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Matricule { get; set; }
        public int Id_role { get; set; }
        public string Mdp { get; set; }

        public UtilisateurModel()
        {

        }

        public UtilisateurModel(int user, string nom, string prenom, string matricule, int role, string mdp)
        {
            Id_utilisateur = user;
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Id_role = role;
            Mdp = mdp;
        }
        

    }
}
