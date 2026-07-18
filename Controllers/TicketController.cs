using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using British_Kingdom_back.Models;



namespace VotreProjet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TicketController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int? uniqueProfilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var tickets = new List<Ticket>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query;
                SqlCommand command;

                if (uniqueProfilId.HasValue)
                {
                    query = "SELECT * FROM Ticket WHERE UniqueProfilId = @UniqueProfilId ORDER BY CreationDate DESC";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UniqueProfilId", uniqueProfilId.Value);
                }
                else
                {
                    query = "SELECT * FROM Ticket ORDER BY CreationDate DESC";
                    command = new SqlCommand(query, connection);
                }

                using (command)
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var ticket = new Ticket
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                UniqueProfilId = reader.GetInt32(reader.GetOrdinal("UniqueProfilId")),
                                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                CreationDate = reader.GetDateTimeOffset(reader.GetOrdinal("CreationDate")),
                                Message = reader.GetString(reader.GetOrdinal("Message")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Image = reader.GetString(reader.GetOrdinal("Image")),

                            };

                            // Load conversations and profile for each ticket
                            ticket.Conversations = await LoadConversationsForTicketAsync(ticket.Id, connection);
                            ticket.Profil = await LoadProfilForTicketAsync(ticket.Id, connection);

                            tickets.Add(ticket);
                        }
                    }
                }
            }

            return Ok(tickets);
        }

        // The rest of your controller methods...
        private async Task<List<Conversation>> LoadConversationsForTicketAsync(int ticketId, SqlConnection connection)
        {
            using (var command = new SqlCommand("SELECT * FROM Conversation WHERE IdTicket = @TicketId", connection))
            {
                command.Parameters.AddWithValue("@TicketId", ticketId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var conversations = new List<Conversation>();

                    while (await reader.ReadAsync())
                    {
                        var conversation = new Conversation
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            IdTicket = reader.GetInt32(reader.GetOrdinal("IdTicket")),
                            UniqueProfilId = reader.GetInt32(reader.GetOrdinal("UniqueProfilId")),
                            DateCrea = reader.GetDateTime(reader.GetOrdinal("DateCrea")),
                            Message = reader.GetString(reader.GetOrdinal("Message")),
                            Admin = reader.GetBoolean(reader.GetOrdinal("Admin")),
                            Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                            VueAdmin = reader.GetBoolean(reader.GetOrdinal("VueAdmin")),
                            VueUser = reader.GetBoolean(reader.GetOrdinal("VueUser")),
                        };

                        conversations.Add(conversation);
                    }

                    return conversations;
                }
            }
        }

        private async Task<Profil> LoadProfilForTicketAsync(int ticketId, SqlConnection connection)
        {
            using (var command = new SqlCommand("SELECT p.* FROM Profil p INNER JOIN Ticket t ON p.Id = t.UniqueProfilId WHERE t.Id = @TicketId", connection))
            {
                command.Parameters.AddWithValue("@TicketId", ticketId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var profil = new Profil
                        {
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
                            // Ajoutez d'autres propriétés si nécessaire
                        };

                        return profil;
                    }

                    return null;
                }
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM Ticket WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var ticket = new Ticket
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                UniqueProfilId = reader.GetInt32(reader.GetOrdinal("UniqueProfilId")),
                                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                                Message = reader.GetString(reader.GetOrdinal("Message")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),


                            };

                            // Chargez les conversations en fonction de l'ID du ticket.
                            ticket.Conversations = await LoadConversationsForTicketAsync(ticket.Id, connection);

                            // Chargez le profil en fonction de l'ID du ticket.
                            ticket.Profil = await LoadProfilForTicketAsync(ticket.Id, connection);

                            return Ok(ticket);
                        }
                        else
                        {
                            return NotFound(); // Le ticket avec l'ID spécifié n'a pas été trouvé.
                        }
                    }
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateTicket(Ticket ticket)
        {
            if (ticket == null)
            {
                return BadRequest("Ticket data is missing.");
            }

            if (ticket.UniqueProfilId <= 0)
            {
                return BadRequest("Invalid value for UniqueProfilId.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            int newTicketId;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insérer le ticket
                        using (var command = new SqlCommand("INSERT INTO Ticket (UniqueProfilId, Subject, CreationDate, Message, Status, Image) OUTPUT INSERTED.Id VALUES (@UniqueProfilId, @Subject, @CreationDate, @Message, @Status, @Image);", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UniqueProfilId", ticket.UniqueProfilId);
                            command.Parameters.AddWithValue("@Subject", ticket.Subject);
                            command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                            command.Parameters.AddWithValue("@Message", ticket.Message);
                            command.Parameters.AddWithValue("@Image", ticket.Image);
                            command.Parameters.AddWithValue("@Status", "En attente");




                            newTicketId = Convert.ToInt32(await command.ExecuteScalarAsync());

                            if (newTicketId <= 0)
                            {
                                transaction.Rollback();
                                return StatusCode(500, "La création du ticket a échoué.");
                            }
                        }

                        // 2. Insérer les conversations associées au ticket
                        if (ticket.Conversations != null && ticket.Conversations.Any())
                        {
                            foreach (var conversation in ticket.Conversations)
                            {
                                using (var conversationCommand = new SqlCommand("INSERT INTO Conversation (IdTicket, UniqueProfilId, DateCrea, Message, Admin, Image, VueUser, VueAdmin) VALUES (@IdTicket, @UniqueProfilId, @DateCrea, @Message, @Admin, @Image, @VueUser, @VueAdmin);", connection, transaction))
                                {
                                    conversationCommand.Parameters.AddWithValue("@IdTicket", newTicketId);
                                    conversationCommand.Parameters.AddWithValue("@UniqueProfilId", conversation.UniqueProfilId);
                                    conversationCommand.Parameters.AddWithValue("@DateCrea", DateTime.Now);
                                    conversationCommand.Parameters.AddWithValue("@Message", conversation.Message);
                                    conversationCommand.Parameters.AddWithValue("@Admin", conversation.Admin);
                                    conversationCommand.Parameters.AddWithValue("@Image", conversation.Image);
                                    conversationCommand.Parameters.AddWithValue("@VueUser", conversation.VueUser);
                                    conversationCommand.Parameters.AddWithValue("@VueAdmin", conversation.VueAdmin);





                                    await conversationCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        transaction.Commit();
                        ticket.Id = newTicketId;
                        return CreatedAtAction(nameof(GetTicket), new { id = newTicketId }, ticket);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite." + ex.Message);
                    }
                }
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, Ticket ticket)
        {
            if (ticket == null)
            {
                return BadRequest("Ticket data is missing.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Mettre à jour le ticket
                        using (var command = new SqlCommand("UPDATE Ticket SET Subject = @Subject, Message = @Message, Status = @Status, Image = @Image WHERE Id = @Id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.Parameters.AddWithValue("@Subject", ticket.Subject);
                            command.Parameters.AddWithValue("@Message", ticket.Message);
                            command.Parameters.AddWithValue("@Status", ticket.Status);
                            command.Parameters.AddWithValue("@Image", ticket.Image);

                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            if (rowsAffected <= 0)
                            {
                                transaction.Rollback();
                                return BadRequest(new { message = "La mise à jour du ticket a échoué." });
                            }
                        }

                        // 2. Mettre à jour les conversations associées au ticket
                        foreach (var conversation in ticket.Conversations)
                        {
                            using (var conversationCommand = new SqlCommand("UPDATE Conversation SET Message = @Message, VueUser = @VueUser, VueAdmin = @VueAdmin WHERE Id = @Id", connection, transaction))
                            {
                                conversationCommand.Parameters.AddWithValue("@Id", conversation.Id);
                                conversationCommand.Parameters.AddWithValue("@Message", conversation.Message);
                                conversationCommand.Parameters.AddWithValue("@VueUser", conversation.VueUser);
                                conversationCommand.Parameters.AddWithValue("@VueAdmin", conversation.VueAdmin);

                                int conversationRowsAffected = await conversationCommand.ExecuteNonQueryAsync();

                                if (conversationRowsAffected <= 0)
                                {
                                    transaction.Rollback();
                                    return BadRequest(new { message = "La mise à jour de la conversation a échoué." });
                                }
                            }
                        }

                        // 3. Mettre à jour le profil associé au ticket
                        if (ticket.Profil != null)
                        {
                            var profil = ticket.Profil;
                            using (var profilCommand = new SqlCommand("UPDATE Profil SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, UserType = @UserType, Siren = @Siren, Facebook = @Facebook, Instagram = @Instagram, Twitter = @Twitter, Youtube = @Youtube, Tiktok = @Tiktok, Email = @Email WHERE Id = @ProfilId", connection, transaction))
                            {
                                profilCommand.Parameters.AddWithValue("@ProfilId", profil.Id);
                                profilCommand.Parameters.AddWithValue("@FirstName", profil.FirstName);
                                profilCommand.Parameters.AddWithValue("@LastName", profil.LastName);
                                profilCommand.Parameters.AddWithValue("@PhoneNumber", profil.PhoneNumber);
                                profilCommand.Parameters.AddWithValue("@UserType", profil.UserType);
                                profilCommand.Parameters.AddWithValue("@Siren", profil.Siren);
                                profilCommand.Parameters.AddWithValue("@Facebook", profil.Facebook);
                                profilCommand.Parameters.AddWithValue("@Instagram", profil.Instagram);
                                profilCommand.Parameters.AddWithValue("@Twitter", profil.Twitter);
                                profilCommand.Parameters.AddWithValue("@Youtube", profil.Youtube);
                                profilCommand.Parameters.AddWithValue("@Tiktok", profil.Tiktok);
                                profilCommand.Parameters.AddWithValue("@Email", profil.Email);

                                int profilRowsAffected = await profilCommand.ExecuteNonQueryAsync();

                                if (profilRowsAffected <= 0)
                                {
                                    transaction.Rollback();
                                    return BadRequest(new { message = "La mise à jour du profil a échoué." });
                                }
                            }
                        }

                        transaction.Commit();
                        return Ok(new { message = "Ticket, conversation et profil mis à jour avec succès." });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Une erreur interne s'est produite." + ex.Message);
                    }
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ticket ID.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Supprimer les conversations associées au ticket
                        using (var deleteConversationsCommand = new SqlCommand("DELETE FROM Conversation WHERE IdTicket = @Id", connection, transaction))
                        {
                            deleteConversationsCommand.Parameters.AddWithValue("@Id", id);

                            await deleteConversationsCommand.ExecuteNonQueryAsync();
                        }

                        // 2. Supprimer le ticket
                        using (var deleteTicketCommand = new SqlCommand("DELETE FROM Ticket WHERE Id = @Id", connection, transaction))
                        {
                            deleteTicketCommand.Parameters.AddWithValue("@Id", id);

                            int rowsAffected = await deleteTicketCommand.ExecuteNonQueryAsync();

                            if (rowsAffected <= 0)
                            {
                                transaction.Rollback();
                                return NotFound(new { message = "Le ticket avec l'ID spécifié n'a pas été trouvé." });
                            }
                        }

                        transaction.Commit();
                        return Ok(new { message = "Ticket et conversations supprimés avec succès." });
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
}
