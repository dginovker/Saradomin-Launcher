using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
using Saradomin.Infrastructure.Messaging;
using Saradomin.Infrastructure.Services;
using Saradomin.Model;
using Saradomin.View.Windows;

namespace Saradomin.ViewModel.Controls
{
    public class PluginManagerViewModel : ViewModelBase
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IPluginDownloadService _pluginDownloadService;

        public bool IsTransactionInProgress { get; private set; }
        
        public ObservableCollection<PluginInfo> LocalPlugins { get; private set; } = new();
        public ObservableCollection<PluginInfo> RemotePlugins { get; private set; } = new();

        public PluginManagerViewModel(
            IPluginManagementService pluginManagementService,
            IPluginDownloadService pluginDownloadService)
        {
            _pluginManagementService = pluginManagementService;
            _pluginDownloadService = pluginDownloadService;
            
            Message.Subscribe<MainViewLoadedMessage>(this, MainViewLoaded);
        }

        public async Task RefreshPluginCollections()
        {
            RemotePlugins.Clear();
            LocalPlugins.Clear();
            
            var localPlugins = (await _pluginManagementService.EnumerateInstalledPlugins())
                .OrderBy(x => x)
                .ToList();
            
            var remotePlugins = (await _pluginDownloadService.FetchAvailablePluginNames())
                .OrderBy(x => x)
                .ToList();

            RemotePlugins = new ObservableCollection<PluginInfo>(
                remotePlugins
                .Except(localPlugins)
                .Select(x => new PluginInfo(x))
            );

            LocalPlugins = new ObservableCollection<PluginInfo>(
                localPlugins
                .Select(x => new PluginInfo(x))
            );
        }

        public async Task InstallRemotePlugin(PluginInfo pluginInfo)
        {
            if (IsTransactionInProgress)
                return;

            try
            {
                IsTransactionInProgress = true;

                await _pluginDownloadService.DownloadPluginFiles(
                    pluginInfo.Name,
                    _pluginManagementService.PluginRepositoryPath
                );

                RemotePlugins.Remove(pluginInfo);
                LocalPlugins.Add(pluginInfo);

                LocalPlugins = new(LocalPlugins.OrderBy(x => x.Name));
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Plugin installation failed: {e.Message}"
                );
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        public async Task UninstallLocalPlugin(PluginInfo pluginInfo)
        {
            if (IsTransactionInProgress)
                return;

            try
            {
                IsTransactionInProgress = true;
                
                await _pluginManagementService.UninstallPlugin(pluginInfo.Name);
                
                LocalPlugins.Remove(pluginInfo);
                RemotePlugins.Add(pluginInfo);

                RemotePlugins = new(RemotePlugins.OrderBy(x => x.Name));
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Plugin removal failed: {e.Message}" 
                );
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        private async void MainViewLoaded(MainViewLoadedMessage _)
        {
            await RefreshPluginCollections();
        }
    }
}