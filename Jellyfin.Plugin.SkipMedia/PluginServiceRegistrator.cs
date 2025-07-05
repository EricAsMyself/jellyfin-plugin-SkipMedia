using Jellyfin.Plugin.SkipMedia;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.SkipMedia
{
    /// <summary>
    /// Registers services for the Template plugin.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Register the hosted service that skips the first 20 minutes of playback
            serviceCollection.AddHostedService<SkipEdl>();
        }
    }
}
