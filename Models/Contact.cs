namespace British_Kingdom_back.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int ProfilId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Num { get; set; } = string.Empty;
        public string Hour { get; set; } = string.Empty;

        public Boolean Vue { get; set; }

        /// <summary>
        /// A-t-on déjà répondu à ce message ? Null veut dire « ne change rien » :
        /// un écran qui met à jour autre chose n'efface pas la réponse sans le vouloir.
        /// </summary>
        public bool? Repondu { get; set; }

        public DateTime DateofCrea { get; set; }

    }
}