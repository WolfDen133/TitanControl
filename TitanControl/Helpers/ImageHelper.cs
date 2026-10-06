using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TitanControl.Helpers
{
    public class ImageHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<Bitmap?> LoadImageAsync(string path)
        {
            return await Task.Run(async () =>
            {
                var bytes = await _httpClient.GetByteArrayAsync(path);
                using var stream = new MemoryStream(bytes);

                return new Bitmap(stream);
            });
        }
    }
}
