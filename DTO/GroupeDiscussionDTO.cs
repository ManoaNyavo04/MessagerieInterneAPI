namespace MessagerieInterneAPI.DTO
{
    public class GroupeDiscussionDTO
    {
        public string Nom { get; set; }
        public string Description { get; set; }
        public List<int> Utilisateurs { get; set; }

        public GroupeDiscussionDTO() { }
    }
}

