namespace MessagerieInterneAPI.Entite
{
    public class DiscussionModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; } // "groupe" | "prive"

        // ✅ NOUVEAU
        public string? Matricule { get; set; }

        public DiscussionModel() { }

        public DiscussionModel(int id, string nom, string type, string? matricule = null)
        {
            Id = id;
            Nom = nom;
            Type = type;
            Matricule = matricule;
        }
    }

}
