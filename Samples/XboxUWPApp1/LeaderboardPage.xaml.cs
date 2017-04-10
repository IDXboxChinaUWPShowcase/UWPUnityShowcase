using Microsoft.Xbox.Services.Social;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XboxLiveIntegration;

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
            }
        }

        private async void btnGetLeaderboardSkipToUser_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                var result = await LeaderboardManager.GetInstance().GetLeaderboardSkipToUserAsync("GameProgress");
            }
        }

        private async void btnGetSocialLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            if (LiveResources.GetInstance().IsSignedIn)
            {
                // Only return people with social relationships who are marked as "Favorite"
                var result = await LeaderboardManager.GetInstance().GetSocialLeaderboardAsync("GameProgress", SocialGroupConstants.Favorite);
            }
        }
    }
}
