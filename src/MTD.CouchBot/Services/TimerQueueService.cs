using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class TimerQueueService
    {
        private readonly BotSettings _botSettings;

        private Timer _beamClientTimer;
        private Timer _mixerPingTimer;
        private Timer _hitboxTimer;
        private Timer _hitboxOwnerTimer;
        private Timer _mobcrushTimer;
        private Timer _mobcrushOwnerTimer;
        private Timer _twitchTimer;
        private Timer _youtubeTimer;
        private Timer _youtubePublishedTimer;
        private Timer _youtubePublishedOwnerTimer;
        private Timer _twitchFeedTimer;
        private Timer _twitchOwnerFeedTimer;
        private Timer _twitchTeamTimer;
        private Timer _twitchGameTimer;
        private Timer _picartoTimer;
        private Timer _picartoOwnerTimer;
        private Timer _vidMeTimer;
        private Timer _vidMeOwnerTimer;
        private Timer _guildCheckTimer;
        private Timer _twitchServerTimer;
        private Timer _cleanupTimer;
        private Timer _uptimeTimer;

        private readonly MixerConstellationService _mixerService;
        private readonly ILoggingManager _loggingManager;
        private readonly PlatformService _platformServices;
        private readonly IStatisticsManager _statisticsManager;
        private readonly GuildInteractionService _guildServices;
        private readonly FileService _fileService;

        private bool _initialServicesRan = false;

        public TimerQueueService(IOptions<BotSettings> botSettings, MixerConstellationService mixerService, ILoggingManager loggingManager,
            PlatformService platformServices, IStatisticsManager statisticsManager, GuildInteractionService guildServices, FileService fileService)
        {
            _botSettings = botSettings.Value;
            _mixerService = mixerService;
            _loggingManager = loggingManager;
            _platformServices = platformServices;
            _statisticsManager = statisticsManager;
            _guildServices = guildServices;
            _fileService = fileService;
        }

        public async Task Init()
        {
            if (_botSettings.PlatformSettings.EnableMixer)
            {
                await _mixerService.ResubscribeToBeamEvents();
                QueueBeamClientCheck();
            }

            if (_botSettings.PlatformSettings.EnableTwitch)
            {
                QueueTwitchChecks();
                QueueTwitchServerCheck();
            }

            if (_botSettings.PlatformSettings.EnableSmashcast)
            {
                QueueHitboxChecks();
            }

            if (_botSettings.PlatformSettings.EnablePicarto)
            {
                QueuePicartoChecks();
            }

            if (_botSettings.PlatformSettings.EnableVidMe)
            {
                QueueVidMeChecks();
            }

            if (_botSettings.PlatformSettings.EnableYouTube)
            {
                QueueYouTubeChecks();
            }

            if (_botSettings.PlatformSettings.EnableMobcrush)
            {
                QueueMobcrushChecks();
            }

            QueueCleanUp();
            QueueUptimeCheckIn();
            QueueHealthChecks();
        }

        public void QueueBeamClientCheck()
        {
            _mixerPingTimer = new Timer(async (e) =>
            {
                Logging.LogMixer("Pinging Mixer.");
                await _mixerService.Ping();
            }, null, 0, 10000);

        }

        public void QueueHitboxChecks()
        {
            _hitboxTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Smashcast;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Smashcast;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogSmashcast("Checking Smashcast Channels.");
                await _platformServices.CheckHitboxLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogSmashcast("Smashcast Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Smashcast);

            _hitboxOwnerTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = Constants.Smashcast;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Smashcast;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogSmashcast("Checking Owner Smashcast Channels.");
                await _platformServices.CheckOwnerHitboxLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogSmashcast("Owner Smashcast Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Smashcast);
        }

        public void QueueTwitchChecks()
        {
            _twitchTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Twitch;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Twitch;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Twitch Channels.");
                await _platformServices.CheckTwitchLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Twitch Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");

                _initialServicesRan = true;
            }, null, 0, _botSettings.IntervalSettings.Twitch);

            _twitchFeedTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Twitch + " - Twitch Feed";
                metrics.ScheduledInterval = _botSettings.IntervalSettings.TwitchFeed;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Twitch Channel Feeds.");
                await _platformServices.CheckTwitchChannelFeeds();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Twitch Channel Feed Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.TwitchFeed);

            _twitchOwnerFeedTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = Constants.Twitch + " - Twitch Feed";
                metrics.ScheduledInterval = _botSettings.IntervalSettings.TwitchFeed;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Owner Twitch Channel Feeds.");
                await _platformServices.CheckTwitchOwnerChannelFeeds();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Owner Twitch Channel Feed Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.TwitchFeed);

            _twitchTeamTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Twitch + " - Teams";
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Twitch;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Twitch Teams.");
                await _platformServices.CheckTwitchTeams();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Checking Twitch Teams Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Twitch);

            _twitchGameTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Twitch + " - Games";
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Twitch;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Twitch Games.");
                await _platformServices.CheckTwitchGames();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Checking Twitch Games Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Twitch);
        }

        public void QueueYouTubeChecks()
        {
            _youtubeTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.YouTube;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.YouTubeLive;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogYouTubeGaming("Checking YouTube Gaming Channels.");
                await _platformServices.CheckYouTubeLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogYouTubeGaming("YouTube Gaming Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.YouTubeLive);

            _youtubePublishedTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.YouTube;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.YouTubePublished;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogYouTube("Checking YouTube Published");
                await _platformServices.CheckPublishedYouTube();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogYouTube("YouTube Published Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.YouTubePublished);

            _youtubePublishedOwnerTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = Constants.YouTube;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.YouTubePublished;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogYouTube("Checking Owner YouTube Published");
                await _platformServices.CheckOwnerPublishedYouTube();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogYouTube("Owner YouTube Published Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.YouTubePublished);
        }

        public void QueueVidMeChecks()
        {
            _vidMeTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.VidMe;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.VidMe;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogVidMe("Checking VidMe");
                await _platformServices.CheckVidMe();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogVidMe("VidMe Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.VidMe);

            _vidMeOwnerTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = Constants.VidMe;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.VidMe;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogVidMe("Checking Owner VidMe Published");
                await _platformServices.CheckOwnerVidMe();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogVidMe("Owner VidMe Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.VidMe);
        }

        public void QueuePicartoChecks()
        {
            _picartoTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Picarto;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Picarto;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogPicarto("Checking Picarto Channels.");
                await _platformServices.CheckPicartoLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogPicarto("Picarto Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Picarto);

            _picartoOwnerTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = Constants.Picarto;
                metrics.ScheduledInterval = _botSettings.IntervalSettings.Picarto;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogPicarto("Checking Picarto Smashcast Channels.");
                await _platformServices.CheckOwnerPicartoLive();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogPicarto("Owner Picarto Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Picarto);
        }

        public void QueueMobcrushChecks()
        {
            _mobcrushTimer = new Timer(async (e) =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogMobcrush("Checking Mobcrush Channels.");
                await _platformServices.CheckMobcrushLive();
                sw.Stop();
                Logging.LogMobcrush("Mobcrush Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Mobcrush);

            _mobcrushOwnerTimer = new Timer(async (e) =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogMobcrush("Checking Owner Mobcrush Channels.");
                await _platformServices.CheckOwnerMobcrushLive();
                sw.Stop();
                Logging.LogMobcrush("Owner Mobcrush Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.Mobcrush);
        }

        public void QueueHealthChecks()
        {
            _guildCheckTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = true;
                metrics.Platform = "Bot - Guild Configuration";
                metrics.ScheduledInterval = 60000;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogInfo("Checking Guild Configurations.");
                _guildServices.CheckGuildConfigurations();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogInfo("Guild Configuration Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, 600000);
        }

        public void QueueTwitchServerCheck()
        {
            _twitchServerTimer = new Timer(async (e) =>
            {
                var metrics = new PerformanceMetrics();
                metrics.CreatedDate = DateTime.UtcNow;
                metrics.IsOwner = false;
                metrics.Platform = Constants.Twitch + " - Discover";
                metrics.ScheduledInterval = _botSettings.IntervalSettings.TwitchServer;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logging.LogTwitch("Checking Twitch Server Channels.");
                await _platformServices.CheckTwitchServer();
                sw.Stop();
                metrics.RunTime = sw.ElapsedMilliseconds;
                await _loggingManager.LogPerformance(metrics);
                Logging.LogTwitch("Twitch Server Check Complete - Elapsed Runtime: " + sw.ElapsedMilliseconds + " milliseconds.");
            }, null, 0, _botSettings.IntervalSettings.TwitchServer);
        }

        public void QueueCleanUp()
        {
            _cleanupTimer = new Timer(async (e) =>
            {
                if (_initialServicesRan)
                {
                    Logging.LogInfo("Cleaning Up Live Files.");

                    if (_botSettings.PlatformSettings.EnableYouTube)
                    {
                        await _platformServices.CleanUpLiveStreams(Constants.YouTubeGaming);
                    }

                    if (_botSettings.PlatformSettings.EnableTwitch)
                    {
                        await _platformServices.CleanUpLiveStreams(Constants.Twitch);
                    }

                    if (_botSettings.PlatformSettings.EnableSmashcast)
                    {
                        await _platformServices.CleanUpLiveStreams(Constants.Smashcast);
                    }

                    if (_botSettings.PlatformSettings.EnablePicarto)
                    {
                        await _platformServices.CleanUpLiveStreams(Constants.Picarto);
                    }

                    if (_botSettings.PlatformSettings.EnableMobcrush)
                    {
                        await _platformServices.CleanUpLiveStreams(Constants.Mobcrush);
                    }

                    Logging.LogInfo("Cleaning Up Live Files Complete.");
                }
            }, null, 0, 300000);
        }

        public void QueueUptimeCheckIn()
        {
            _uptimeTimer = new Timer((e) =>
            {
                using (var httpClient = new HttpClient())
                {
                    Logging.LogInfo("Adding to Uptime.");
                    _statisticsManager.AddUptimeMinutes();
                    Logging.LogInfo("Uptime Update Complete.");
                }
            }, null, 0, 60000);
        }
    }
}
