using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using British_Kingdom_back.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BlogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private object ToDto(SqlDataReader reader)
        {
            return new
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProfilId = reader.GetInt32(reader.GetOrdinal("ProfilId")),
                Slug = reader.GetString(reader.GetOrdinal("Slug")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Excerpt = reader.GetString(reader.GetOrdinal("Excerpt")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                CoverImage = reader.GetString(reader.GetOrdinal("CoverImage")),
                Date = reader.GetDateTime(reader.GetOrdinal("PostDate")).ToString("yyyy-MM-dd"),
                ReadingTime = reader.GetInt32(reader.GetOrdinal("ReadingTime")),
                Content = JsonConvert.DeserializeObject<List<BlogPostBlockInput>>(reader.GetString(reader.GetOrdinal("ContentJson")))
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var posts = new List<object>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM BlogPost WHERE ProfilId = @ProfilId ORDER BY PostDate DESC", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            posts.Add(ToDto(reader));
                        }
                    }
                }
            }

            return Ok(posts);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetPostBySlug(string slug, [FromQuery] int profilId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM BlogPost WHERE Slug = @Slug AND ProfilId = @ProfilId", connection))
                {
                    command.Parameters.AddWithValue("@Slug", slug);
                    command.Parameters.AddWithValue("@ProfilId", profilId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return Ok(ToDto(reader));
                        }
                        return NotFound(new { message = "Article non trouvé" });
                    }
                }
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] BlogPostInput input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Blog post data is missing." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "INSERT INTO BlogPost (ProfilId, Slug, Title, Excerpt, Category, CoverImage, PostDate, ReadingTime, ContentJson) " +
                    "OUTPUT INSERTED.Id " +
                    "VALUES (@ProfilId, @Slug, @Title, @Excerpt, @Category, @CoverImage, @PostDate, @ReadingTime, @ContentJson)", connection))
                {
                    command.Parameters.AddWithValue("@ProfilId", input.ProfilId);
                    command.Parameters.AddWithValue("@Slug", input.Slug);
                    command.Parameters.AddWithValue("@Title", input.Title);
                    command.Parameters.AddWithValue("@Excerpt", input.Excerpt);
                    command.Parameters.AddWithValue("@Category", input.Category);
                    command.Parameters.AddWithValue("@CoverImage", input.CoverImage);
                    command.Parameters.AddWithValue("@PostDate", input.Date);
                    command.Parameters.AddWithValue("@ReadingTime", input.ReadingTime);
                    command.Parameters.AddWithValue("@ContentJson", JsonConvert.SerializeObject(input.Content));

                    var newId = (int)await command.ExecuteScalarAsync();
                    return CreatedAtAction(nameof(GetPostBySlug), new { slug = input.Slug, profilId = input.ProfilId }, new { Id = newId });
                }
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] BlogPostInput input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Blog post data is missing." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "UPDATE BlogPost SET Slug = @Slug, Title = @Title, Excerpt = @Excerpt, Category = @Category, " +
                    "CoverImage = @CoverImage, PostDate = @PostDate, ReadingTime = @ReadingTime, ContentJson = @ContentJson " +
                    "WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Slug", input.Slug);
                    command.Parameters.AddWithValue("@Title", input.Title);
                    command.Parameters.AddWithValue("@Excerpt", input.Excerpt);
                    command.Parameters.AddWithValue("@Category", input.Category);
                    command.Parameters.AddWithValue("@CoverImage", input.CoverImage);
                    command.Parameters.AddWithValue("@PostDate", input.Date);
                    command.Parameters.AddWithValue("@ReadingTime", input.ReadingTime);
                    command.Parameters.AddWithValue("@ContentJson", JsonConvert.SerializeObject(input.Content));

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Article mis à jour avec succès" });
                    }
                    return NotFound(new { message = "Article non trouvé" });
                }
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("DELETE FROM BlogPost WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Article supprimé avec succès" });
                    }
                    return NotFound(new { message = "Article non trouvé" });
                }
            }
        }
    }

    public class BlogPostInput
    {
        public int ProfilId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ReadingTime { get; set; }
        public List<BlogPostBlockInput> Content { get; set; } = new List<BlogPostBlockInput>();
    }

    public class BlogPostBlockInput
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
