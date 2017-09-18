using Microsoft.Xbox.Services;
using Microsoft.Xbox.Services.System;
using System;
using System.Diagnostics;

namespace XboxLiveIntegration
{
    public class LiveResources
    {
        private static LiveResources instance;
        private XboxLiveContext xboxLiveContext;
        private XboxLiveUser xboxLiveUser;
        private string gamertag;
        private uint titleId;
        private string scid;
        private Windows.UI.Core.CoreDispatcher UIDispatcher = null;
        /// <summary>
        /// Get LiveResources instance
        /// </summary>
        /// <returns></returns>
        public static LiveResources GetInstance()
        {
            if (instance == null)
            {
                instance = new LiveResources();
            }
            return instance;
        }

        /// <summary>
        /// Check if the Xbox user is logged in
        /// </summary>
        public bool IsSignedIn
        {
            get { return xboxLiveUser != null && xboxLiveUser.IsSignedIn; }
        }
        /// <summary>
        /// Get Xbox Live User
        /// </summary>
        public XboxLiveUser User
        {
            get { return xboxLiveUser; }
        }
        /// <summary>
        /// Get Xbox Live Context
        /// </summary>
        public XboxLiveContext LiveContext
        {
            get { return xboxLiveContext; }
        }

        /// <summary>
        /// Get Xbox Live Title Id
        /// </summary>
        public uint TitleId
        {
            get { return titleId; }
        }

        /// <summary>
        /// Get Service Config Id
        /// </summary>
        public string ServiceConfigId
        {
            get { return scid; }
        }

        public LiveResources()
        {
            Initialize();
        }

        private void Initialize()
        {
            xboxLiveUser = new XboxLiveUser();
            Windows.ApplicationModel.Core.CoreApplicationView mainView = Windows.ApplicationModel.Core.CoreApplication.MainView;
            Windows.UI.Core.CoreWindow cw = mainView.CoreWindow;

            UIDispatcher = cw.Dispatcher;

            Refresh();
        }

        private void HandleSignInResult(SignInResult signInResult)
        {
            if (signInResult != null)
            {
                switch (signInResult.Status)
                {
                    case SignInStatus.Success:
                        Refresh();
                        break;
                    case SignInStatus.UserCancel:
                        Debug.WriteLine("Error: User canceled");
                        UpdateCurrentUser();
                        break;
                    case SignInStatus.UserInteractionRequired:
                        Debug.WriteLine("Error: User interaction required");
                        UpdateCurrentUser();
                        break;
                }
            }
        }

        private void UpdateCurrentUser()
        {
            if (xboxLiveUser != null && xboxLiveUser.IsSignedIn)
            {
                XboxLiveUser.SignOutCompleted += XboxLiveUser_SignOutCompleted;
                xboxLiveContext = new XboxLiveContext(xboxLiveUser);
                gamertag = xboxLiveUser.Gamertag;
                Debug.WriteLine("Signed in, gametag: " + gamertag);
            }
            else
            {
                xboxLiveUser = new XboxLiveUser();
                xboxLiveContext = null;
                gamertag = null;
                Debug.WriteLine("Please sign in");
                //SignIn();//Re-sign for Debug
            }
        }

        private void XboxLiveUser_SignOutCompleted(object sender, SignOutCompletedEventArgs e)
        {
            UpdateCurrentUser();
        }

        private async void Refresh()
        {
            // Get the XboxLiveConfiguration that holds the title ID for the current game.  
            var xblConfig = XboxLiveAppConfiguration.SingletonInstance;
            titleId = xblConfig.TitleId;
            scid = xblConfig.ServiceConfigurationId;
            if (!String.IsNullOrEmpty(xblConfig.Sandbox))
            {
                Debug.WriteLine("sandbox: " + xblConfig.Sandbox);
            }

            if (!xboxLiveUser.IsSignedIn)
            {
                try
                {
                    var signInResult = await xboxLiveUser.SignInSilentlyAsync(UIDispatcher);
                    HandleSignInResult(signInResult);
                }
                catch (Exception ex)
                {
                    string errorStr = "Silently sign in failed: " + ex.Message;
                    Debug.WriteLine(errorStr);
                    UpdateCurrentUser();
                    //throw; //Comment to avoid crashing
                }
            }
            else
            {
                UpdateCurrentUser();
            }
        }

        public async void SignIn()
        {
            try
            {
                var signInResult = await xboxLiveUser.SignInAsync(UIDispatcher);
                HandleSignInResult(signInResult);
            }
            catch (Exception ex)
            {
                string errorStr = "Sign in failed: " + ex.Message;
                Debug.WriteLine(errorStr);
                UpdateCurrentUser();
                //throw; //Comment to avoid crashing
            }
        }
        [Obsolete]
        public void SwitchAccount()
        {
        }
    }
}
