using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, DateTime> _activeVisitors = new ConcurrentDictionary<string, DateTime>();
        private const int OnlineThresholdSeconds = 90;

        // La colonne VisitLog.Source existe-t-elle ? Vérifié une fois, puis gardé en mémoire.
        // En cas d'échec on retente dix minutes plus tard : un hoquet de la base ne doit pas
        // priver le site de la provenance jusqu'au prochain redémarrage.
        private static bool _colonneSource;
        private static DateTime _colonneSourceVue = DateTime.MinValue;
        private static readonly TimeSpan _colonneSourceRetard = TimeSpan.FromMinutes(10);

        /// <summary>
        /// S'assure que VisitLog a sa colonne Source, en la créant au besoin.
        /// Si la base refuse (droits, verrou…), on renvoie false et tout continue comme avant :
        /// une visite ne doit jamais être perdue à cause de la provenance.
        /// </summary>
        private static async Task<bool> ColonneSourceAsync(SqlConnection connection)
        {
            if (_colonneSource) return true;
            if (DateTime.UtcNow - _colonneSourceVue < _colonneSourceRetard) return false;
            _colonneSourceVue = DateTime.UtcNow;

            try
            {
                using (var verifie = new SqlCommand("SELECT COL_LENGTH('VisitLog', 'Source')", connection))
                {
                    var trouvee = await verifie.ExecuteScalarAsync();
                    if (trouvee != null && trouvee != DBNull.Value)
                    {
                        _colonneSource = true;
                        return true;
                    }
                }

                using (var ajoute = new SqlCommand("ALTER TABLE VisitLog ADD Source NVARCHAR(60) NULL", connection))
                {
                    await ajoute.ExecuteNonQueryAsync();
                }

                _colonneSource = true;
            }
            catch
            {
                _colonneSource = false;
            }

            return _colonneSource;
        }

        /// <summary>
        /// Nettoie ce que le site a envoyé : un nom court et lisible, ou rien du tout.
        /// </summary>
        private static string? NettoyerSource(string? brut)
        {
            if (string.IsNullOrWhiteSpace(brut)) return null;

            var propre = new string(brut.Trim()
                .Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == '-' || c == '_' || c == '\'' || c == 'é' || c == 'è' || c == 'ê' || c == 'à' || c == 'ç')
                .ToArray())
                .Trim();

            if (propre.Length == 0) return null;
            return propre.Length > 60 ? propre.Substring(0, 60) : propre;
        }

        public StatistiqueController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetClientIp()
        {
            string raw;

            if (Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
                raw = cfIp.ToString();
            else if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
                raw = forwardedFor.ToString().Split(',')[0].Trim();
            else
                raw = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // X-Forwarded-For peut contenir "ip:port" pour de l'IPv4 ; on retire le port
            var colonIndex = raw.IndexOf(':');
            if (colonIndex > 0 && raw.Count(c => c == ':') == 1)
                raw = raw.Substring(0, colonIndex);

            return raw;
        }

        private bool IsLikelyBot(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return true;
            var lower = userAgent.ToLowerInvariant();
            string[] botSignatures = { "bot", "crawl", "spider", "slurp", "curl", "wget", "python", "scrapy", "headless", "monitor", "uptime", "facebookexternalhit" };
            return botSignatures.Any(sig => lower.Contains(sig));
        }

        private string DescribeUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Inconnu";
            var lower = userAgent.ToLowerInvariant();

            if (lower.Contains("googlebot")) return "Googlebot";
            if (lower.Contains("bingbot")) return "Bingbot";
            if (lower.Contains("ahrefsbot")) return "AhrefsBot";
            if (lower.Contains("semrushbot")) return "SemrushBot";
            if (IsLikelyBot(userAgent)) return "Robot";

            var device = lower.Contains("iphone") ? "iPhone"
                : lower.Contains("ipad") ? "iPad"
                : lower.Contains("android") ? "Android"
                : "Ordinateur";

            var browser = lower.Contains("edg/") ? "Edge"
                : lower.Contains("chrome") ? "Chrome"
                : lower.Contains("firefox") ? "Firefox"
                : lower.Contains("safari") ? "Safari"
                : "navigateur inconnu";

            return $"{device} ({browser})";
        }

        private async Task<string> ResolveLocationAsync(string ip)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ip}?fields=status,city,country");
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var status) && status.GetString() == "success")
                {
                    var city = root.TryGetProperty("city", out var c) ? c.GetString() : null;
                    var country = root.TryGetProperty("country", out var co) ? co.GetString() : null;

                    if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
                        return $"{city}, {country}";
                    if (!string.IsNullOrEmpty(country))
                        return country;
                }
            }
            catch
            {
                // Géolocalisation indisponible, on continue sans bloquer l'enregistrement de la visite
            }

            return "Inconnu";
        }

        [HttpPost]
        public async Task<IActionResult> AddVisit(Statistique statistique)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var ip = GetClientIp();
            var userAgent = Request.Headers.TryGetValue("User-Agent", out var ua) ? ua.ToString() : string.Empty;
            var isBot = IsLikelyBot(userAgent);
            var today = DateTime.Today;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Vérifier si cette IP a déjà visité aujourd'hui, pour ne pas fausser les compteurs
                bool isNewVisitorToday;
                var checkIpQuery = "SELECT COUNT(*) FROM VisitLog WHERE ProfilId = @ProfilId AND VisitorIp = @VisitorIp AND CAST(VisitedAt AS DATE) = CAST(GETUTCDATE() AS DATE)";
                using (var checkIpCommand = new SqlCommand(checkIpQuery, connection))
                {
                    checkIpCommand.Parameters.AddWithValue("@ProfilId", statistique.ProfilId);
                    checkIpCommand.Parameters.AddWithValue("@VisitorIp", ip);
                    var existingForIp = (int)await checkIpCommand.ExecuteScalarAsync();
                    isNewVisitorToday = existingForIp == 0;
                }

                if (isNewVisitorToday && !isBot)
                {
                    var query = "SELECT COUNT(*) FROM Statistique WHERE ProfilId = @ProfilId AND DateVisite = @DateVisite";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProfilId", statistique.ProfilId);
                        command.Parameters.AddWithValue("@DateVisite", today);

                        int existingVisits = (int)await command.ExecuteScalarAsync();

                        if (existingVisits == 0)
                        {
                            query = "INSERT INTO Statistique (ProfilId, NbrVisitesTotal, NbrVisitesJour, DateVisite) VALUES (@ProfilId, @NbrVisitesTotal, @NbrVisitesJour, @DateVisite)";
                            command.CommandText = query;
                            command.Parameters.AddWithValue("@NbrVisitesTotal", 1);
                            command.Parameters.AddWithValue("@NbrVisitesJour", 1);
                        }
                        else
                        {
                            query = "UPDATE Statistique SET NbrVisitesTotal = NbrVisitesTotal + 1, NbrVisitesJour = NbrVisitesJour + 1 WHERE ProfilId = @ProfilId AND DateVisite = @DateVisite";
                            command.CommandText = query;
                        }

                        await command.ExecuteNonQueryAsync();
                    }
                }

                var location = await ResolveLocationAsync(ip);
                var source = NettoyerSource(statistique.Source);
                var avecSource = source != null && await ColonneSourceAsync(connection);

                var logQuery = avecSource
                    ? "INSERT INTO VisitLog (ProfilId, VisitedAt, VisitorIp, Location, UserAgent, IsBot, Source) VALUES (@ProfilId, @VisitedAt, @VisitorIp, @Location, @UserAgent, @IsBot, @Source)"
                    : "INSERT INTO VisitLog (ProfilId, VisitedAt, VisitorIp, Location, UserAgent, IsBot) VALUES (@ProfilId, @VisitedAt, @VisitorIp, @Location, @UserAgent, @IsBot)";
                using (var logCommand = new SqlCommand(logQuery, connection))
                {
                    logCommand.Parameters.AddWithValue("@ProfilId", statistique.ProfilId);
                    logCommand.Parameters.AddWithValue("@VisitedAt", DateTime.UtcNow);
                    logCommand.Parameters.AddWithValue("@VisitorIp", ip);
                    logCommand.Parameters.AddWithValue("@Location", location);
                    logCommand.Parameters.AddWithValue("@UserAgent", (object?)userAgent.Substring(0, Math.Min(userAgent.Length, 500)) ?? DBNull.Value);
                    logCommand.Parameters.AddWithValue("@IsBot", isBot);
                    if (avecSource) logCommand.Parameters.AddWithValue("@Source", source!);
                    await logCommand.ExecuteNonQueryAsync();
                }
            }

            return Ok();
        }

      [Authorize]
      [HttpGet("recent/{profilId}")]
      public async Task<IActionResult> GetRecentVisits(int profilId, [FromQuery] int limit = 20)
      {
          var connectionString = _configuration.GetConnectionString("DefaultConnection");

          using (var connection = new SqlConnection(connectionString))
          {
              await connection.OpenAsync();

              var avecSource = await ColonneSourceAsync(connection);
              var query = avecSource
                  ? "SELECT TOP (@Limit) VisitedAt, VisitorIp, Location, UserAgent, IsBot, Source FROM VisitLog WHERE ProfilId = @ProfilId ORDER BY VisitedAt DESC"
                  : "SELECT TOP (@Limit) VisitedAt, VisitorIp, Location, UserAgent, IsBot FROM VisitLog WHERE ProfilId = @ProfilId ORDER BY VisitedAt DESC";
              using (var command = new SqlCommand(query, connection))
              {
                  command.Parameters.AddWithValue("@ProfilId", profilId);
                  command.Parameters.AddWithValue("@Limit", limit);

                  var visits = new System.Collections.Generic.List<object>();
                  using (var reader = await command.ExecuteReaderAsync())
                  {
                      while (await reader.ReadAsync())
                      {
                          var userAgent = reader.IsDBNull(reader.GetOrdinal("UserAgent")) ? "" : reader.GetString(reader.GetOrdinal("UserAgent"));
                          var visitedAtUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("VisitedAt")), DateTimeKind.Utc);
                          visits.Add(new
                          {
                              // Sérialisé en string à la main : le DateTimeConverter global tronque les DateTime à "yyyy-MM-dd"
                              // (voulu pour les dates de naissance), ce qui écrasait l'heure de chaque visite.
                              VisitedAt = visitedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                              IsBot = reader.GetBoolean(reader.GetOrdinal("IsBot")),
                              Device = DescribeUserAgent(userAgent),
                              Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                              VisitorIp = reader.IsDBNull(reader.GetOrdinal("VisitorIp")) ? null : reader.GetString(reader.GetOrdinal("VisitorIp")),
                              Source = !avecSource || reader.IsDBNull(reader.GetOrdinal("Source")) ? null : reader.GetString(reader.GetOrdinal("Source"))
                          });
                      }
                  }

                  return Ok(visits);
              }
          }
      }

      [Authorize]
      [HttpGet("daily/{profilId}")]
      public async Task<IActionResult> GetDailyStats(int profilId, [FromQuery] int days = 14)
      {
          var connectionString = _configuration.GetConnectionString("DefaultConnection");
          var startDate = DateTime.Today.AddDays(-(days - 1));

          var countsByDate = new System.Collections.Generic.Dictionary<DateTime, int>();

          using (var connection = new SqlConnection(connectionString))
          {
              await connection.OpenAsync();

              var query = "SELECT DateVisite, NbrVisitesJour FROM Statistique WHERE ProfilId = @ProfilId AND DateVisite >= @StartDate";
              using (var command = new SqlCommand(query, connection))
              {
                  command.Parameters.AddWithValue("@ProfilId", profilId);
                  command.Parameters.AddWithValue("@StartDate", startDate);

                  using (var reader = await command.ExecuteReaderAsync())
                  {
                      while (await reader.ReadAsync())
                      {
                          countsByDate[reader.GetDateTime(reader.GetOrdinal("DateVisite")).Date] = reader.GetInt32(reader.GetOrdinal("NbrVisitesJour"));
                      }
                  }
              }
          }

          var result = new System.Collections.Generic.List<object>();
          for (var d = startDate; d <= DateTime.Today; d = d.AddDays(1))
          {
              result.Add(new { Date = d, Count = countsByDate.TryGetValue(d, out var c) ? c : 0 });
          }

          return Ok(result);
      }

      [Authorize]
      [HttpGet("locations/{profilId}")]
      public async Task<IActionResult> GetTopLocations(int profilId, [FromQuery] int days = 30, [FromQuery] int limit = 5)
      {
          var connectionString = _configuration.GetConnectionString("DefaultConnection");
          var startDate = DateTime.UtcNow.AddDays(-days);

          using (var connection = new SqlConnection(connectionString))
          {
              await connection.OpenAsync();

              var query = @"SELECT TOP (@Limit) Location, COUNT(*) AS Cnt
                            FROM VisitLog
                            WHERE ProfilId = @ProfilId AND VisitedAt >= @StartDate AND Location IS NOT NULL AND Location <> 'Inconnu' AND IsBot = 0
                            GROUP BY Location
                            ORDER BY Cnt DESC";
              using (var command = new SqlCommand(query, connection))
              {
                  command.Parameters.AddWithValue("@ProfilId", profilId);
                  command.Parameters.AddWithValue("@StartDate", startDate);
                  command.Parameters.AddWithValue("@Limit", limit);

                  var locations = new System.Collections.Generic.List<object>();
                  using (var reader = await command.ExecuteReaderAsync())
                  {
                      while (await reader.ReadAsync())
                      {
                          locations.Add(new
                          {
                              Location = reader.GetString(reader.GetOrdinal("Location")),
                              Count = reader.GetInt32(reader.GetOrdinal("Cnt"))
                          });
                      }
                  }

                  return Ok(locations);
              }
          }
      }

      /// <summary>
      /// Par quel réseau les visiteurs sont arrivés, sur les derniers jours.
      /// </summary>
      [Authorize]
      [HttpGet("sources/{profilId}")]
      public async Task<IActionResult> GetTopSources(int profilId, [FromQuery] int days = 30, [FromQuery] int limit = 8)
      {
          var connectionString = _configuration.GetConnectionString("DefaultConnection");
          var startDate = DateTime.UtcNow.AddDays(-days);

          using (var connection = new SqlConnection(connectionString))
          {
              await connection.OpenAsync();

              if (!await ColonneSourceAsync(connection))
                  return Ok(new System.Collections.Generic.List<object>());

              var query = @"SELECT TOP (@Limit) Source, COUNT(*) AS Cnt
                            FROM VisitLog
                            WHERE ProfilId = @ProfilId AND VisitedAt >= @StartDate AND Source IS NOT NULL AND Source <> '' AND IsBot = 0
                            GROUP BY Source
                            ORDER BY Cnt DESC";
              using (var command = new SqlCommand(query, connection))
              {
                  command.Parameters.AddWithValue("@ProfilId", profilId);
                  command.Parameters.AddWithValue("@StartDate", startDate);
                  command.Parameters.AddWithValue("@Limit", limit);

                  var sources = new System.Collections.Generic.List<object>();
                  using (var reader = await command.ExecuteReaderAsync())
                  {
                      while (await reader.ReadAsync())
                      {
                          sources.Add(new
                          {
                              Source = reader.GetString(reader.GetOrdinal("Source")),
                              Count = reader.GetInt32(reader.GetOrdinal("Cnt"))
                          });
                      }
                  }

                  return Ok(sources);
              }
          }
      }

      [HttpPost("heartbeat")]
      public IActionResult Heartbeat([FromBody] Statistique statistique)
      {
          var ip = GetClientIp();
          var userAgent = Request.Headers.TryGetValue("User-Agent", out var ua) ? ua.ToString() : string.Empty;

          if (!IsLikelyBot(userAgent))
          {
              var key = $"{statistique.ProfilId}:{ip}";
              _activeVisitors[key] = DateTime.UtcNow;
          }

          return Ok();
      }

      [Authorize]
      [HttpGet("online/{profilId}")]
      public IActionResult GetOnlineCount(int profilId)
      {
          var cutoff = DateTime.UtcNow.AddSeconds(-OnlineThresholdSeconds);

          foreach (var entry in _activeVisitors)
          {
              if (entry.Value < cutoff)
              {
                  _activeVisitors.TryRemove(entry.Key, out _);
              }
          }

          var prefix = $"{profilId}:";
          var count = _activeVisitors.Keys.Count(k => k.StartsWith(prefix));

          return Ok(new { online = count });
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
