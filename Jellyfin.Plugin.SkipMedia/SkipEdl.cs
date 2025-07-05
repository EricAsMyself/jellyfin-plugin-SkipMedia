using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SkipMedia
{
    /// <summary>
    /// Plugin to automatically skip defined video segments using EDL (Edit Decision List) files.
    /// </summary>
    public sealed class SkipEdl : IHostedService, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<SkipEdl> _logger;
        private readonly System.Timers.Timer _timer;
        private readonly HashSet<string> _skippedSessions = new();
        private bool _disposed;

        private static readonly char[] EdlSeparators = { ' ', '\t' };

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipEdl"/> class.
        /// </summary>
        /// <param name="sessionManager">The session manager used to access playback sessions.</param>
        /// <param name="logger">Logger instance for writing plugin logs.</param>
        public SkipEdl(
            ISessionManager sessionManager,
            ILogger<SkipEdl> logger)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _timer = new System.Timers.Timer(100)
            {
                AutoReset = true
            };

            _timer.Elapsed += OnTimerElapsed;
            _sessionManager.SessionEnded += OnSessionEnded;
        }

        /// <summary>
        /// Starts the plugin service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the task.</param>
        /// <returns>A completed task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SkipEdl started.");
            _timer.Start();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the plugin service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the task.</param>
        /// <returns>A completed task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SkipEdl stopping.");
            _timer.Stop();
            return Task.CompletedTask;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (var session in _sessionManager.Sessions)
            {
                var item = session.NowPlayingItem;
                if (item?.Path is not { Length: > 0 })
                {
                    continue;
                }

                var edlPath = GetEdlPath(item.Path);
                if (!File.Exists(edlPath))
                {
                    continue;
                }

                var skipRanges = ParseEdlFile(edlPath);
                if (skipRanges.Count > 0)
                {
                    ApplySkipLogic(session, skipRanges);
                }
            }
        }

        private void ApplySkipLogic(SessionInfo session, List<(long Start, long End)> skipRanges)
        {
            var playState = session.PlayState;
            if (playState == null)
            {
                return;
            }

            var currentSeconds = playState.PositionTicks / TimeSpan.TicksPerSecond;

            foreach (var (start, end) in skipRanges)
            {
                if (currentSeconds >= start && currentSeconds < end)
                {
                    _logger.LogInformation(
                        "Skipping from {Start} to {End} for session {SessionId}",
                        start,
                        end,
                        session.Id);

                    _sessionManager.SendPlaystateCommand(
                        session.Id,
                        session.Id,
                        new PlaystateRequest
                        {
                            Command = PlaystateCommand.Seek,
                            ControllingUserId = session.UserId.ToString("N"),
                            SeekPositionTicks = end * TimeSpan.TicksPerSecond
                        },
                        CancellationToken.None);

                    break;
                }
            }
        }

        private static string GetEdlPath(string mediaPath)
        {
            var basePath = Path.ChangeExtension(mediaPath, null);
            return $"{basePath}.edl";
        }

        private static List<(long Start, long End)> ParseEdlFile(string edlPath)
        {
            var skipRanges = new List<(long, long)>();

            foreach (var line in File.ReadLines(edlPath))
            {
                var parts = line.Trim().Split(EdlSeparators, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], out var start) &&
                    double.TryParse(parts[1], out var end))
                {
                    skipRanges.Add(((long)start, (long)end));
                }
            }

            return skipRanges;
        }

        private void OnSessionEnded(object? sender, SessionEventArgs e)
        {
            if (e?.SessionInfo != null && _skippedSessions.Remove(e.SessionInfo.Id))
            {
                _logger.LogInformation("Session ended: cleared skip state for session {SessionId}", e.SessionInfo.Id);
            }
        }

        /// <summary>
        /// Releases unmanaged resources and unregisters event handlers.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the timer and unregisters session event listeners.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be released.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _sessionManager.SessionEnded -= OnSessionEnded;
            }

            _disposed = true;
        }
    }
}
