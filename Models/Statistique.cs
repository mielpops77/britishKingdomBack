 public class Statistique
    {
        public int Id { get; set; }
        public int ProfilId { get; set; }
        public int NbrVisitesTotal { get; set; }
        public int NbrVisitesJour { get; set; }
        public DateTime DateVisite { get; set; }

        /// <summary>
        /// Par où le visiteur est arrivé : « TikTok », « Instagram », « Recherche Google »…
        /// Le site le déduit de l'étiquette du lien (utm_source) ou du site référent.
        /// Vide quand on ne sait pas (arrivée directe, favori, lien sans étiquette).
        /// </summary>
        public string? Source { get; set; }
    }