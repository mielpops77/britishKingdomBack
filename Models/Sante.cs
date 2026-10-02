namespace British_Kingdom_back.Models
{
    using System;

    /// <summary>
    /// Une ligne du carnet de santé : une pesée de chaton ou un vaccin de reproducteur.
    /// Les deux partagent la même table — même forme (un animal, une date, une note),
    /// seules changent les colonnes remplies.
    /// </summary>
    public class Sante
    {
        public int Id { get; set; }
        public int ProfilId { get; set; }

        /// <summary>« chaton » ou « chat » : de quelle table vient l'animal.</summary>
        public string Espece { get; set; } = "chaton";

        /// <summary>L'identifiant du chaton (table Chaton) ou du chat (table Cats).</summary>
        public int AnimalId { get; set; }

        /// <summary>« poids » ou « vaccin ».</summary>
        public string Categorie { get; set; } = "poids";

        /// <summary>Le jour de la pesée ou de l'injection.</summary>
        public DateTime DateFait { get; set; }

        /// <summary>Le poids en grammes (pesées seulement).</summary>
        public int? Poids { get; set; }

        /// <summary>Le nom du vaccin tel que l'éleveur l'écrit (vaccins seulement).</summary>
        public string? Libelle { get; set; }

        /// <summary>La date du prochain rappel, quand elle est connue (vaccins seulement).</summary>
        public DateTime? Rappel { get; set; }

        public string? Note { get; set; }
    }
}
