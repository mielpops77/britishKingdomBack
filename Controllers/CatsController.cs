using Microsoft.AspNetCore.Mvc;
using British_Kingdom_back.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CatsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateCat(Cat cat)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            int newId;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("INSERT INTO Cats (Name, ProfilId, Robe, EyeColor, Sex, Breed, DateOfBirth, UrlProfil, UrlProfilMother, UrlProfilFather, sailliesExterieures, Pedigree, Images, Archivee) OUTPUT INSERTED.ID VALUES (@Name, @ProfilId, @Robe, @EyeColor, @Sex, @Breed, @DateOfBirth, @UrlProfil, @UrlProfilMother, @UrlProfilFather, @sailliesExterieures, @Pedigree, @Images, @Archivee)", connection))
                {
                    // Ajout des paramètres comme avant
                    command.Parameters.AddWithValue("@Name", cat.Name);
                    command.Parameters.AddWithValue("@ProfilId", cat.ProfilId);
                    command.Parameters.AddWithValue("@Robe", cat.Robe);
                    command.Parameters.AddWithValue("@EyeColor", cat.EyeColor);
                    command.Parameters.AddWithValue("@DateOfBirth", cat.DateOfBirth);
                    command.Parameters.AddWithValue("@Sex", cat.Sex);
                    command.Parameters.AddWithValue("@Breed", cat.Breed);
                    command.Parameters.AddWithValue("@UrlProfil", cat.UrlProfil);
                    command.Parameters.AddWithValue("@UrlProfilMother", cat.UrlProfilMother);
                    command.Parameters.AddWithValue("@UrlProfilFather", cat.UrlProfilFather);
                    command.Parameters.AddWithValue("@sailliesExterieures", cat.sailliesExterieures);
                    command.Parameters.AddWithValue("@Pedigree", cat.Pedigree);
                    command.Parameters.AddWithValue("@Images", string.Join(",", cat.Images ?? Array.Empty<string>()));
                    command.Parameters.AddWithValue("@Archivee", cat.Archivee);

                    // Exécuter la commande et récupérer l'identifiant généré
                    newId = (int)command.ExecuteScalar();
                }
            }

            cat.Id = newId;
            return CreatedAtAction(nameof(GetAllCats), cat);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCats(int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var cats = new List<Cat>();
            string query = "SELECT * FROM Cats WHERE ProfilId = @ProfilId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var cat = new Cat
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Robe = reader.GetString(reader.GetOrdinal("Robe")),
                                EyeColor = reader.GetString(reader.GetOrdinal("EyeColor")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Sex = reader.GetString(reader.GetOrdinal("Sex")),
                                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                UrlProfil = reader.GetString(reader.GetOrdinal("UrlProfil")),
                                UrlProfilMother = reader.GetString(reader.GetOrdinal("UrlProfilMother")),
                                UrlProfilFather = reader.GetString(reader.GetOrdinal("UrlProfilFather")),
                                sailliesExterieures = reader.GetString(reader.GetOrdinal("sailliesExterieures")),
                                Pedigree = reader.GetString(reader.GetOrdinal("Pedigree")),
                                Images = reader.GetString(reader.GetOrdinal("Images")).Split(','),
                                Archivee = reader.GetBoolean(reader.GetOrdinal("Archivee"))
                            };
                            cats.Add(cat);
                        }
                    }
                }
            }

            return Ok(cats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatById(int id, int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = "SELECT * FROM Cats WHERE Id = @Id AND ProfilId = @ProfilId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var cat = new Cat
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Robe = reader.GetString(reader.GetOrdinal("Robe")),
                                EyeColor = reader.GetString(reader.GetOrdinal("EyeColor")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Sex = reader.GetString(reader.GetOrdinal("Sex")),
                                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                UrlProfil = reader.GetString(reader.GetOrdinal("UrlProfil")),
                                UrlProfilMother = reader.GetString(reader.GetOrdinal("UrlProfilMother")),
                                UrlProfilFather = reader.GetString(reader.GetOrdinal("UrlProfilFather")),
                                sailliesExterieures = reader.GetString(reader.GetOrdinal("sailliesExterieures")),
                                Pedigree = reader.GetString(reader.GetOrdinal("Pedigree")),
                                Images = reader.GetString(reader.GetOrdinal("Images")).Split(','),
                                Archivee = reader.GetBoolean(reader.GetOrdinal("Archivee"))
                            };

                            return Ok(cat);
                        }
                        else
                        {
                            return NotFound(new { message = "Chat non trouvé" });
                        }
                    }
                }
            }
        }


        [Authorize]
        [HttpPatch("{id}/archive")]
        public IActionResult SetArchiveStatus(int id, [FromBody] CatArchiveRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Cats SET Archivee = @Archivee WHERE Id = @Id", connection))
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
        public IActionResult DeleteCat(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM Cats WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Chat supprimé avec succès" });
                    }
                    else
                    {
                        return NotFound(new { message = "Chat non trouvé" });
                    }
                }
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateCat(int id, Cat cat)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "UPDATE Cats " + "SET Name = @Name, ProfilId = @ProfilId, Robe = @Robe, EyeColor = @EyeColor, Sex = @Sex, Breed = @Breed, " +
                    "DateOfBirth = @DateOfBirth, UrlProfil = @UrlProfil, UrlProfilMother = @UrlProfilMother, " +
                    "UrlProfilFather = @UrlProfilFather, sailliesExterieures = @sailliesExterieures, Pedigree = @Pedigree," +
                    "Images = @Images, Archivee = @Archivee WHERE Id = @Id", connection))
                {
                    // Ajoutez les paramètres comme avant
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Name", cat.Name);
                    command.Parameters.AddWithValue("@ProfilId", cat.ProfilId);
                    command.Parameters.AddWithValue("@Robe", cat.Robe);
                    command.Parameters.AddWithValue("@EyeColor", cat.EyeColor);
                    command.Parameters.AddWithValue("@DateOfBirth", cat.DateOfBirth);
                    command.Parameters.AddWithValue("@Sex", cat.Sex);
                    command.Parameters.AddWithValue("@Breed", cat.Breed);
                    command.Parameters.AddWithValue("@UrlProfil", cat.UrlProfil);
                    command.Parameters.AddWithValue("@UrlProfilMother", cat.UrlProfilMother);
                    command.Parameters.AddWithValue("@UrlProfilFather", cat.UrlProfilFather);
                    command.Parameters.AddWithValue("@sailliesExterieures", cat.sailliesExterieures);
                    command.Parameters.AddWithValue("@Pedigree", cat.Pedigree);
                    command.Parameters.AddWithValue("@Images", string.Join(",", cat.Images ?? Array.Empty<string>()));
                    command.Parameters.AddWithValue("@Archivee", cat.Archivee);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Code pour mettre à jour les informations de la portée si le chat est le père ou la mère d'une portée
                            UpdatePorteeInformation(id, cat.UrlProfil, connection);

                            return Ok(new { message = "Chat mis à jour avec succès" });
                        }
                        else
                        {
                            return NotFound(new { message = "Chat non trouvé" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message = "Une erreur s'est produite lors de la mise à jour du chat", error = ex.Message });
                    }
                }
            }
        }

        // Méthode pour mettre à jour les informations de la portée si nécessaire
        private void UpdatePorteeInformation(int catId, string newProfileUrl, SqlConnection connection)
        {
            // Obtenir le sexe du chat que nous mettons à jour
            string sex = GetSexById(catId, connection);

            // Définir le parentType en fonction du sexe du chat
            var parentType = (sex == "Femelle") ? "Maman" : "Papa";

            var parentColumn = (parentType == "Maman") ? "IdMaman" : "IdPapa";
            var parentProfileColumn = (parentType == "Maman") ? "UrlProfilMother" : "UrlProfilFather";

            // Affiche le sexe du parent que nous modifions
            Console.WriteLine($"Updating Portee {parentProfileColumn} for {parentType} ({sex}) with ID {catId} to {newProfileUrl}");

            var porteeQuery = $"UPDATE Portee SET {parentProfileColumn} = @UrlProfil WHERE {parentColumn} = @CatId";

            using (var porteeCommand = new SqlCommand(porteeQuery, connection))
            {
                porteeCommand.Parameters.AddWithValue("@UrlProfil", newProfileUrl);
                porteeCommand.Parameters.AddWithValue("@CatId", catId);

                int rowsAffected = porteeCommand.ExecuteNonQuery();
                Console.WriteLine($"Rows affected by Portee update: {rowsAffected}");
            }
        }

        private string GetSexById(int catId, SqlConnection connection)
        {
            string sex = string.Empty;

            using (var command = new SqlCommand("SELECT Sex FROM Cats WHERE Id = @CatId", connection))
            {
                command.Parameters.AddWithValue("@CatId", catId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sex = reader["Sex"].ToString();
                    }
                }
            }

            return sex;
        }







    }

    public class CatArchiveRequest
    {
        public bool Archivee { get; set; }
    }
}


// ... Autres actions ...

