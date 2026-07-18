namespace British_Kingdom_back.Models
{
    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProfilId { get; set; } 

        public string Robe { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Breed { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string UrlProfil { get; set; } = string.Empty;
        public string UrlProfilMother { get; set; } = string.Empty;
        public string UrlProfilFather { get; set; } = string.Empty;
        public string sailliesExterieures { get; set; } = string.Empty;
        public string Pedigree { get; set; } = string.Empty;
        public bool Archivee { get; set; }

         public string[]? Images { get; set; }


  

    }
}

