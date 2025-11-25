namespace MessagerieInterneAPI
{
    public class EspaceTravailDTO
    {
        public string Nom { get; set; }
        public int? Id_pole { get; set; }
        // public int? Id_admin { get; set; }
        public List<int>? Utilisateurs { get; set; }
    }
}
