using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using British_Kingdom_back.Models;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ContactController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult CreateContact(Contact contact)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            int newId;

            // L'heure envoyée par le client n'est pas fiable (et le DateTimeConverter global
            // tronque de toute façon les DateTime à la date seule) : on utilise l'heure serveur.
            var now = DateTime.UtcNow;
            var nowParis = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("INSERT INTO Contact (ProfilId, Num, Subject, Name, Message, Email, Vue, DateofCrea, Hour) OUTPUT INSERTED.ID VALUES (@ProfilId, @Num, @Subject, @Name, @Message, @Email, @Vue, @DateofCrea, @Hour)", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", contact.ProfilId);
                    command.Parameters.AddWithValue("@Num", contact.Num);
                    command.Parameters.AddWithValue("@Subject", contact.Subject);
                    command.Parameters.AddWithValue("@Name", contact.Name);
                    command.Parameters.AddWithValue("@Message", contact.Message);
                    command.Parameters.AddWithValue("@Email", contact.Email);
                    command.Parameters.AddWithValue("@Hour", nowParis.ToString("HH:mm"));
                    command.Parameters.AddWithValue("@Vue", contact.Vue);
                    command.Parameters.AddWithValue("@DateofCrea", now);



                    newId = (int)command.ExecuteScalar();
                }
            }

            contact.Id = newId;
            return CreatedAtAction(nameof(GetContactById), new { id = newId, profilId = contact.ProfilId }, contact);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(int id, int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = "SELECT * FROM Contact WHERE Id = @Id AND ProfilId = @ProfilId";

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
                            var dateOfCreaUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("DateofCrea")), DateTimeKind.Utc);
                            var contact = new
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Num = reader.GetString(reader.GetOrdinal("Num")),
                                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Message = reader.GetString(reader.GetOrdinal("Message")),
                                Hour = reader.GetString(reader.GetOrdinal("Hour")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Vue = reader.GetBoolean(reader.GetOrdinal("Vue")),
                                // Sérialisé en string à la main : le DateTimeConverter global tronque les DateTime à "yyyy-MM-dd"
                                DateofCrea = dateOfCreaUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            };

                            return Ok(contact);
                        }
                        else
                        {
                            return NotFound(new { message = "Contact non trouvé" });
                        }
                    }
                }
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllContacts(int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var contacts = new List<object>();
            string query = "SELECT * FROM Contact WHERE ProfilId = @ProfilId ORDER BY DateofCrea DESC";

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
                            var dateOfCreaUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("DateofCrea")), DateTimeKind.Utc);
                            var contact = new
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                Num = reader.GetString(reader.GetOrdinal("Num")),
                                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Message = reader.GetString(reader.GetOrdinal("Message")),
                                Hour = reader.GetString(reader.GetOrdinal("Hour")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Vue = reader.GetBoolean(reader.GetOrdinal("Vue")),
                                // Sérialisé en string à la main : le DateTimeConverter global tronque les DateTime à "yyyy-MM-dd"
                                DateofCrea = dateOfCreaUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            };
                            contacts.Add(contact);
                        }
                    }
                }
            }

            return Ok(contacts);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM Contact WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Contact supprimé avec succès" });
                    }
                    else
                    {
                        return NotFound(new { message = "Contact non trouvé" });
                    }
                }
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateContact(int id, Contact contact)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Assurez-vous que le contact avec l'ID spécifié existe avant de le mettre à jour
                if (ContactExists(connection, id))
                {
                    // Mettez à jour le contact
                    // DateofCrea/Hour ne sont volontairement pas mis à jour : ce sont des dates de création
                    // immuables, et contact.DateofCrea (venant du corps de la requête) transite par le
                    // DateTimeConverter global qui tronque l'heure à minuit.
                    using (var command = new SqlCommand("UPDATE Contact SET ProfilId = @ProfilId, Num = @Num, Subject = @Subject, Name = @Name, Message = @Message, Email = @Email, Vue = @Vue WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ProfilId", contact.ProfilId);
                        command.Parameters.AddWithValue("@Num", contact.Num);
                        command.Parameters.AddWithValue("@Subject", contact.Subject);
                        command.Parameters.AddWithValue("@Name", contact.Name);
                        command.Parameters.AddWithValue("@Message", contact.Message);
                        command.Parameters.AddWithValue("@Email", contact.Email);
                        command.Parameters.AddWithValue("@Vue", contact.Vue);




                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "Contact mis à jour avec succès" });
                        }
                        else
                        {
                            return NotFound(new { message = "Contact non trouvé" });
                        }
                    }
                }
                else
                {
                    return NotFound(new { message = "Contact non trouvé" });
                }
            }
        }

        // Méthode utilitaire pour vérifier l'existence du contact
        private bool ContactExists(SqlConnection connection, int id)
        {
            using (var command = new SqlCommand("SELECT COUNT(*) FROM Contact WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
    }
}

