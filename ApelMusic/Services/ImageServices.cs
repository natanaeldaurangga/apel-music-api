using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Services
{
    public class ImageServices
    {
        private readonly IWebHostEnvironment _env;

        private readonly string _imagePath = "Files\\Images\\";

        private readonly IConfiguration _configuration;

        private readonly ILogger<ImageServices> _logger;

        public ImageServices(IWebHostEnvironment env, IConfiguration configuration, ILogger<ImageServices> logger)
        {
            _env = env;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "")
        {
            if (file == null || file.Length == 0)
            {
                return _configuration.GetSection("Image:Default").Value;
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath;
            if (!string.IsNullOrEmpty(folder) || !string.IsNullOrWhiteSpace(folder))
            {
                filePath = Path.Combine(_env.ContentRootPath, _imagePath + folder + "/" + fileName);
            }
            else
            {
                filePath = Path.Combine(_env.ContentRootPath, _imagePath + fileName);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return folder + "%5C%5C" + fileName; // NOTE: '%5C%5C' -> '\\', biar bisa dibaca Http urlnya
        }

        public bool DeleteImage(string fileName)
        {
            var filePath = Path.Combine(_env.ContentRootPath, _imagePath + fileName);
            if (!File.Exists(filePath))
            {
                // _logger.LogInformation("Gambar tidak ketemu bro {}", filePath);
                throw new FileNotFoundException();
            }
            // _logger.LogInformation("Gambar ketemu bro {}", filePath);

            File.Delete(filePath);
            return true;
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            var filePath = Path.Combine(_env.ContentRootPath, _imagePath + fileName);
            _logger.LogInformation("filePath: ", filePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            return await File.ReadAllBytesAsync(filePath);
        }

    }
}