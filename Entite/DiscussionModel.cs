namespace MessagerieInterneAPI.Entite
{
    public class DiscussionModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; } // "groupe" or "prive"

        public DiscussionModel() { }

        public DiscussionModel(int idGrpOrUser, string nomGrpOrUer, string type)
        {
            Id = idGrpOrUser;
            Nom = nomGrpOrUer;
            Type = type; // "groupe" or "prive"
        }
    }
}
