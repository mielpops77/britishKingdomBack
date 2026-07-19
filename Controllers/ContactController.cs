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
                    command.Parameters.AddWithValue("@Hour", contact.Hour);
                    command.Parameters.AddWithValue("@Vue", contact.Vue);
                    command.Parameters.AddWithValue("@DateofCrea", contact.DateofCrea);



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
                            var contact = new Contact
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
                                DateofCrea = reader.GetDateTime(reader.GetOrdinal("DateofCrea"))

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
            var contacts = new List<Contact>();
            string query = "SELECT * FROM Contact WHERE ProfilId = @ProfilId ORDER BY CONVERT(datetime, DateofCrea + ' ' + Hour, 120) DESC";

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
                            var contact = new Contact
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
                                DateofCrea = reader.GetDateTime(reader.GetOrdinal("DateofCrea"))
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
                    using (var command = new SqlCommand("UPDATE Contact SET ProfilId = @ProfilId, Num = @Num, Subject = @Subject, Name = @Name, Message = @Message, Email = @Email, Vue = @Vue, DateofCrea = @DateofCrea, Hour = @Hour WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ProfilId", contact.ProfilId);
                        command.Parameters.AddWithValue("@Num", contact.Num);
                        command.Parameters.AddWithValue("@Subject", contact.Subject);
                        command.Parameters.AddWithValue("@Name", contact.Name);
                        command.Parameters.AddWithValue("@Message", contact.Message);
                        command.Parameters.AddWithValue("@Hour", contact.Hour);
                        command.Parameters.AddWithValue("@Email", contact.Email);
                        command.Parameters.AddWithValue("@Vue", contact.Vue);
                        command.Parameters.AddWithValue("@DateofCrea", contact.DateofCrea);




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

