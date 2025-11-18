namespace CinemaBookingWeb.Areas.Admin.Data
{
    public class FileUploadService
    {
        private readonly string _folderPath;

        public FileUploadService(string subFolder = "poster")
        {
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", subFolder);
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        public async Task<string?> SavePosterAsync(IFormFile file, string? oldFile = null)
        {
            if (file == null || file.Length == 0)
                return oldFile;

            if (!string.IsNullOrEmpty(oldFile))
            {
                string oldPath = Path.Combine(_folderPath, oldFile);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            //string newFileName = Path.GetFileName(file.FileName);
            string newFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string newPath = Path.Combine(_folderPath, newFileName);

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return newFileName;
        }

        public bool DeletePoster(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.jpg")
                return false;

            try
            {
                string filePath = Path.Combine(_folderPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể xóa file poster: {ex.Message}");
            }

            return false;
        }

    }
}
