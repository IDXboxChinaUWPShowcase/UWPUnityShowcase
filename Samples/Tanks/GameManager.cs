using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


#if NETFX_CORE
using System.Collections.Generic;
using Windows.UI.Notifications;
using XboxLiveIntegration;
#endif


namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 3;            // The number of rounds a single player has to win to win the game.
        private bool isTrytoSilentSignin = true;
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.


        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.


        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }

#if NETFX_CORE
        GUIStyle m_guiStyle = new GUIStyle();
        string m_logText = string.Empty;
        List<string> m_logLines = new List<string>();

        #region Xbox Live
        private IEnumerator SignInSilent()
        {
            var liveResource = XboxLiveIntegration.LiveResources.GetInstance();

            yield return new WaitForSeconds(1f);

            if (liveResource.IsSignedIn)
            {
                var msg = $"You're signed in, gametag: {liveResource.User.Gamertag}";
                LogLine(msg);
                ShowToastNotification("Xbox Live", msg);
            }
        }
        /// <summary>
        /// Xbox Live Sign in with UI
        /// </summary>
        void SignIn()
        {
            try
            {
                var liveResource = XboxLiveIntegration.LiveResources.GetInstance();
                liveResource.SignIn();

                if (liveResource.IsSignedIn)
                {
                    LogLine($"You're signed in, gametag: {liveResource.User.Gamertag}");
                }
            }
            catch (System.Exception ex)
            {
                LogLine("SignIn failed: " + ex.ToString());
            }
        }
        /// <summary>
        /// Xbox Live Switch Account
        /// </summary>
        void SwitchAccount()
        {
            try
            {
                var liveResource = XboxLiveIntegration.LiveResources.GetInstance();
                liveResource.SwitchAccount();
                if (liveResource.IsSignedIn)
                {
                    LogLine($"You're signed in, gametag: {liveResource.User.Gamertag}");
                }
            }
            catch (System.Exception ex)
            {
                LogLine("SignIn failed: " + ex.ToString());
            }
        }
        /// <summary>
        /// Unlock First Win Achievement
        /// </summary>
        /// <param name="totalWinCount">TotalWinCount</param>
        void UnlockFirstWinAchievement(int totalWinCount)
        {
            var liveResource = XboxLiveIntegration.LiveResources.GetInstance();
            var measurements = new Windows.Foundation.Collections.PropertySet();
            measurements.Add("TotalWinCount", totalWinCount);

            var dimensions = new Windows.Foundation.Collections.PropertySet();
            dimensions.Add("UserId", liveResource.User.XboxUserId);

            AchievementManager.GetInstance().WriteGameEvent("TotalDataUpdate", dimensions, measurements);
            System.Diagnostics.Debug.WriteLine("Wrote game event: TotalDataUpdate");
        }
        /// <summary>
        /// Get Xbox Live Achievement by Id
        /// </summary>
        /// <param name="id"></param>
        async void GetAchievementById(int id)
        {
            var ach = await AchievementManager.GetInstance().GetAchievement(id.ToString());
            if (ach.ProgressState == Microsoft.Xbox.Services.Achievements.AchievementProgressState.Achieved)
            {
                var msg = $"You achieved {ach.Name}";
                System.Diagnostics.Debug.WriteLine(msg);
                LogLine(msg);
                ShowToastNotification("Xbox Live Achievement", msg);
            }
        }
        /// <summary>
        /// Get Xbox Live Leaderboard by Leaderboard name
        /// </summary>
        /// <param name="name"></param>
        async void GetLeaderboardByName(string name)
        {
            var result = await LeaderboardManager.GetInstance().GetLeaderboardAsync(name);
        }
        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);

            GetAchievementById(1);
        }
        #endregion

        #region UI
        private void DrawTextWithShadow(float x, float y, float width, float height, string text)
        {
            m_guiStyle.fontSize = 14;
            m_guiStyle.normal.textColor = Color.black;
            GUI.Label(new UnityEngine.Rect(x, y, height, height), text, m_guiStyle);
            m_guiStyle.normal.textColor = Color.white;
            GUI.Label(new UnityEngine.Rect(x - 1, y - 1, width, height), text, m_guiStyle);
        }

        public void LogLine(string line)
        {
            lock (m_logText)
            {
                if (m_logLines.Count > 5)
                {
                    m_logLines.RemoveAt(0);
                }
                m_logLines.Add($"{System.DateTime.Now} - {line}");

                m_logText = string.Empty;
                foreach (string s in m_logLines)
                {
                    m_logText += "\n";
                    m_logText += s;
                }
            }
        }

        public void OnGUI()
        {
            lock (m_logText)
            {
                DrawTextWithShadow(10, 50, 800, 500, m_logText);
            }

            if (GUI.Button(new Rect(10, 10, 100, 30), "Sign-in"))
            {
                SignIn();
            }

            if (GUI.Button(new Rect(120, 10, 100, 30), "Change Users"))
            {
                SwitchAccount();
            }
            if (GUI.Button(new Rect(230, 10, 100, 30), "Quit"))
            {
                UnityEngine.Application.Quit();
            }
        }
        #endregion

        #region ToastNotificationHelper
        private void ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = System.DateTime.Now.AddSeconds(10);
            ToastNotifier.Show(toast);
        }
        #endregion

        private void Update()
        {
            if (Input.GetKeyDown("t") || Input.GetKeyDown("joystick button 2"))
            {
                this.SignIn();
            }
            if (Input.GetKeyDown("y") || Input.GetKeyDown("joystick button 3"))
            {
                this.SwitchAccount();
            }
            if (Input.GetKey("escape") || Input.GetKeyDown("joystick button 6"))
                UnityEngine.Application.Quit();
        }
#endif

        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }


        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());
#if NETFX_CORE
            #region Silently sign in
            if (isTrytoSilentSignin) {
                isTrytoSilentSignin = false;
                yield return StartCoroutine(SignInSilent());
            }
            #endregion
#endif
            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                isTrytoSilentSignin = true;
                // If there is a game winner, restart the level.
                SceneManager.LoadScene(0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks();
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }
        
        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;
#if NETFX_CORE
            if(message.Contains("WINS THE GAME"))
                yield return new WaitForSeconds(2);
#endif
            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }


        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                {
#if NETFX_CORE
                    var liveResource = XboxLiveIntegration.LiveResources.GetInstance();
                    if (liveResource.IsSignedIn)
                        this.UnlockFirstWinAchievement(1);
                    else
                        LogLine("Please sign in to Xbox Live first");

                    if (liveResource.IsSignedIn)
                    {
                        StartCoroutine(ExecuteAfterTime(2));
                    }
#endif

                    return m_Tanks[i];
                }
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}