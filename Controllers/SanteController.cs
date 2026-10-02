using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using British_Kingdom_back.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace British_Kingdom_back.Controllers
{
    /// <summary>
    /// Le carnet de santé de l'élevage : les pesées des chatons, les vaccins des reproducteurs.
    /// Ces lignes ne sont jamais montrées sur le site public, elles servent à l'éleveur :
    /// tout l'accès demande donc d'être connecté à l'espace de gestion.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SanteController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SanteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // La table est créée au premier appel : pas de migration à lancer à la main sur Azure.
        private static bool _table;

        private static async Task TableAsync(SqlConnection connection)
        {
            if (_table) return;

            using (var creer = new SqlCommand(@"
IF OBJECT_ID('Sante', 'U') IS NULL
BEGIN
    CREATE TABLE Sante (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProfilId INT NOT NULL,
        Espece NVARCHAR(10) NOT NULL,
        AnimalId INT NOT NULL,
        Categorie NVARCHAR(12) NOT NULL,
        DateFait DATE NOT NULL,
        Poids INT NULL,
        Libelle NVARCHAR(80) NULL,
        Rappel DATE NULL,
        Note NVARCHAR(300) NULL,
        CreeLe DATETIME2 NOT NULL CONSTRAINT DF_Sante_CreeLe DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_Sante_Animal ON Sante (ProfilId, Categorie, Espece, AnimalId);
END", connection))
            {
                await creer.ExecuteNonQueryAsync();
            }

            _table = true;
        }

        private static Sante Lire(SqlDataReader reader) => new Sante
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
            Espece = reader.GetString(reader.GetOrdinal("Espece")),
            AnimalId = reader.GetInt32(reader.GetOrdinal("AnimalId")),
            Categorie = reader.GetString(reader.GetOrdinal("Categorie")),
            DateFait = reader.GetDateTime(reader.GetOrdinal("DateFait")),
            Poids = reader.IsDBNull(reader.GetOrdinal("Poids")) ? null : reader.GetInt32(reader.GetOrdinal("Poids")),
            Libelle = reader.IsDBNull(reader.GetOrdinal("Libelle")) ? null : reader.GetString(reader.GetOrdinal("Libelle")),
            Rappel = reader.IsDBNull(reader.GetOrdinal("Rappel")) ? null : reader.GetDateTime(reader.GetOrdinal("Rappel")),
            Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note")),
        };

        /// <summary>Tout le carnet d'un élevage, le plus récent en premier.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int profilId, [FromQuery] string? categorie = null)
        {
            var lignes = new List<Sante>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await TableAsync(connection);

                var requete = "SELECT * FROM Sante WHERE ProfilId = @ProfilId"
                            + (string.IsNullOrWhiteSpace(categorie) ? "" : " AND Categorie = @Categorie")
                            + " ORDER BY DateFait DESC, Id DESC";

                using (var command = new SqlCommand(requete, connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);
                    if (!string.IsNullOrWhiteSpace(categorie))
                        command.Parameters.AddWithValue("@Categorie", categorie.Trim().ToLowerInvariant());

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) lignes.Add(Lire(reader));
                    }
                }
            }

            return Ok(lignes);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Sante ligne)
        {
            var souci = Verifier(ligne);
            if (souci != null) return BadRequest(new { message = souci });

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await TableAsync(connection);

                using (var command = new SqlCommand(@"
INSERT INTO Sante (ProfilId, Espece, AnimalId, Categorie, DateFait, Poids, Libelle, Rappel, Note)
OUTPUT INSERTED.Id
VALUES (@ProfilId, @Espece, @AnimalId, @Categorie, @DateFait, @Poids, @Libelle, @Rappel, @Note)", connection))
                {
                    Remplir(command, ligne);
                    var id = await command.ExecuteScalarAsync();
                    ligne.Id = Convert.ToInt32(id);
                }
            }

            return Ok(ligne);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Sante ligne)
        {
            var souci = Verifier(ligne);
            if (souci != null) return BadRequest(new { message = souci });

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await TableAsync(connection);

                using (var command = new SqlCommand(@"
UPDATE Sante SET Espece = @Espece, AnimalId = @AnimalId, Categorie = @Categorie,
       DateFait = @DateFait, Poids = @Poids, Libelle = @Libelle, Rappel = @Rappel, Note = @Note
WHERE Id = @Id AND ProfilId = @ProfilId", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    Remplir(command, ligne);
                    if (await command.ExecuteNonQueryAsync() == 0) return NotFound();
                }
            }

            ligne.Id = id;
            return Ok(ligne);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await TableAsync(connection);

                using (var command = new SqlCommand("DELETE FROM Sante WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    if (await command.ExecuteNonQueryAsync() == 0) return NotFound();
                }
            }

            return Ok(new { id });
        }

        /// <summary>Ce qu'on refuse d'enregistrer, pour ne pas garder de ligne inutilisable.</summary>
        private static string? Verifier(Sante ligne)
        {
            if (ligne == null) return "Rien à enregistrer.";
            if (ligne.AnimalId <= 0) return "Il manque l'animal.";
            if (ligne.DateFait == default) return "Il manque la date.";

            var categorie = (ligne.Categorie ?? "").Trim().ToLowerInvariant();
            if (categorie != "poids" && categorie != "vaccin") return "Catégorie inconnue.";
            if (categorie == "poids" && (ligne.Poids == null || ligne.Poids <= 0)) return "Il manque le poids.";
            if (categorie == "vaccin" && string.IsNullOrWhiteSpace(ligne.Libelle)) return "Il manque le nom du vaccin.";

            return null;
        }

        private static void Remplir(SqlCommand command, Sante ligne)
        {
            var espece = (ligne.Espece ?? "").Trim().ToLowerInvariant();
            command.Parameters.AddWithValue("@ProfilId", ligne.ProfilId);
            command.Parameters.AddWithValue("@Espece", espece == "chat" ? "chat" : "chaton");
            command.Parameters.AddWithValue("@AnimalId", ligne.AnimalId);
            command.Parameters.AddWithValue("@Categorie", (ligne.Categorie ?? "").Trim().ToLowerInvariant());
            command.Parameters.AddWithValue("@DateFait", ligne.DateFait.Date);
            command.Parameters.AddWithValue("@Poids", (object?)ligne.Poids ?? DBNull.Value);
            command.Parameters.AddWithValue("@Libelle", (object?)Court(ligne.Libelle, 80) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Rappel", (object?)ligne.Rappel?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@Note", (object?)Court(ligne.Note, 300) ?? DBNull.Value);
        }

        private static string? Court(string? texte, int taille)
        {
            if (string.IsNullOrWhiteSpace(texte)) return null;
            texte = texte.Trim();
            return texte.Length <= taille ? texte : texte.Substring(0, taille);
        }
    }
}
