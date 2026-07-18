

using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        public ImagesController(IWebHostEnvironment environment, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _environment = environment;
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }
        /* 
                [HttpPost("upload")]
                public async Task<IActionResult> UploadImages(List<IFormFile> files, [FromQuery] string directory)
                {
                    try
                    {
                        if (files == null || files.Count == 0)
                        {
                            return BadRequest("No files were selected.");
                        }

                        List<string> uploadedFilePaths = new List<string>();

                        foreach (var file in files)
                        {
                            if (file.Length == 0)
                                continue;

                            string uploadsFolder = Path.Combine(_environment.WebRootPath, $"assets/{directory}");

                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);
                            string extension = Path.GetExtension(file.FileName);
                            string uniqueFileName = Guid.NewGuid().ToString()+ extension;

                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            uploadedFilePaths.Add(filePath);
                        }

                        return Ok(new { uploadedFilePaths });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                    }
                }


            } */
        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files, [FromQuery] string directory)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files were selected.");
                }

                List<string> uploadedFilePaths = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    // Generate a unique name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}.webp";

                    // Get a reference to the container
                    var containerName = _configuration["AzureStorage:ContainerName"];
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient($"{directory}/{uniqueFileName}");

                    using (var memoryStream = new MemoryStream())
                    {
                        // Load the image using ImageSharp
                        using (var image = await Image.LoadAsync(file.OpenReadStream()))
                        {
                            // Convert the image to WebP and save it to the memory stream
                            await image.SaveAsWebpAsync(memoryStream);
                        }

                        // Reset the position of the memory stream before uploading
                        memoryStream.Position = 0;

                        // Upload the converted image to Azure Blob Storage
                        await blobClient.UploadAsync(memoryStream, true);
                    }

                    // Get the URL of the uploaded blob
                    var blobUrl = blobClient.Uri.ToString();

                    uploadedFilePaths.Add(blobUrl);
                }

                return Ok(new { uploadedFilePaths });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
