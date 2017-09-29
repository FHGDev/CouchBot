using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTD.CouchBot.Dals;
using MTD.CouchBot.Dals.Implementations;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Managers.Implementations;
using MTD.CouchBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MTD.CouchBot
{
    public class Program
    {
        private IConfigurationRoot _config;
        private DiscordShardedClient _discord;

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            Logging.LogInfo("Starting the Bot.");
            Logging.LogInfo("Building the Configuration.");
            var builder = new ConfigurationBuilder()    
                .SetBasePath(AppContext.BaseDirectory)  
                .AddJsonFile("BotSettings.json"); 
            _config = builder.Build();
            Logging.LogInfo("Completed - Building the Configuration.");

            Logging.LogInfo("Configuring the Services");
            var provider = ConfigureServices();
            Logging.LogInfo("Completed - Configuring the Services");
                        
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();
            _discord = provider.GetRequiredService<DiscordShardedClient>();

            while (_discord.CurrentUser == null)
            {
                Logging.LogInfo("Waiting for User to Log In...");
                Thread.Sleep(1000);
            }

            Logging.LogInfo("Setting Up Event Handlers.");
            provider.GetRequiredService<GuildInteractionService>().Init();
            Logging.LogInfo("Completed - Setting Up Event Handlers.");

            Logging.LogInfo("Initializing Timers.");
            await provider.GetRequiredService<TimerQueueService>().Init();
            Logging.LogInfo("Completed - Initializing Timers.");

            await Task.Delay(-1);
        }

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    TotalShards = int.Parse(_config["BotConfig:TotalShards"]),
                    AlwaysDownloadUsers = true
                }))
                .AddSingleton(new CommandService())
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<MixerConstellationService>()
                .AddSingleton<FileService>()
                .AddSingleton<GuildInteractionService>()
                .AddSingleton<DiscordService>()
                .AddSingleton<MessagingService>()
                .AddSingleton<PlatformService>()
                .AddSingleton<TimerQueueService>()
                .AddSingleton<IAlertDal, AlertDal>()
                .AddSingleton<IApiAiDal, ApiAiDal>()
                .AddSingleton<ILoggingDal, LoggingDal>()
                .AddSingleton<IMixerDal, MixerDal>()
                .AddSingleton<IMobcrushDal, MobcrushDal>()
                .AddSingleton<IPicartoDal, PicartoDal>()
                .AddSingleton<ISmashcastDal, SmashcastDal>()
                .AddSingleton<IStatisticsDal, StatisticsDal>()
                .AddSingleton<IStrawpollDal, StrawPollDal>()
                .AddSingleton<ITwitchDal, TwitchDal>()
                .AddSingleton<IVidMeDal, VidMeDal>()
                .AddSingleton<IYouTubeDal, YouTubeDal>()
                .AddSingleton<IAlertManager, AlertManager>()
                .AddSingleton<IApiAiManager, ApiAiManager>()
                .AddSingleton<ILoggingManager, LoggingManager>()
                .AddSingleton<IMixerManager, MixerManager>()
                .AddSingleton<IMobcrushManager, MobcrushManager>()
                .AddSingleton<IPicartoManager, PicartoManager>()
                .AddSingleton<ISmashcastManager, SmashcastManager>()
                .AddSingleton<IStatisticsManager, StatisticsManager>()
                .AddSingleton<IStrawPollManager, StrawPollManager>()
                .AddSingleton<ITwitchManager, TwitchManager>()
                .AddSingleton<IVidMeManager, VidMeManager>()
                .AddSingleton<IYouTubeManager, YouTubeManager>()
                .AddSingleton<Random>();

            services.AddOptions();
            services.Configure<BotSettings>(_config);

            return services.BuildServiceProvider();
        }

        //public async Task BroadcastMessage(string message)
        //{
        //    var serverFiles = Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory);

        //    foreach (var serverFile in serverFiles)
        //    {
        //        var serverId = Path.GetFileNameWithoutExtension(serverFile);
        //        var serverJson = File.ReadAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + serverId + ".json");
        //        var server = JsonConvert.DeserializeObject<DiscordServer>(serverJson);

        //        if (!string.IsNullOrEmpty(server.AnnouncementsChannel.ToString()) && server.AnnouncementsChannel != 0)
        //        {
        //            var chat = await _discordService.GetMessageChannel(server.Id, server.AnnouncementsChannel);

        //            if (chat != null)
        //            {
        //                try
        //                {
        //                    await chat.SendMessageAsync("**[" + _discord.CurrentUser.Username + "]** " + message);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logging.LogError("Broadcast Message Error: " + ex.Message + " in server " + serverId);
        //                }
        //            }
        //        }
        //    }
        //}

        //public async Task<ChannelMessage> SendMessage(BroadcastMessage message)
        //{
        //    var chat = await DiscordHelper.GetMessageChannel(message.GuildId, message.ChannelId);

        //    if (chat != null)
        //    {
        //        try
        //        {
        //            ChannelMessage channelMessage = new ChannelMessage();
        //            channelMessage.ChannelId = message.ChannelId;
        //            channelMessage.GuildId = message.GuildId;
        //            channelMessage.DeleteOffline = message.DeleteOffline;

        //            if (message.Embed != null)
        //            {
        //                RequestOptions options = new RequestOptions();
        //                options.RetryMode = RetryMode.AlwaysRetry;
        //                var msg = await chat.SendMessageAsync(message.Message, false, message.Embed, options);

        //                if (msg != null || msg.Id != 0)
        //                {
        //                    channelMessage.MessageId = msg.Id;
        //                }
        //            }
        //            else
        //            {
        //                var msg = await chat.SendMessageAsync(message.Message);

        //                if (msg != null || msg.Id != 0)
        //                {
        //                    channelMessage.MessageId = msg.Id;
        //                }
        //            }

        //            if (message.Platform.Equals(Constants.YouTube))
        //            {
        //                statisticsManager.AddToYouTubeAlertCount();
        //            }

        //            if (message.Platform.Equals(Constants.Twitch))
        //            {
        //                statisticsManager.AddToTwitchAlertCount();
        //            }

        //            if (message.Platform.Equals(Constants.Mixer))
        //            {
        //                statisticsManager.AddToBeamAlertCount();
        //            }

        //            if (message.Platform.Equals(Constants.Smashcast))
        //            {
        //                statisticsManager.AddToHitboxAlertCount();
        //            }

        //            if (message.Platform.Equals(Constants.VidMe))
        //            {
        //                statisticsManager.AddToVidMeAlertCount();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logging.LogError("Send Message Error: " + ex.Message + " in server " + message.GuildId);
        //        }
        //    }

        //    return null; // we never get here :(
        //}

        //private async Task ChatStuff()
        //{
        //    UserCredential credential;

        //    using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
        //    {
        //        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //                GoogleClientSecrets.Load(stream).Secrets,
        //                new[] { YouTubeService.Scope.Youtube },
        //                "user",
        //                CancellationToken.None,
        //                new FileDataStore(@"C:\ProgramData\ChatClient\" + this.GetType().ToString())
        //            );
        //    }

        //    youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = this.GetType().ToString()
        //    });

        //    var videoRequest = youtubeService.Videos.List("liveStreamingDetails");
        //    videoRequest.Id = "7djd-be6HJE";
        //    var videoResponse = await videoRequest.ExecuteAsync();

        //    if (videoResponse.Items == null || videoResponse.Items.Count == 0)
        //    {
        //        Console.WriteLine("Sorry, video not found. Stopping.\r\nHit enter to exit.");
        //        Console.ReadLine();
        //        return;
        //    }

        //    liveChatId = videoResponse.Items[0].LiveStreamingDetails.ActiveLiveChatId;
        //    var keepGoing = true;

        //    Timer chatTimer;
        //    int interval = 10000;
        //    PollChatResponse initResponse = null;

        //    chatTimer = new Timer(async (e) =>
        //    {
        //        var response = await PollChat(youtubeService, liveChatId, initResponse);
        //        initResponse = response;
        //        interval = response.NextInterval;
        //    }, null, 0, interval);
        //}

        //private async Task<PollChatResponse> PollChat(YouTubeService youtubeService, string liveChatId, PollChatResponse lastResponse)
        //{
        //    var chatMessageRequest = youtubeService.LiveChatMessages.List(liveChatId, "snippet,authorDetails");
        //    if (lastResponse != null)
        //    {
        //        chatMessageRequest.PageToken = lastResponse.NextPageToken;
        //    }
        //    var chatMessageResponse = await chatMessageRequest.ExecuteAsync();

        //    if (lastResponse != null)
        //    {
        //        foreach (var item in chatMessageResponse.Items)
        //        {
        //            await ProcessInput(item, liveChatId, youtubeService);
        //        }
        //    }

        //    var nextInterval = chatMessageResponse.PollingIntervalMillis.HasValue ?
        //        chatMessageResponse.PollingIntervalMillis.Value : 10000;

        //    return new PollChatResponse()
        //    {
        //        NextInterval = (int)nextInterval,
        //        NextPageToken = chatMessageResponse.NextPageToken
        //    };
        //}

        //private async Task ProcessInput(LiveChatMessage message, string liveChatId, YouTubeService youtubeService)
        //{
        //    var guild = client.GetGuild(263688866978988032);
        //    var channel = (IMessageChannel) guild.GetChannel(263688866978988032);

        //    await channel.SendMessageAsync("**[YouTube]** " + message.AuthorDetails.DisplayName + ": " + message.Snippet.DisplayMessage);
        //    //Console.WriteLine(message.AuthorDetails.DisplayName + ": " + message.Snippet.DisplayMessage);

        //    //if (message.Snippet.DisplayMessage.Equals("Hi Couchbot", StringComparison.CurrentCultureIgnoreCase))
        //    //{
        //    //    await SendMessage("Hi back at you, " + message.AuthorDetails.DisplayName + "!", liveChatId, youtubeService);
        //    //}
        //}

        //private async Task SendMessage(string message, string liveChatId, YouTubeService youtubeService)
        //{
        //    var chatMessage = new LiveChatMessage();
        //    var chatMessageSnippet = new LiveChatMessageSnippet();
        //    var chatMessageDetails = new LiveChatTextMessageDetails();

        //    chatMessageSnippet.LiveChatId = liveChatId;
        //    chatMessageSnippet.Type = "textMessageEvent";

        //    chatMessageDetails.MessageText = message;
        //    chatMessageSnippet.TextMessageDetails = chatMessageDetails;
        //    chatMessage.Snippet = chatMessageSnippet;

        //    var chatMessageRequest = youtubeService.LiveChatMessages.Insert(chatMessage, "snippet");
        //    var chatMessageResponse = await chatMessageRequest.ExecuteAsync();
        //}
    }
}