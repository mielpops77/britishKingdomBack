using Microsoft.AspNetCore.Mvc;
using British_Kingdom_back.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PorteeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PorteeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> GetPorteesByProfilId([FromQuery] int profilId)
        {
            Response.Headers.CacheControl = "no-store";
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var portees = new List<Portee>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM Portee WHERE ProfilId = @ProfilId", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var portee = new Portee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                IdPapa = reader.GetInt32(reader.GetOrdinal("IdPapa")),
                                IdMaman = reader.GetInt32(reader.GetOrdinal("IdMaman")),
                                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                DateOfSell = reader.GetDateTime(reader.GetOrdinal("DateOfSell")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                UrlProfilFather = reader.GetString(reader.GetOrdinal("UrlProfilFather")),
                                UrlProfilMother = reader.GetString(reader.GetOrdinal("UrlProfilMother")),
                                Disponible = reader.GetBoolean(reader.GetOrdinal("Disponible")),
                                Archivee = reader.GetBoolean(reader.GetOrdinal("Archivee")),
                                ExternalFatherName = reader.GetString(reader.GetOrdinal("ExternalFatherName")),
                                ExternalFatherPhoto = reader.GetString(reader.GetOrdinal("ExternalFatherPhoto"))
                            };

                            // Chargez les chatons en fonction de l'ID de la portée.
                            portee.Chatons = await LoadChatonsForPorteeAsync(portee.Id, connection);
                            portees.Add(portee);
                        }
                    }
                }
            }
            return Ok(portees);
        }

        private async Task<List<Chaton>> LoadChatonsForPorteeAsync(int porteeId, SqlConnection connection)
        {
            using (var command = new SqlCommand("SELECT * FROM Chaton WHERE IdPortee = @PorteeId", connection))
            {
                command.Parameters.AddWithValue("@PorteeId", porteeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var chatons = new List<Chaton>();

                    while (await reader.ReadAsync())
                    {
                        var chaton = new Chaton
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            IdPortee = reader.GetInt32(reader.GetOrdinal("IdPortee")),
                            ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            PorteeName = reader.GetString(reader.GetOrdinal("PorteeName")),
                            Sex = reader.GetString(reader.GetOrdinal("Sex")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Photos = reader.IsDBNull(reader.GetOrdinal("Photos")) ? null : reader.GetString(reader.GetOrdinal("Photos")).Split(','),
                            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                            UrlProfil = reader.GetString(reader.GetOrdinal("UrlProfil")),
                            Robe = reader.GetString(reader.GetOrdinal("Robe")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Loof = reader.GetBoolean(reader.GetOrdinal("Loof")),

                        };
                        chatons.Add(chaton);
                    }

                    return chatons;
                }
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortee(int id)
        {
            Response.Headers.CacheControl = "no-store";
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Remplacez la requête suivante par votre requête SQL réelle pour récupérer une portée par ID.
                using (var command = new SqlCommand("SELECT * FROM Portee WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var portee = new Portee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                IdPapa = reader.GetInt32(reader.GetOrdinal("IdPapa")),
                                IdMaman = reader.GetInt32(reader.GetOrdinal("IdMaman")),
                                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                DateOfSell = reader.GetDateTime(reader.GetOrdinal("DateOfSell")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                // Chargez les chatons en fonction de l'ID de la portée (à implémenter).
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                UrlProfilFather = reader.GetString(reader.GetOrdinal("UrlProfilFather")),
                                UrlProfilMother = reader.GetString(reader.GetOrdinal("UrlProfilMother")),
                                Disponible = reader.GetBoolean(reader.GetOrdinal("Disponible")),
                                Archivee = reader.GetBoolean(reader.GetOrdinal("Archivee")),
                                ExternalFatherName = reader.GetString(reader.GetOrdinal("ExternalFatherName")),
                                ExternalFatherPhoto = reader.GetString(reader.GetOrdinal("ExternalFatherPhoto"))
                            };
                            portee.Chatons = await LoadChatonsForPorteeAsync(portee.Id, connection);
                            return Ok(portee);
                        }
                        else
                        {
                            return NotFound(); // La portée avec l'ID spécifié n'a pas été trouvée.
                        }
                    }
                }
            }
        }

 [HttpGet("GetPorteesByParentId")]
public async Task<IActionResult> GetPorteesByParentId(int parentId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Remplacez la requête suivante par votre requête SQL réelle pour récupérer une portée par ID du papa ou de la maman.
        using (var command = new SqlCommand("SELECT * FROM Portee WHERE IdPapa = @ParentId OR IdMaman = @ParentId", connection))
        {
            command.Parameters.AddWithValue("@ParentId", parentId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                List<Portee> portees = new List<Portee>();

                while (await reader.ReadAsync())
                {
                    var portee = new Portee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        IdPapa = reader.GetInt32(reader.GetOrdinal("IdPapa")),
                        IdMaman = reader.GetInt32(reader.GetOrdinal("IdMaman")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        DateOfSell = reader.GetDateTime(reader.GetOrdinal("DateOfSell")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                        UrlProfilFather = reader.GetString(reader.GetOrdinal("UrlProfilFather")),
                        UrlProfilMother = reader.GetString(reader.GetOrdinal("UrlProfilMother")),
                        Disponible = reader.GetBoolean(reader.GetOrdinal("Disponible")),
                        Archivee = reader.GetBoolean(reader.GetOrdinal("Archivee")),
                        ExternalFatherName = reader.GetString(reader.GetOrdinal("ExternalFatherName")),
                        ExternalFatherPhoto = reader.GetString(reader.GetOrdinal("ExternalFatherPhoto"))
                    };
                    portee.Chatons = await LoadChatonsForPorteeAsync(portee.Id, connection);
                    portees.Add(portee);
                }

                return Ok(portees); // Retourne la liste des portées (même si elle est vide)
            }
        }
    }
}



        [Authorize]
        [HttpPost]
        public IActionResult CreatePortee(Portee portee)
        {
            if (portee == null)
            {
                return BadRequest("Portée data is missing.");
            }

            var isExternalFather = portee.IdPapa <= 0 && !string.IsNullOrWhiteSpace(portee.ExternalFatherName);

            if ((portee.IdPapa <= 0 && !isExternalFather) || portee.IdMaman <= 0 || portee.ProfilId <= 0)
            {
                return BadRequest("Invalid values for IdPapa, IdMaman, or ProfilId.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            int newPorteeId;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insérer la portée
                        using (var command = new SqlCommand("INSERT INTO Portee (IdPapa, IdMaman, DateOfBirth, DateOfSell, ProfilId, UrlProfilFather, UrlProfilMother, Name, Disponible, Archivee, ExternalFatherName, ExternalFatherPhoto) VALUES (@IdPapa, @IdMaman, @DateOfBirth, @DateOfSell, @ProfilId, @UrlProfilFather, @UrlProfilMother, @Name, @Disponible, @Archivee, @ExternalFatherName, @ExternalFatherPhoto); SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdPapa", portee.IdPapa);
                            command.Parameters.AddWithValue("@IdMaman", portee.IdMaman);
                            command.Parameters.AddWithValue("@DateOfBirth", portee.DateOfBirth);
                            command.Parameters.AddWithValue("@DateOfSell", portee.DateOfSell);
                            command.Parameters.AddWithValue("@ProfilId", portee.ProfilId);
                            command.Parameters.AddWithValue("@name", portee.Name);
                            command.Parameters.AddWithValue("@Disponible", portee.Disponible);
                            command.Parameters.AddWithValue("@Archivee", portee.Archivee);
                            command.Parameters.AddWithValue("@ExternalFatherName", isExternalFather ? portee.ExternalFatherName : string.Empty);
                            command.Parameters.AddWithValue("@ExternalFatherPhoto", isExternalFather ? (portee.ExternalFatherPhoto ?? string.Empty) : string.Empty);

                            // Utilisation de GetParentUrl (le père externe utilise sa propre photo, pas une fiche chat)
                            var fatherUrl = isExternalFather ? (portee.ExternalFatherPhoto ?? string.Empty) : GetParentUrl(connection, transaction, portee.IdPapa);
                            var motherUrl = GetParentUrl(connection, transaction, portee.IdMaman);

                            command.Parameters.AddWithValue("@UrlProfilFather", fatherUrl);
                            command.Parameters.AddWithValue("@UrlProfilMother", motherUrl);



                            newPorteeId = Convert.ToInt32(command.ExecuteScalar());
                            Console.WriteLine("New Portee ID: " + newPorteeId);

                            if (newPorteeId <= 0)
                            {
                                transaction.Rollback();
                                return StatusCode(500, "La création de la portée a échoué.");
                            }

                            // 2. Insérer les chatons
                            foreach (var chaton in portee.Chatons)
                            {
                                using (var chatonCommand = new SqlCommand("INSERT INTO Chaton (IdPortee, ProfilId, Name, Sex, Status, Photos, UrlProfil, DateOfBirth, PorteeName, Robe, Breed, Loof ) OUTPUT INSERTED.Id VALUES (@IdPortee, @ProfilId, @Name, @Sex, @Status, @Photos, @UrlProfil, @DateOfBirth, @PorteeName, @Robe, @Breed, @Loof);", connection, transaction))
                                {
                                    chatonCommand.Parameters.AddWithValue("@IdPortee", newPorteeId);
                                    chatonCommand.Parameters.AddWithValue("@ProfilId", portee.ProfilId);
                                    chatonCommand.Parameters.AddWithValue("@Name", chaton.Name);
                                    chatonCommand.Parameters.AddWithValue("@PorteeName", chaton.PorteeName);
                                    chatonCommand.Parameters.AddWithValue("@Sex", chaton.Sex);
                                    chatonCommand.Parameters.AddWithValue("@Status", chaton.Status);
                                    chatonCommand.Parameters.AddWithValue("@Photos", string.Join(",", chaton.Photos ?? Array.Empty<string>()));
                                    chatonCommand.Parameters.AddWithValue("@UrlProfil", chaton.UrlProfil);
                                    chatonCommand.Parameters.AddWithValue("@DateOfBirth", chaton.DateOfBirth);
                                    chatonCommand.Parameters.AddWithValue("@Robe", chaton.Robe);
                                    chatonCommand.Parameters.AddWithValue("@Breed", chaton.Breed);
                                    chatonCommand.Parameters.AddWithValue("@Loof", chaton.Loof);
                                    int newChatonId = Convert.ToInt32(chatonCommand.ExecuteScalar());
                                    Console.WriteLine("New Chaton ID: " + newChatonId);
                                    chaton.Id = newChatonId;

                                    if (newChatonId <= 0)
                                    {
                                        transaction.Rollback();
                                        return StatusCode(500, "La création des chatons a échoué.");
                                    }
                                }
                            }

                            transaction.Commit();
                            portee.Id = newPorteeId;
                            return CreatedAtAction(nameof(GetPortee), new { id = newPorteeId }, portee);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite." + ex.Message);
                    }
                }
            }
        }


        private string GetParentUrl(SqlConnection connection, SqlTransaction transaction, int parentId)
        {
            // Query the database to get the URL for the parent based on parentId
            using (var command = new SqlCommand("SELECT UrlProfil FROM Cats WHERE Id = @Id", connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", parentId);
                var url = command.ExecuteScalar()?.ToString();
                return url ?? string.Empty;
            }
        }




        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdatePortee(int id, Portee portee)
        {
            if (portee == null)
            {
                return BadRequest("Portée data is missing.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Mettre à jour la portée
                        using (var command = new SqlCommand(
                            "UPDATE Portee SET IdPapa = @IdPapa, IdMaman = @IdMaman, DateOfSell = @DateOfSell,  DateOfBirth = @DateOfBirth, UrlProfilFather = @UrlProfilFather, UrlProfilMother = @UrlProfilMother, Name = @Name, Disponible = @Disponible, Archivee = @Archivee, ProfilId = @ProfilId, ExternalFatherName = @ExternalFatherName, ExternalFatherPhoto = @ExternalFatherPhoto WHERE Id = @Id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.Parameters.AddWithValue("@IdPapa", portee.IdPapa);
                            command.Parameters.AddWithValue("@IdMaman", portee.IdMaman);
                            command.Parameters.AddWithValue("@Name", portee.Name);
                            command.Parameters.AddWithValue("@DateOfBirth", portee.DateOfBirth);
                            command.Parameters.AddWithValue("@DateOfSell", portee.DateOfSell);
                            command.Parameters.AddWithValue("@ProfilId", portee.ProfilId);
                            command.Parameters.AddWithValue("@UrlProfilFather", portee.UrlProfilFather);
                            command.Parameters.AddWithValue("@UrlProfilMother", portee.UrlProfilMother);
                            command.Parameters.AddWithValue("@Disponible", portee.Disponible);
                            command.Parameters.AddWithValue("@Archivee", portee.Archivee);
                            command.Parameters.AddWithValue("@ExternalFatherName", portee.ExternalFatherName ?? string.Empty);
                            command.Parameters.AddWithValue("@ExternalFatherPhoto", portee.ExternalFatherPhoto ?? string.Empty);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected <= 0)
                            {
                                transaction.Rollback();
                                return BadRequest(new { message = "La mise à jour de la portée a échoué." });
                            }
                        }

                        // 2. Mettre à jour les chatons
                        foreach (var chaton in portee.Chatons)
                        {
                            using (var chatonCommand = new SqlCommand(
                                "UPDATE Chaton SET Name = @Name, Sex = @Sex, Status = @Status, Photos = @Photos, UrlProfil = @UrlProfil, DateOfBirth = @DateOfBirth, PorteeName = @PorteeName, Robe = @Robe, Breed = @Breed, Loof = @Loof  WHERE Id = @Id", connection, transaction))
                            {
                                chatonCommand.Parameters.AddWithValue("@Id", chaton.Id);
                                chatonCommand.Parameters.AddWithValue("@Name", chaton.Name);
                                chatonCommand.Parameters.AddWithValue("@Sex", chaton.Sex);
                                chatonCommand.Parameters.AddWithValue("@Status", chaton.Status);
                                chatonCommand.Parameters.AddWithValue("@Photos", string.Join(",", chaton.Photos ?? Array.Empty<string>()));
                                chatonCommand.Parameters.AddWithValue("@UrlProfil", chaton.UrlProfil);
                                chatonCommand.Parameters.AddWithValue("@DateOfBirth", chaton.DateOfBirth);
                                chatonCommand.Parameters.AddWithValue("@PorteeName", chaton.PorteeName);
                                chatonCommand.Parameters.AddWithValue("@Robe", chaton.Robe);
                                chatonCommand.Parameters.AddWithValue("@Breed", chaton.Breed);
                                chatonCommand.Parameters.AddWithValue("@Loof", chaton.Loof);

                                int chatonRowsAffected = chatonCommand.ExecuteNonQuery();

                                if (chatonRowsAffected <= 0)
                                {
                                    transaction.Rollback();
                                    return BadRequest(new { message = "La mise à jour du chaton a échoué." });
                                }
                            }
                        }

                        // Si tout s'est bien passé, commit la transaction
                        transaction.Commit();
                        return Ok(new { message = "Portée et chatons mis à jour avec succès." });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite." + ex.Message);
                    }
                }
            }
        }




        [Authorize]
        [HttpPatch("{id}/archive")]
        public IActionResult SetArchiveStatus(int id, [FromBody] ArchiveRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Portee SET Archivee = @Archivee WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Archivee", request.Archivee);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected <= 0)
                        return NotFound();

                    return Ok();
                }
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeletePortee(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Supprimer d'abord les chatons de la portée
                        using (var deleteChatonsCommand = new SqlCommand("DELETE FROM Chaton WHERE IdPortee = @IdPortee", connection, transaction))
                        {
                            deleteChatonsCommand.Parameters.AddWithValue("@IdPortee", id);
                            int chatonsDeleted = deleteChatonsCommand.ExecuteNonQuery();
                        }

                        // Ensuite, supprimer la portée elle-même
                        using (var deletePorteeCommand = new SqlCommand("DELETE FROM Portee WHERE Id = @Id", connection, transaction))
                        {
                            deletePorteeCommand.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = deletePorteeCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return NoContent();
                                // return Ok("Portée et chatons supprimés avec succès.");
                            }
                            else
                            {
                                transaction.Rollback();
                                return BadRequest("La suppression de la portée a échoué.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite." + ex.Message);
                    }
                }
            }
        }

    }

    public class ArchiveRequest
    {
        public bool Archivee { get; set; }
    }
}
