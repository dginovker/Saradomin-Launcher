using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
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
            
            string downloadPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11" + Path.GetExtension(downloadUrl)
            );
            string extractedPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11"
            );

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                var contentLength = response.Content.Headers.ContentLength ?? 40 * 1024 * 1024L;
                var totalRead = 0L;
                var buffer = new byte[8192];

                // Create a FileStream to write the downloaded bytes to
                await using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        int bytesRead;
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
            
            JavaDownloadProgressChanged?.Invoke(this, 1f);
            
            if (Directory.Exists(extractedPath)) Directory.Delete(extractedPath, true);
            
            if (Path.GetExtension(downloadUrl) == ".zip")
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "jre11_temp");
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                await Task.Run(() => ZipFile.ExtractToDirectory(downloadPath, tempDir));
                Directory.Move(Directory.GetDirectories(tempDir)[0], extractedPath);
                Directory.Delete(tempDir, true);
            }
            else if (Path.GetExtension(downloadUrl) == ".gz" || Path.GetExtension(downloadUrl) == ".tar.gz")
            {
                Directory.CreateDirectory(extractedPath);
                await Task.Run(() => CrossPlatform.RunCommandAndGetOutput($"tar xf {downloadPath} -C {extractedPath} --strip-components 1"));
            }
            
            File.Delete(downloadPath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                settingsService.Launcher.JavaExecutableLocation = Path.Combine(
                    extractedPath,
                    "Contents",
                    "Home",
                    "bin",
                    "java"
                );
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                settingsService.Launcher.JavaExecutableLocation = Path.Combine(
                    extractedPath,
                    "bin",
                    "java.exe"
                );
            } else {
                settingsService.Launcher.JavaExecutableLocation = Path.Combine(
                    extractedPath,
                    "bin",
                    "java"
                );
            }
            settingsService.SaveAll();
        }
    }
}
