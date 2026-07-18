using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using British_Kingdom_back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatistiqueController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StatistiqueController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> AddVisit(Statistique statistique)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Vérifier si une entrée existe pour cette ProfilId et la date actuelle
                var query = "SELECT COUNT(*) FROM Statistique WHERE ProfilId = @ProfilId AND DateVisite = @DateVisite";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", statistique.ProfilId);
                    command.Parameters.AddWithValue("@DateVisite", DateTime.Today);

                    int existingVisits = (int)command.ExecuteScalar();

                    if (existingVisits == 0)
                    {
                        // Créer une nouvelle entrée dans la table Statistique
                        query = "INSERT INTO Statistique (ProfilId, NbrVisitesTotal, NbrVisitesJour, DateVisite) VALUES (@ProfilId, @NbrVisitesTotal, @NbrVisitesJour, @DateVisite)";
                        command.CommandText = query;
                        command.Parameters.AddWithValue("@NbrVisitesTotal", 1);
                        command.Parameters.AddWithValue("@NbrVisitesJour", 1);
                    }
                    else
                    {
                        // Mettre à jour les valeurs existantes
                        query = "UPDATE Statistique SET NbrVisitesTotal = NbrVisitesTotal + 1, NbrVisitesJour = NbrVisitesJour + 1 WHERE ProfilId = @ProfilId AND DateVisite = @DateVisite";
                        command.CommandText = query;
                    }

                    await command.ExecuteNonQueryAsync();
                }
            }

            return Ok();
        }

      [Authorize]
      [HttpGet("{profilId}")]
public async Task<IActionResult> GetStats(int profilId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    var today = DateTime.Today;

    // Mettre à jour NbrVisitesJour si c'est le début d'un nouveau jour
    await UpdateVisitsIfNeeded(connectionString, profilId, today);

    using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Récupérer les statistiques pour la ProfilId spécifiée
        var query = @"SELECT 
                        SUM(NbrVisitesTotal) AS NbrVisitesTotal, 
                        (SELECT TOP 1 NbrVisitesJour 
                         FROM Statistique 
                         WHERE ProfilId = @ProfilId AND DateVisite = @DateVisite) AS NbrVisitesJour 
                      FROM Statistique 
                      WHERE ProfilId = @ProfilId";
        
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ProfilId", profilId);
            command.Parameters.AddWithValue("@DateVisite", today); // Filtre pour la date d'aujourd'hui

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var stats = new
                    {
                        NbrVisitesTotal = reader.IsDBNull(reader.GetOrdinal("NbrVisitesTotal")) ? 0 : reader.GetInt32(reader.GetOrdinal("NbrVisitesTotal")),
                        NbrVisitesJour = reader.IsDBNull(reader.GetOrdinal("NbrVisitesJour")) ? 0 : reader.GetInt32(reader.GetOrdinal("NbrVisitesJour"))
                    };

                    return Ok(stats);
                }
                else
                {
                    return NotFound(new { message = "Statistiques non trouvées pour la ProfilId spécifiée." });
                }
            }
        }
    }
}



        private async Task UpdateVisitsIfNeeded(string connectionString, int profilId, DateTime currentDate)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT NbrVisitesJour FROM Statistique WHERE ProfilId = @ProfilId AND DateVisite = @CurrentDate";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);
                    command.Parameters.AddWithValue("@CurrentDate", currentDate);

                    var result = await command.ExecuteScalarAsync();

                    // Si aucune entrée n'existe pour cette ProfilId et la date actuelle, insérer une nouvelle entrée avec NbrVisitesJour initialisé à 0
                    if (result == null || result == DBNull.Value)
                    {
                        query = "INSERT INTO Statistique (ProfilId, NbrVisitesTotal, NbrVisitesJour, DateVisite) VALUES (@ProfilId, 0, 0, @CurrentDate)";
                        command.CommandText = query;
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
