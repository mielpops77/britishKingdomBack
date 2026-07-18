namespace British_Kingdom_back.Models
{
    using System;
    using System.Collections.Generic;

    public class Portee
    {
        public string Name { get; set; } = string.Empty;

        public int Id { get; set; }
        public int IdPapa { get; set; }
        public int IdMaman { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfSell { get; set; }

        public List<Chaton> Chatons { get; set; }
        public int ProfilId { get; set; }
        public string UrlProfilMother { get; set; } = string.Empty;
        public string UrlProfilFather { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public bool Archivee { get; set; }
    }
}
