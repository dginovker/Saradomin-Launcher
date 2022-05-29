using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Saradomin.Utilities;

namespace Saradomin.Services
{
    public class ClientUpdateService : IClientUpdateService
    {
        private const string RemoteClientHashURL = "https://cdn.2009scape.org/2009scape.md5sum";
        private const string RemoteClientExecutableURL = "https://cdn.2009scape.org/2009scape.jar";

        private float CurrentDownloadProgress { get; set; }

        public event EventHandler<float> DownloadProgressChanged;

        public async Task<string> FetchRemoteClientHashAsync(CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(RemoteClientHashURL, cancellationToken);
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }

        public async Task FetchRemoteClientExecutableAsync(CancellationToken cancellationToken, string targetPath = null)
        {
            CurrentDownloadProgress = 0;

            if (targetPath == null)
                targetPath = CrossPlatform.Locate2009scapeExecutable();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(RemoteClientExecutableURL, cancellationToken);
                var contentLength = response.Content.Headers.ContentLength ?? 12 * 1024 * 1024 * 1024f;

                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var outFileStream = File.OpenWrite(targetPath);

                var data = new byte[1024];
                var totalRead = 0;

                while (responseStream.Position < contentLength)
                {
                    var dataRead = await responseStream.ReadAsync(data, 0, data.Length, cancellationToken);

                    if (dataRead <= 0)
                        throw new IOException("Unexpected 0-byte read in network stream.");

                    await outFileStream.WriteAsync(data[0..dataRead], cancellationToken);
                    totalRead += dataRead;
                    
                    CurrentDownloadProgress = totalRead / contentLength;
                    DownloadProgressChanged?.Invoke(this, CurrentDownloadProgress);
                }
            }
        }

        public async Task<string> ComputeLocalClientHashAsync(string filePath = null)
        {
            if (filePath == null)
                filePath = CrossPlatform.Locate2009scapeExecutable();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Unable to calculate local client hash. File '{filePath}' missing.");

            using var stream = File.OpenRead(filePath);

            var md5 = MD5.Create();
            var hash = await md5.ComputeHashAsync(stream);

            var sb = new StringBuilder();

            foreach (var b in hash)
                sb.Append($"{b:x2}");

            return sb.ToString();
        }
    }
}