using Microsoft.Xbox.Services.Achievements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.UI.Core;

namespace XboxLiveIntegration
{
    public class AchievementManager : INotifyPropertyChanged
    {
        private static AchievementManager instance;
        private LiveResources liveResource = null;
        CoreDispatcher UIDispatcher;
        private ObservableCollection<Achievement> achievements;
        private uint maxAchievements = 10;

        public ObservableCollection<Achievement> AllAchievements
        {
            get { return achievements; }
            set
            {
                if (value != achievements)
                {
                    achievements = value;
                    // Notify of the change.
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Max achievement items, default value: 10
        /// </summary>
        public uint MaxItems
        {
            get { return maxAchievements; }
            set { maxAchievements = value; }
        }
        /// <summary>
        /// The wrapper for LiveContext.AchievementService
        /// </summary>
        public AchievementService AchievementService
        {
            get { return liveResource.LiveContext != null ? liveResource.LiveContext.AchievementService : null; }
        }
        /// <summary>
        /// Get Xbox Live AchievementManager instance
        /// </summary>
        /// <returns></returns>
        public static AchievementManager GetInstance()
        {
            if (instance == null)
            {
                instance = new AchievementManager();
            }
            return instance;
        }
        /// <summary>
        /// AchievementManager constructor
        /// </summary>
        public AchievementManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            liveResource = LiveResources.GetInstance();
            Windows.ApplicationModel.Core.CoreApplicationView mainView = Windows.ApplicationModel.Core.CoreApplication.MainView;
            Windows.UI.Core.CoreWindow cw = mainView.CoreWindow;
            UIDispatcher = cw.Dispatcher;

            AllAchievements = new ObservableCollection<Achievement>();
        }

        /// <summary>
        /// Get all achievements for current xbox live user and title, save the Achievement entity to AllAchievements property.
        /// The default max items: 10, set MaxItems property as you wish
        /// </summary>
        public async void GetAllAchievementsSaveToAllAchievements()
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var achievementService = liveResource.LiveContext.AchievementService;
                    var result = await achievementService.GetAchievementsForTitleIdAsync(liveResource.User.XboxUserId, liveResource.TitleId, AchievementType.All, false, AchievementOrderBy.TitleId, 0, maxAchievements);
                    //Reset collection
                    AllAchievements = new ObservableCollection<Achievement>();
                    ProcessAchievements(result);
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
            }
        }

        /// <summary>
        /// Get all achievements for current xbox live user and title.
        /// The default max items: 10, set MaxItems property as you wish
        /// </summary>
        public async Task<AchievementsResult> GetAllAchievements()
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var achievementService = liveResource.LiveContext.AchievementService;
                    var result = await achievementService.GetAchievementsForTitleIdAsync(liveResource.User.XboxUserId, liveResource.TitleId, AchievementType.All, false, AchievementOrderBy.TitleId, 0, maxAchievements);
#if DEBUG
                    ProcessAchievements(result);
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
        /// Get achievement by Achievement id for current xbox live user and scid, save the Achievement entity to AllAchievements property
        /// </summary>
        /// <param name="singleAchievementId">Achievement id</param>
        public async void GetAchievementSaveToAllAchievements(string singleAchievementId)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var achievementService = liveResource.LiveContext.AchievementService;
                    var result = await achievementService.GetAchievementAsync(liveResource.User.XboxUserId, liveResource.ServiceConfigId, singleAchievementId);
                    //Reset collection
                    AllAchievements = new ObservableCollection<Achievement>();
                    GenerateItems(result);
#if DEBUG
                    PrintAchievement(result);
#endif
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
            }
        }

        /// <summary>
        /// Get achievement by Achievement id for current xbox live user and scid
        /// </summary>
        /// <param name="singleAchievementId">Achievement id</param>
        /// <returns></returns>
        public async Task<Achievement> GetAchievement(string singleAchievementId)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    var achievementService = liveResource.LiveContext.AchievementService;
                    var result = await achievementService.GetAchievementAsync(liveResource.User.XboxUserId, liveResource.ServiceConfigId, singleAchievementId);
#if DEBUG
                    PrintAchievement(result);
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
        /// The wrapper method of writing game event
        /// </summary>
        /// <param name="gameEventName">Game Event Name</param>
        /// <param name="dimensions">Dimensions include event fields with a finite number of defined numeric or string values. Examples of dimensions: map id, difficulty level, character or weapon class, game mode, boolean settings, etc.</param>
        /// <param name="measurements">Measurements include event fields that represent scalar numeric metrics. Examples of measurements: score, time, counters, position, etc.</param>
        public void WriteGameEvent(string gameEventName, Windows.Foundation.Collections.PropertySet dimensions = null, Windows.Foundation.Collections.PropertySet measurements = null)
        {
            if (liveResource != null && liveResource.LiveContext != null)
            {
                try
                {
                    liveResource.LiveContext.EventsService.WriteInGameEvent(gameEventName, dimensions, measurements);
                }
                catch (Exception ex)
                {
                    // Handle errors
                    Debug.WriteLine($"Failed to write event, exception:\r\n{ex.Message}");
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("Please Sign in");
            }
        }

        private ObservableCollection<Achievement> GenerateItems(IReadOnlyList<Achievement> items)
        {
            foreach (var item in items)
            {
                AllAchievements.Add(item);
            }
            return AllAchievements;
        }
        private ObservableCollection<Achievement> GenerateItems(Achievement item)
        {
            AllAchievements.Add(item);
            return AllAchievements;
        }
        private async void ProcessAchievements(AchievementsResult result)
        {
            if (result != null)
            {
#if DEBUG
                foreach (var achievement in result.Items)
                {
                    PrintAchievement(achievement);
                }
#endif
                if (result.Items.Count > 0)
                {
                    GenerateItems(result.Items);
                }

                // Keep processing if there are more Achievements.
                if (result.HasNext)
                {
                    var _result = await result.GetNextAsync(maxAchievements);
                    ProcessAchievements(_result);
                }
            }
        }
        private void PrintAchievement(Achievement achievement)
        {
            Debug.WriteLine($"Achievement Id: {achievement.Id} ");
            Debug.WriteLine($"Name: {achievement.Name} ");
            if (achievement.ProgressState == AchievementProgressState.Achieved)
            {
                Debug.WriteLine($"Description: { achievement.UnlockedDescription} ");
            }
            else
            {
                Debug.WriteLine($"Description: { achievement.LockedDescription} ");
            }
            Debug.WriteLine($"Achievement Type: {achievement.AchievementType} ");
            Debug.WriteLine($"Progress State: {achievement.ProgressState} ");

        }
        
        #region INotifyPropertyChanged Interface
        // PropertyChanged event.
        public event PropertyChangedEventHandler PropertyChanged;

        // PropertyChanged event triggering method.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
