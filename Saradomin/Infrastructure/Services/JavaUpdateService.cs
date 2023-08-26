using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class JavaUpdateService : IJavaUpdateService
    {
        public event EventHandler<float> JavaDownloadProgressChanged;

        public async Task DownloadAndSetJava11(ISettingsService settingsService)
        {
            string downloadUrl = CrossPlatform.GetJava11DownloadUrl();
            
            Console.WriteLine($"Downloading Java 11 from {downloadUrl}.");
            string downloadPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11" + Path.GetExtension(downloadUrl)
            );
            string extractedPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11"
            );
            Console.WriteLine($"Download path: {downloadPath}, extracted path: {extractedPath}.");

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                var contentLength = response.Content.Headers.ContentLength ?? 1;
                var totalRead = 0L;
                var buffer = new byte[8192];

                // Create a FileStream to write the downloaded bytes to
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var bytesRead = 0;
                        do
                        {
                            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            totalRead += bytesRead;

                            // Write the bytes to the FileStream
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            var progress = (float)totalRead / contentLength;
                            JavaDownloadProgressChanged?.Invoke(this, progress);
                        } while (bytesRead > 0);
                    }
                }
            }

            if (Path.GetExtension(downloadUrl) == ".zip")
            {
                ZipFile.ExtractToDirectory(downloadPath, extractedPath);
            }
            else if (Path.GetExtension(downloadUrl) == ".gz" || Path.GetExtension(downloadUrl) == ".tar.gz")
            {
                if (!Directory.Exists(extractedPath)) Directory.CreateDirectory(extractedPath);
                string extractOutput = CrossPlatform.RunCommandAndGetOutput($"tar xf {downloadPath} -C {extractedPath} --strip-components 1");
                Console.WriteLine($"Extract output: {extractOutput}");
            }
            
            File.Delete(downloadPath);

            settingsService.Launcher.JavaExecutableLocation = Path.Combine(
                extractedPath,
                "bin/java"
            );
            settingsService.SaveAll();
        }
    }
}
