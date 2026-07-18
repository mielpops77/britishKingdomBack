using Microsoft.AspNetCore.Mvc;
using British_Kingdom_back.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatonController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ChatonController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> GetChatonsByProfilId([FromQuery] int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var chatons = new List<Chaton>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM Chaton WHERE ProfilId = @ProfilId", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var chaton = new Chaton
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                IdPortee = reader.GetInt32(reader.GetOrdinal("IdPortee")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                PorteeName = reader.GetString(reader.GetOrdinal("PorteeName")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Sex = reader.GetString(reader.GetOrdinal("Sex")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                Photos = reader.GetString(reader.GetOrdinal("Photos")).Split(','),
                                UrlProfil = reader.GetString(reader.GetOrdinal("UrlProfil")),
                                Robe = reader.GetString(reader.GetOrdinal("Robe")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Loof = reader.GetBoolean(reader.GetOrdinal("Loof"))

                            };
                            chatons.Add(chaton);
                            // Chargez les chatons en fonction de l'ID de la portée.
                            /*    chatons.Chatons = await LoadChatonsForPorteeAsync(portee.Id, connection);
                               chatons.Add(portee); */
                        }

                    }
                }
            }
            return Ok(chatons);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteChaton(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Supprimez le chaton avec l'ID spécifié
                        using (var deleteChatonCommand = new SqlCommand("DELETE FROM Chaton WHERE Id = @Id", connection, transaction))
                        {
                            deleteChatonCommand.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = deleteChatonCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return NoContent();

                            }
                            else
                            {
                                transaction.Rollback();
                                return NotFound("Le chaton avec l'ID spécifié n'a pas été trouvé.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite : " + ex.Message);
                    }
                }
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChaton(int id, [FromBody] Chaton updatedChaton)
        {
            if (updatedChaton == null)
            {
                return BadRequest("Le chaton à mettre à jour n'a pas été fourni.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Vérifiez si le chaton avec l'ID spécifié existe
                        using (var checkChatonCommand = new SqlCommand("SELECT COUNT(*) FROM Chaton WHERE Id = @Id", connection, transaction))
                        {
                            checkChatonCommand.Parameters.AddWithValue("@Id", id);
                            int existingChatonCount = (int)await checkChatonCommand.ExecuteScalarAsync();

                            if (existingChatonCount == 0)
                            {
                                transaction.Rollback();
                                return NotFound("Le chaton avec l'ID spécifié n'a pas été trouvé.");
                            }
                        }

                        // Mettez à jour le chaton
                        using (var updateChatonCommand = new SqlCommand("UPDATE Chaton SET Name = @Name, Sex = @Sex, Status = @Status, Photos = @Photos, UrlProfil = @UrlProfil, DateOfBirth = @DateOfBirth, PorteeName = @PorteeName, Robe = @Robe, Breed = @Breed, Loof = @Loof WHERE Id = @Id", connection, transaction))
                        {
                            updateChatonCommand.Parameters.AddWithValue("@Id", id);
                            updateChatonCommand.Parameters.AddWithValue("@Name", updatedChaton.Name);
                            updateChatonCommand.Parameters.AddWithValue("@PorteeName", updatedChaton.PorteeName);
                            updateChatonCommand.Parameters.AddWithValue("@Sex", updatedChaton.Sex);
                            updateChatonCommand.Parameters.AddWithValue("@Status", updatedChaton.Status);
                            updateChatonCommand.Parameters.AddWithValue("@Photos", string.Join(",", updatedChaton.Photos));
                            updateChatonCommand.Parameters.AddWithValue("@DateOfBirth", updatedChaton.DateOfBirth);
                            updateChatonCommand.Parameters.AddWithValue("@UrlProfil", updatedChaton.UrlProfil);
                            updateChatonCommand.Parameters.AddWithValue("@Robe", updatedChaton.Robe);
                            updateChatonCommand.Parameters.AddWithValue("@Breed", updatedChaton.Breed);
                            updateChatonCommand.Parameters.AddWithValue("@Loof", updatedChaton.Loof);



                            int rowsAffected = await updateChatonCommand.ExecuteNonQueryAsync();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return NoContent();
                            }
                            else
                            {
                                transaction.Rollback();
                                return StatusCode(500, "La mise à jour du chaton a échoué.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite : " + ex.Message);
                    }
                }
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateChaton([FromBody] Chaton newChaton)
        {
            if (newChaton == null)
            {
                return BadRequest("Les informations du chaton à créer n'ont pas été fournies.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insérez le nouveau chaton
                        using (var createChatonCommand = new SqlCommand("INSERT INTO Chaton (Name, PorteeName,IdPortee , Sex, Status, Photos, DateOfBirth, UrlProfil, ProfilId, Robe, Breed, Loof) VALUES (@Name, @PorteeName, @IdPortee, @Sex, @Status, @Photos, @DateOfBirth, @UrlProfil, @ProfilId, @Robe, @Breed, @Loof); SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            createChatonCommand.Parameters.AddWithValue("@Name", newChaton.Name);
                            createChatonCommand.Parameters.AddWithValue("@PorteeName", newChaton.PorteeName);
                            createChatonCommand.Parameters.AddWithValue("@Sex", newChaton.Sex);
                            createChatonCommand.Parameters.AddWithValue("@Status", newChaton.Status);
                            createChatonCommand.Parameters.AddWithValue("@Photos", string.Join(",", newChaton.Photos));
                            // Convertir la chaîne de date en objet DateTime
                            createChatonCommand.Parameters.AddWithValue("@DateOfBirth", DateTime.ParseExact(newChaton.DateOfBirth.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture));
                            createChatonCommand.Parameters.AddWithValue("@IdPortee", newChaton.IdPortee);
                            createChatonCommand.Parameters.AddWithValue("@UrlProfil", newChaton.UrlProfil);
                            createChatonCommand.Parameters.AddWithValue("@ProfilId", newChaton.ProfilId);
                            createChatonCommand.Parameters.AddWithValue("@Robe", newChaton.Robe);
                            createChatonCommand.Parameters.AddWithValue("@Breed", newChaton.Breed);
                            createChatonCommand.Parameters.AddWithValue("@Loof", newChaton.Loof);

                            // Exécutez la commande et récupérez l'ID du nouveau chaton
                            var newChatonId = await createChatonCommand.ExecuteScalarAsync();
                            newChaton.Id = Convert.ToInt32(newChatonId);

                            transaction.Commit();
                            return CreatedAtAction("GetChatonsByProfilId", new { profilId = newChaton.ProfilId }, newChaton);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite : " + ex.Message);
                    }
                }
            }
        }



    }


}