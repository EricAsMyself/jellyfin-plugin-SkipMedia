using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SkipMedia.Configuration
{
    /// <summary>
    /// Configuration options for the SkipMedia plugin.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class with default values.
        /// </summary>
        public PluginConfiguration()
        {
            AString = string.Empty;
            Options = string.Empty;
            AnInteger = 0;
            TrueFalseSetting = false;
            SessionCheckInterval = 100; // Default value: 100 ms
        }

        /// <summary>
        /// Gets or sets a sample string property.
        /// </summary>
        public string AString { get; set; }

        /// <summary>
        /// Gets or sets options string.
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        /// Gets or sets an integer value.
        /// </summary>
        public int AnInteger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the true/false setting is enabled.
        /// </summary>
        public bool TrueFalseSetting { get; set; }

        /// <summary>
        /// Gets or sets how often to check sessions, in milliseconds.
        /// </summary>
        public int SessionCheckInterval { get; set; }
    }
}
