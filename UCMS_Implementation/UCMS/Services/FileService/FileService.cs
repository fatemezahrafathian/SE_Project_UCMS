using System;
using UCMS.Services.FileService.Abstraction;

namespace UCMS.Services.FileService
{
    public class FileService : IFileService
    {

        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            environment = environment;
        }

        public void DeleteFile(string fileNameWithExtension)
        {

        }

        public async Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile));
            }

            var contentPath = _environment.ContentRootPath;
            var path = Path.Combine(contentPath, "Uploads");
            // path = "c://projects/ImageManipulation.Ap/uploads" ,not exactly, but something like that

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Check the allowed extenstions
            var ext = Path.GetExtension(imageFile.FileName);
            //if (!allowedFileExtensions.Contains(ext))
            //{
                //throw new ArgumentException($"Only {string.Join(",", allowedFileExtensions)} are allowed.");
            //}

            // generate a unique filename
            var fileName = $"{Guid.NewGuid().ToString()}{ext}";
            var fileNameWithPath = Path.Combine(path, fileName);
            using var stream = new FileStream(fileNameWithPath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            return fileNameWithPath;
        }
    }
}
