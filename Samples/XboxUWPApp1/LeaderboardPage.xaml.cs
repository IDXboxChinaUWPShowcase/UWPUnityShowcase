using Microsoft.Xbox.Services.Leaderboard;
using Microsoft.Xbox.Services.Social;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XboxLiveIntegration;
using XboxUWPApp1.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace XboxUWPApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LeaderboardPage : Page
    {
        public LeaderboardPage()
        {
            this.InitializeComponent();
            //LiveResources.GetInstance();
        }

        private async void btnGetLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                var result = await LeaderboardManager.GetInstance().GetLeaderboardAsync("GameProgress");
                if (result != null)
                {
                    ProcessLeaderboard(result);
                }
            }
        }

        private async void btnGetLeaderboardSkipToUser_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                var result = await LeaderboardManager.GetInstance().GetLeaderboardSkipToUserAsync("GameProgress");
                if (result != null)
                {
                    ProcessLeaderboard(result);
                }
            }
        }

        private async void btnGetSocialLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                // Only return people with social relationships who are marked as "Favorite"
                var result = await LeaderboardManager.GetInstance().GetSocialLeaderboardAsync("GameProgress", SocialGroupConstants.Favorite);
                if (result != null)
                {
                    ProcessLeaderboard(result);
                }
            }
        }

        private void ProcessLeaderboard(LeaderboardResult result)
        {
            var output = string.Empty;
            if (result != null)
            {
                // Show information about the leaderboard
                output = output.WriteMessageLine("Leaderboard: " + result.DisplayName);

                // Show the columns in the leaderbard
                output = output.WriteMessageLine("Column count:" + result.Columns.Count.ToString());
                foreach (var column in result.Columns)
                {
                    // Display the LeaderboardColumn property bag
                    output = output.WriteMessageLine(column.DisplayName);
                    output = output.WriteMessageLine(column.StatisticName);
                    output = output.WriteMessageLine(column.StatisticType.ToString());
                }

                // Show the user's row in the leaderboard
                output = output.WriteMessageLine("Row count:" + result.TotalRowCount.ToString());
                foreach (var row in result.Rows)
                {
                    // Display the LeaderboardRow property bag
                    output = output.WriteMessageLine(row.XboxUserId);
                    output = output.WriteMessageLine(row.Gamertag);
                    output = output.WriteMessageLine(row.Percentile.ToString());
                    output = output.WriteMessageLine(row.Rank.ToString());
                }

                WriteOutputMessage(outputMessage, output);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void WriteOutputMessage(ListBox lb, string msg)
        {
            lb.Items.Add(msg);
            lb.SelectedIndex = lb.Items.Count - 1;
            lb.UpdateLayout();
            lb.ScrollIntoView(lb.SelectedItem);
        }
    }
}
