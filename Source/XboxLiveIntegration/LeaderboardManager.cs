using Microsoft.Xbox.Services.Leaderboard;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace XboxLiveIntegration
{
    public class LeaderboardManager
    {
        private static LeaderboardManager instance;
        private LiveResources liveResource = null;
        private uint maxLeaderboards = 100;

        /// <summary>
        /// Max leaderboard items, default value: 100
        /// </summary>
        public uint MaxItems
        {
            get { return maxLeaderboards; }
            set { maxLeaderboards = value; }
        }
        /// <summary>
        /// The wrapper for LiveContext.LeaderboardService
        /// </summary>
        public LeaderboardService LeaderboardService
        {
            get { return liveResource.LiveContext != null ? liveResource.LiveContext.LeaderboardService : null; }
        }
        /// <summary>
        /// Get Xbox Live LeaderboardManager instance
        /// </summary>
        /// <returns></returns>
        public static LeaderboardManager GetInstance()
        {
            if (instance == null)
            {
                instance = new LeaderboardManager();
            }
            return instance;
        }
        /// <summary>
        /// LeaderboardManager constructor
        /// </summary>
        public LeaderboardManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            liveResource = LiveResources.GetInstance();
        }

        /// <summary>
        /// Retrieving a leaderboard
        /// </summary>
        /// <param name="leaderboardName">Leaderboard Name</param>
        public async Task<LeaderboardResult> GetLeaderboardAsync(string leaderboardName)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var leaderboardService = liveResource.LiveContext.LeaderboardService;
                    var result = await leaderboardService.GetLeaderboardAsync(liveResource.ServiceConfigId, leaderboardName);
#if DEBUG
                    ProcessLeaderboard(result);
#endif
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("Please Sign in");
                return null;
            }
        }

        /// <summary>
        /// Retrieving a leaderboard entry for a User.
        /// The default max items: 100, set MaxItems property as you wish
        /// </summary>
        /// <param name="leaderboardName">Leaderboard Name</param>
        /// <returns></returns>
        public async Task<LeaderboardResult> GetLeaderboardSkipToUserAsync(string leaderboardName)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var leaderboardService = liveResource.LiveContext.LeaderboardService;
                    var result = await leaderboardService.GetLeaderboardWithSkipToUserAsync(liveResource.ServiceConfigId, leaderboardName, liveResource.User.XboxUserId, maxLeaderboards);
#if DEBUG
                    ProcessLeaderboard(result);
#endif
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("Please Sign in");
                return null;
            }
        }

        /// <summary>
        /// Retrieving a leaderboard with a friend on it.
        /// Get a leaderboard containing a socially linked friend. Get a friend's leaderboard.
        /// </summary>
        /// <param name="statisticName">Statistic name. This is defined by the developer on the Xbox Development Portal</param>
        /// <param name="socialGroup">Social Group</param>
        /// <returns></returns>
        public async Task<LeaderboardResult> GetSocialLeaderboardAsync(string statisticName, string socialGroup)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    socialGroup = socialGroup == "Favorites" ? "Favorite" : socialGroup;
                    var leaderboardService = liveResource.LiveContext.LeaderboardService;
                    var result = await leaderboardService.GetLeaderboardForSocialGroupAsync(liveResource.User.XboxUserId, liveResource.ServiceConfigId, statisticName, socialGroup, maxLeaderboards);
#if DEBUG
                    ProcessLeaderboard(result);
#endif
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("Please Sign in");
                return null;
            }
        }

        private void ProcessLeaderboard(LeaderboardResult result)
        {
            if (result != null)
            {
                // Show information about the leaderboard
                Debug.WriteLine("Leaderboard: " + result.DisplayName);

                // Show the columns in the leaderbard
                Debug.WriteLine("Column count:" + result.Columns.Count.ToString());
                foreach (var column in result.Columns)
                {
                    // Display the LeaderboardColumn property bag
                    Debug.WriteLine(column.DisplayName);
                    Debug.WriteLine(column.StatisticName);
                    Debug.WriteLine(column.StatisticType.ToString());
                }

                // Show the user's row in the leaderboard
                Debug.WriteLine("Row count:" + result.TotalRowCount.ToString());
                foreach (var row in result.Rows)
                {
                    // Display the LeaderboardRow property bag
                    Debug.WriteLine(row.XboxUserId);
                    Debug.WriteLine(row.Gamertag);
                    Debug.WriteLine(row.Percentile.ToString());
                    Debug.WriteLine(row.Rank.ToString());
                }
            }
        }
    }
}
