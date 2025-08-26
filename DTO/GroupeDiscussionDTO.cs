namespace MessagerieInterneAPI.DTO
{
    public class GroupeDiscussionDTO
    {
        public string Nom { get; set; }
        public string Description { get; set; }
        // public int Id_createur { get; set; }
        // public int Id_espace_travail { get; set; }
        public List<int> Utilisateurs { get; set; }

        public GroupeDiscussionDTO() { }
    }
}

