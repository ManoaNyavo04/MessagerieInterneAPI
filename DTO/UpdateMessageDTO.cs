namespace MessagerieInterneAPI
{
    public class UpdateMessageDTO
    {
        public int Id_message { get; set; }
        public int IdUtilisateur { get; set; }
        public string NouveauContenu { get; set; }
    }
}
