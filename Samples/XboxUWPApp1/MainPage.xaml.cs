using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XboxLiveIntegration;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XboxUWPApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = AchievementManager.GetInstance();
        }
        
        private void btnGetAchievements_Click(object sender, RoutedEventArgs e)
        {
            if(LiveResources.GetInstance().IsSignedIn)
                AchievementManager.GetInstance().GetAllAchievementsSaveToAllAchievements();
        }

        private void btnGetAchievement_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
                AchievementManager.GetInstance().GetAchievementSaveToAllAchievements("1");
        }

        private void btnWriteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                //UnlockFirstDeath(1);
                UpdateGameProgress(20);
            }
        }

        /// <summary>
        /// Unlock First Dead Achievement
        /// </summary>
        /// <param name="totalDeadCount">TotalDeadCount</param>
        public void UnlockFirstDeath(int totalDeadCount)
        {
            var measurements = new Windows.Foundation.Collections.PropertySet();
            measurements.Add("TotalDeadCount", totalDeadCount);

            var dimensions = new Windows.Foundation.Collections.PropertySet();
            dimensions.Add("UserId", LiveResources.GetInstance().User.XboxUserId);

            AchievementManager.GetInstance().WriteGameEvent("LevelTotalDataUpdate", dimensions, measurements);
            System.Diagnostics.Debug.WriteLine("Wrote game event: LevelTotalDataUpdate");
        }

        public void UpdateGameProgress(float percent)
        {
            var measurements = new Windows.Foundation.Collections.PropertySet();
            measurements.Add("CompletionPercent", percent);

            var dimensions = new Windows.Foundation.Collections.PropertySet();
            dimensions.Add("UserId", LiveResources.GetInstance().User.XboxUserId);

            AchievementManager.GetInstance().WriteGameEvent("GameProgress", dimensions, measurements);
            System.Diagnostics.Debug.WriteLine("Wrote game event: GameProgress");
            
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            LiveResources.GetInstance().SignIn();
        }

        private void btnSwitchUser_Click(object sender, RoutedEventArgs e)
        {
            LiveResources.GetInstance().SwitchAccount();
        }

        private void btnToLeaderboardPage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LeaderboardPage));
        }
    }
}
