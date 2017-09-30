using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals.Implementations
{
    public class StatisticsDal : IStatisticsDal
    {
        private readonly BotSettings _botSettings;

        public StatisticsDal(IOptions<BotSettings> botSettings)
        {
            _botSettings = botSettings.Value;
        }

        public async Task<BotStats> GetBotStats()
        {
            BotStats stats = null;
            string query = "SELECT * FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {
                stats = new BotStats();

                stats.BeamAlertCount = int.Parse(MyReader2["MixerAlertCount"].ToString());
                stats.FlipCount = int.Parse(MyReader2["FlipCount"].ToString());
                stats.HaiBaiCount = int.Parse(MyReader2["Id"].ToString());
                stats.HitboxAlertCount = int.Parse(MyReader2["SmashcastAlertCount"].ToString());
                stats.LastRestart = DateTime.Parse(MyReader2["LastRestart"].ToString());
                stats.LoggingStartDate = DateTime.Parse(MyReader2["LoggingStartDate"].ToString());
                stats.PicartoAlertCount = int.Parse(MyReader2["PicartoAlertCount"].ToString());
                stats.TwitchAlertCount = int.Parse(MyReader2["TwitchAlertCount"].ToString());
                stats.UnflipCount = int.Parse(MyReader2["UnflipCount"].ToString());
                stats.UptimeMinutes = int.Parse(MyReader2["UptimeMinutes"].ToString());
                stats.VidMeAlertCount = int.Parse(MyReader2["VidMeAlertCount"].ToString());
                stats.YouTubeAlertCount = int.Parse(MyReader2["YouTubeAlertCount"].ToString());
                stats.MobcrushAlertCount = int.Parse(MyReader2["MobcrushAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
             
            return stats;
        }

        public async Task SaveBotStats(BotStats stats)
        {
            var Query = "UPDATE `BotStats` SET `YouTubeAlertCount`=" + stats.YouTubeAlertCount + 
                ",`MixerAlertCount`=" + stats.BeamAlertCount + 
                ",`SmashcastAlertCount`=" + stats.HitboxAlertCount + 
                ",`PicartoAlertCount`=" + stats.PicartoAlertCount + 
                ",`VidMeAlertCount`=" + stats.VidMeAlertCount + 
                ",`UptimeMinutes`=" + stats.UptimeMinutes + 
                ",`HaiBaiCount`=" + stats.HaiBaiCount + 
                ",`FlipCount`=" + stats.FlipCount + 
                ",`UnflipCount`=" + stats.UnflipCount + 
                ",`TwitchAlertCount`=" + stats.TwitchAlertCount + 
                ",`LoggingStartDate`='" + stats.LoggingStartDate + 
                "',`LastRestart`='" + stats.LastRestart.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToBeamAlertCount()
        {
            var current = await GetBeamAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `MixerAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToHitboxAlertCount()
        {
            var current = await GetHitboxAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `SmashcastAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToPicartoAlertCount()
        {
            var current = await GetPicartoAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `PicartoAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToTwitchAlertCount()
        {
            var current = await GetTwitchAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `TwitchAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToYouTubeAlertCount()
        {
            var current = await GetYouTubeAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `YouTubeAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddUptimeMinutes()
        {
            var current = await GetUptimeMinutes().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `UptimeMinutes`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task<int> GetBeamAlertCount()
        {
            string query = "SELECT MixerAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["MixerAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
            
            return count;
        }

        public async Task<int> GetHitboxAlertCount()
        {
            string query = "SELECT SmashcastAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["SmashcastAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task<int> GetTwitchAlertCount()
        {
            string query = "SELECT TwitchAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["TwitchAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task<int> GetUptimeMinutes()
        {
            string query = "SELECT UptimeMinutes FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["UptimeMinutes"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task<int> GetYouTubeAlertCount()
        {
            string query = "SELECT YouTubeAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["YouTubeAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task<int> GetHaiBaiCount()
        {
            string query = "SELECT HaiBaiCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["HaiBaiCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }
        
        public async Task LogRestartTime()
        {
            string query = "Update BotStats SET `LastRestart`='" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "' where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToHaiBaiCount()
        {
            var current = await GetHaiBaiCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `HaiBaiCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task AddToFlipCount()
        {
            var current = await GetFlipCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `FlipCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task<int> GetFlipCount()
        {
            string query = "SELECT FlipCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["FlipCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task AddToUnflipCount()
        {
            var current = await GetUnflipCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `UnflipCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task<int> GetUnflipCount()
        {
            string query = "SELECT UnflipCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["UnflipCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task AddToVidMeAlertCount()
        {
            var current = await GetVidMeAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `VidMeAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task<int> GetVidMeAlertCount()
        {
            string query = "SELECT VidMeAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["VidMeAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task<int> GetPicartoAlertCount()
        {
            string query = "SELECT PicartoAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["PicartoAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }

        public async Task AddToMobcrushAlertCount()
        {
            var current = await GetMobcrushAlertCount().ConfigureAwait(false);
            current++;

            string query = "Update BotStats SET `MobcrushAlertCount`=" + current + " where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {

            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();
        }

        public async Task<int> GetMobcrushAlertCount()
        {
            string query = "SELECT MobcrushAlertCount FROM BotStats where Id = 1";

            MySqlConnection MyConn2 = new MySqlConnection(_botSettings.ConnectionString.BotContext);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            int count = 0;
            while (MyReader2.Read())
            {
                count = int.Parse(MyReader2["MobcrushAlertCount"].ToString());
            }
            MyConn2.Close();
            await MyConn2.ClearPoolAsync(MyConn2);
            await MyConn2.ClearAllPoolsAsync();

            return count;
        }
    }
}
