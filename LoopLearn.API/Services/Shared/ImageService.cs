namespace LoopLearn.API.Services.Shared
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _env;
        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task DeleteOldImageFileAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                // Only delete if it's a local file (not an external URL)
                if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                    return;

                var uri = new Uri(imageUrl);
                // Only delete if host matches your application (optional security)
                // if (uri.Host != Request.Host.Host) return;

                // Get the relative path from the URL (e.g., "/uploads/avatars/xxx.jpg")
                var relativePath = uri.LocalPath.TrimStart('/');
                var physicalPath = Path.Combine(_env.WebRootPath, relativePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    await Task.Run(() => System.IO.File.Delete(physicalPath));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
