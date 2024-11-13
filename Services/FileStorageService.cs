using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Services
{
    public class FileStorageService
    {
       public static readonly string ROOT = Path.Combine(Directory.GetCurrentDirectory(),"project-images");
        public FileStorageService()
        {
           Directory.CreateDirectory(ROOT);
        }
        
        public async Task<string> SaveAsync(IFormFile formFile)
        {
            string filePath = $"{Guid.NewGuid().ToString()}{Path.GetExtension(formFile.FileName)}";
            Stream stream = File.Create(Path.Combine(ROOT, filePath));
            await formFile.CopyToAsync(stream);
            return filePath;
        }        

    }
}
