using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class ClientUpdateService : IClientUpdateService
    {
        private const string LegacyRemoteClientHashURL = "https://cdn.2009scape.org/2009scape.md5sum";
        private const string LegacyRemoteClientExecutableURL = "https://cdn.2009scape.org/2009scape.jar";
        
        private const string ExperimentalRemoteClientExecutableURL = "https://github.com/Pazaz/RT4-Client/releases/latest/download/rt4-client.jar";
        private const string ExperimentalRemoteClientHashURL = "https://github.com/Pazaz/RT4-Client/releases/latest/download/rt4-client.jar.sha256";
        
        private readonly ISettingsService _settingsService;
        
        private float CurrentDownloadProgress { get; set; }

        public string PreferredDownloadURL
        {
            get
            {
                return _settingsService.Launcher.ClientProfile switch
                {
                    LauncherSettings.ClientReleaseProfile.Legacy => LegacyRemoteClientExecutableURL,
                    LauncherSettings.ClientReleaseProfile.Experimental => ExperimentalRemoteClientExecutableURL,
                    _ => throw new NotSupportedException("Unsupported client release profile.")
                };
            }
        }
        
        public string PreferredHashUrl
        {
            get
            {
                return _settingsService.Launcher.ClientProfile switch
                {
                    LauncherSettings.ClientReleaseProfile.Legacy => LegacyRemoteClientHashURL,
                    LauncherSettings.ClientReleaseProfile.Experimental => ExperimentalRemoteClientHashURL,
                    _ => throw new NotSupportedException("Unsupported client release profile.")
                };
            }
        }

        public string PreferredTargetFilePath 
            => _settingsService.Launcher.ClientProfile == LauncherSettings.ClientReleaseProfile.Experimental
                ? CrossPlatform.Locate2009scapeExperimentalExecutable()
                : CrossPlatform.Locate2009scapeLegacyExecutable();

        public event EventHandler<float> DownloadProgressChanged;

        public ClientUpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<string> FetchRemoteClientHashAsync(CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            {
                var response = await httpClient.GetAsync(PreferredHashUrl, cancellationToken);
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }

        public async Task FetchRemoteClientExecutableAsync(CancellationToken cancellationToken, string targetPath = null)
        {
            CurrentDownloadProgress = 0;

            targetPath ??= PreferredTargetFilePath;

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(PreferredDownloadURL, cancellationToken);
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
            var isExperimental = _settingsService.Launcher.ClientProfile == LauncherSettings.ClientReleaseProfile.Experimental;

            filePath ??= PreferredTargetFilePath;
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Unable to calculate local client hash. File '{filePath}' missing.");

            await using var stream = File.OpenRead(filePath);
            {
                if (isExperimental)
                {
                    var sha256 = SHA256.Create();
                    stream.Position = 0;
                    var hash = await sha256.ComputeHashAsync(stream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
                else
                {
                    var md5 = MD5.Create();
                    var hash = await md5.ComputeHashAsync(stream);

                    var sb = new StringBuilder();

                    foreach (var b in hash)
                        sb.Append($"{b:x2}");

                    return sb.ToString();
                }
            }
        }
    }
}