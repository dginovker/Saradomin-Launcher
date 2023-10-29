using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class SingleplayerUpdateService : ISingleplayerUpdateService
    {
        public event EventHandler<Tuple<float, bool>> SingleplayerDownloadProgressChanged;

        public async Task DownloadSingleplayer()
        {
            SingleplayerDownloadProgressChanged?.Invoke(this, new Tuple<float, bool>(0f, false));
            string downloadUrl =
                "https://gitlab.com/2009scape/singleplayer/windows/-/archive/master/windows-master.zip";

            string downloadPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "singleplayer" + Path.GetExtension(downloadUrl)
            );

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                var contentLength = response.Content.Headers.ContentLength ?? 40 * 1024 * 1024L;
                var totalRead = 0L;
                var buffer = new byte[8192];

                // Create a FileStream to write the downloaded bytes to
                await using (var fileStream =
                             new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
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
                            SingleplayerDownloadProgressChanged?.Invoke(this,
                                 new Tuple<float, bool>(progress, false));
                        } while (bytesRead > 0);
                    }
                }
            }

            SingleplayerDownloadProgressChanged?.Invoke(this, new Tuple<float, bool>(1f, false));

            if (Directory.Exists(CrossPlatform.LocateSingleplayerHome())) Directory.Delete(CrossPlatform.LocateSingleplayerHome(), true);

            // Don't use /tmp because Directory.Move doesn't work cross-partition
            string tempDir = Path.Combine(CrossPlatform.LocateDefault2009scapeHome(), "singleplayer_temp"); 
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            await Task.Run(() => ZipFile.ExtractToDirectory(downloadPath, tempDir));
            foreach (string directory in Directory.GetDirectories(tempDir))
            {
                Console.WriteLine("Found directory " + directory + " in " + tempDir);
            }
            Directory.Move(Directory.GetDirectories(tempDir)[0], CrossPlatform.LocateSingleplayerHome());
            Directory.Delete(tempDir, true);

            File.Delete(downloadPath);
            SingleplayerDownloadProgressChanged?.Invoke(this, new Tuple<float, bool>(1f, true));
        }
    }
}
