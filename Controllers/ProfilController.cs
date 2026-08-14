using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace VotreNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProfilController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public IActionResult CreateProfil(Profil profil)
        {
            if (profil == null)
            {
                return BadRequest(new { message = "Profil data is missing." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("INSERT INTO Profil (FirstName, LastName, PhoneNumber, UserType, Siren, Facebook, Instagram, Twitter, Youtube, Tiktok, Email, DomaineName, profilId) VALUES (@FirstName, @LastName, @PhoneNumber, @UserType, @Siren, @Facebook, @Instagram, @Twitter, @Youtube, @Tiktok, @Email, @DomaineName, @ProfilId);", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profil.ProfilId);
                    command.Parameters.AddWithValue("@FirstName", profil.FirstName);
                    command.Parameters.AddWithValue("@LastName", profil.LastName);
                    command.Parameters.AddWithValue("@PhoneNumber", profil.PhoneNumber);
                    command.Parameters.AddWithValue("@UserType", profil.UserType);
                    command.Parameters.AddWithValue("@Siren", profil.Siren);
                    command.Parameters.AddWithValue("@Facebook", profil.Facebook);
                    command.Parameters.AddWithValue("@Instagram", profil.Instagram);
                    command.Parameters.AddWithValue("@Twitter", profil.Twitter);
                    command.Parameters.AddWithValue("@Youtube", profil.Youtube);
                    command.Parameters.AddWithValue("@Tiktok", profil.Tiktok);
                    command.Parameters.AddWithValue("@Email", profil.Email);
                    command.Parameters.AddWithValue("@DomaineName", profil.DomaineName);


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Profil ajouté avec succès." });
                    }
                    else
                    {
                        return BadRequest(new { message = "L'insertion a échoué." });
                    }
                }
            }
        }

        // ... Les autres actions GetProfils et GetProfil restent inchangées.


        [HttpGet]
        public IActionResult GetProfils(int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var profils = new List<Profil>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Remplacez la requête suivante par votre requête SQL réelle pour récupérer tous les profils.
                using (var command = new SqlCommand("SELECT * FROM Profil WHERE ProfilId = @ProfilId", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var profil = new Profil
                            {
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                UserType = reader.GetString(reader.GetOrdinal("UserType")),
                                Siren = reader.GetString(reader.GetOrdinal("Siren")),
                                Facebook = reader.GetString(reader.GetOrdinal("Facebook")),
                                Instagram = reader.GetString(reader.GetOrdinal("Instagram")),
                                Twitter = reader.GetString(reader.GetOrdinal("Twitter")),
                                Youtube = reader.GetString(reader.GetOrdinal("Youtube")),
                                Tiktok = reader.GetString(reader.GetOrdinal("Tiktok")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                DomaineName = reader.GetString(reader.GetOrdinal("DomaineName")),
                            };

                            // Ajoutez d'autres propriétés ici en fonction de votre table Profil.

                            profils.Add(profil);
                        }

                        return Ok(profils);
                    }
                }
            }
        }


        [HttpGet("{id}")]
        public IActionResult GetProfil(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Remplacez la requête suivante par votre requête SQL réelle pour récupérer un profil par ID.
                using (var command = new SqlCommand("SELECT * FROM Profil WHERE ProfilId = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var profil = new Profil
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                UserType = reader.GetString(reader.GetOrdinal("UserType")),
                                Siren = reader.GetString(reader.GetOrdinal("Siren")),
                                Facebook = reader.GetString(reader.GetOrdinal("Facebook")),
                                Instagram = reader.GetString(reader.GetOrdinal("Instagram")),
                                Twitter = reader.GetString(reader.GetOrdinal("Twitter")),
                                Youtube = reader.GetString(reader.GetOrdinal("Youtube")),
                                Tiktok = reader.GetString(reader.GetOrdinal("Tiktok")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                DomaineName = reader.GetString(reader.GetOrdinal("DomaineName")),

                            };

                            // Ajoutez d'autres propriétés ici en fonction de votre table Profil.

                            return Ok(profil);
                        }
                        else
                        {
                            return NotFound(); // Le profil avec l'ID spécifié n'a pas été trouvé.
                        }
                    }
                }
            }
        }

        // Résout un slug d'URL (ex: "love-of-british" dans eleveurconnect.fr/love-of-british)
        // vers le profil correspondant. On réutilise la colonne DomaineName comme slug de démo :
        // il suffit d'enregistrer DomaineName = "love-of-british" sur le profil de l'éleveur.
        // Aucune modification du schéma de base n'est nécessaire.
        [HttpGet("by-slug/{slug}")]
        public IActionResult GetProfilBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest(new { message = "Slug manquant." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Comparaison insensible à la casse et aux espaces de début/fin.
                using (var command = new SqlCommand("SELECT * FROM Profil WHERE LTRIM(RTRIM(DomaineName)) = @Slug", connection))
                {
                    command.Parameters.AddWithValue("@Slug", slug.Trim());

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var profil = new Profil
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                UserType = reader.GetString(reader.GetOrdinal("UserType")),
                                Siren = reader.GetString(reader.GetOrdinal("Siren")),
                                Facebook = reader.GetString(reader.GetOrdinal("Facebook")),
                                Instagram = reader.GetString(reader.GetOrdinal("Instagram")),
                                Twitter = reader.GetString(reader.GetOrdinal("Twitter")),
                                Youtube = reader.GetString(reader.GetOrdinal("Youtube")),
                                Tiktok = reader.GetString(reader.GetOrdinal("Tiktok")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                DomaineName = reader.GetString(reader.GetOrdinal("DomaineName")),
                            };

                            return Ok(profil);
                        }
                        else
                        {
                            return NotFound(new { message = "Aucun profil pour ce slug." });
                        }
                    }
                }
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProfil(int id, [FromBody] Profil updatedProfil)
        {
            if (updatedProfil == null)
            {
                return BadRequest(new { message = "Profil data is missing." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("UPDATE Profil SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, UserType = @UserType, Siren = @Siren, Facebook = @Facebook, Instagram = @Instagram, Twitter = @Twitter, Youtube = @Youtube, tiktok = @tiktok, Email = @Email, DomaineName = @DomaineName, ProfilId = @ProfilId WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ProfilId", updatedProfil.ProfilId);
                    command.Parameters.AddWithValue("@FirstName", updatedProfil.FirstName);
                    command.Parameters.AddWithValue("@LastName", updatedProfil.LastName);
                    command.Parameters.AddWithValue("@PhoneNumber", updatedProfil.PhoneNumber);
                    command.Parameters.AddWithValue("@UserType", updatedProfil.UserType);
                    command.Parameters.AddWithValue("@Siren", updatedProfil.Siren);
                    command.Parameters.AddWithValue("@Facebook", updatedProfil.Facebook);
                    command.Parameters.AddWithValue("@Instagram", updatedProfil.Instagram);
                    command.Parameters.AddWithValue("@Twitter", updatedProfil.Twitter);
                    command.Parameters.AddWithValue("@Youtube", updatedProfil.Youtube);
                    command.Parameters.AddWithValue("@Tiktok", updatedProfil.Tiktok);
                    command.Parameters.AddWithValue("@Email", updatedProfil.Email);
                    command.Parameters.AddWithValue("@DomaineName", updatedProfil.DomaineName);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Profil mis à jour avec succès : Id = {id}");
                        return Ok(new { message = "Profil mis à jour avec succès." });
                    }
                    else
                    {
                        Console.WriteLine("La mise à jour a échoué.");
                        return BadRequest(new { message = "La mise à jour a échoué." });
                    }
                }
            }
        }

    }
}
