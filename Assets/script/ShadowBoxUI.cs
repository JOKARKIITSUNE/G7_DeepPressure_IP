using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniGames
{
    /// <summary>
    /// Drives a 2D Canvas UI for ShadowBoxGame. Wire the fields below to a
    /// Screen Space - Overlay canvas panel in the Inspector; see the setup
    /// steps in chat for the exact hierarchy this expects.
    /// </summary>
    public class ShadowBoxUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShadowBoxGame game;

        [Tooltip("The root panel for the whole minigame screen. Enabled/disabled by ShowMinigame/HideMinigame.")]
        [SerializeField] private GameObject minigameRoot;

        [Header("Role & round labels")]
        [SerializeField] private TMP_Text playerRoleText;
        [SerializeField] private TMP_Text botRoleText;
        [SerializeField] private TMP_Text roundNumberText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text revealText;

        [Header("Strike pips (assign exactly strikesToLose images each)")]
        [SerializeField] private Image[] playerStrikePips;
        [SerializeField] private Image[] botStrikePips;
        [SerializeField] private Color pipEmptyColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color pipFilledColor = Color.white;

        [Header("Direction buttons (optional on-screen input)")]
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [Header("Sound cues")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Played when a round resolves as a match (a strike lands, either side)")]
        [SerializeField] private AudioClip correctClip;
        [Tooltip("Played when a round resolves as a miss (roles flip)")]
        [SerializeField] private AudioClip wrongClip;

        [Header("Win panel")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TMP_Text winText;
        [SerializeField] private Button restartButton;

        [Tooltip("Optional: button on the win panel that continues to the next part of the game instead of restarting")]
        [SerializeField] private Button continueButton;

        [Tooltip("Scene to load when Continue is pressed. Leave blank if you don't want an automatic scene change.")]
        [SerializeField] private string nextSceneName;

        private void Awake()
        {
            if (upButton != null) upButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Up));
            if (downButton != null) downButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Down));
            if (leftButton != null) leftButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Left));
            if (rightButton != null) rightButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Right));
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueToNextScene);
        }

        private void OnEnable()
        {
            game.OnRoundStarted += HandleRoundStarted;
            game.OnRoundResolved += HandleRoundResolved;
            game.OnRoleFlipped += HandleRoleFlipped;
            game.OnStrikeTaken += HandleStrikeTaken;
            game.OnGameWon += HandleGameWon;
        }

        private void OnDisable()
        {
            game.OnRoundStarted -= HandleRoundStarted;
            game.OnRoundResolved -= HandleRoundResolved;
            game.OnRoleFlipped -= HandleRoleFlipped;
            game.OnStrikeTaken -= HandleStrikeTaken;
            game.OnGameWon -= HandleGameWon;
        }

        /// <summary>Call this from a trigger/interact script to open the minigame screen and begin play.</summary>
        public void ShowMinigame()
        {
            if (minigameRoot != null) minigameRoot.SetActive(true);
            if (winPanel != null) winPanel.SetActive(false);
            game.StartGame();
        }

        /// <summary>Call this from a trigger/interact script to close the minigame screen.</summary>
        public void HideMinigame()
        {
            game.StopGame();
            if (minigameRoot != null) minigameRoot.SetActive(false);
        }

        private void Restart()
        {
            if (winPanel != null) winPanel.SetActive(false);
            game.StartGame();
        }

        private void HandleRoundStarted()
        {
            if (revealText != null) revealText.text = string.Empty;
            if (statusText != null) statusText.text = "Your move";
            UpdateRoleLabels();
            UpdatePips();
            if (roundNumberText != null) roundNumberText.text = "Round " + game.RoundNumber;
        }

        private void HandleRoundResolved(PunchDirection playerDir, PunchDirection botDir, bool wasHit)
        {
            if (revealText != null)
            {
                revealText.text = "You: " + Arrow(playerDir) + "    Bot: " + Arrow(botDir);
            }
            UpdateRoleLabels();
            UpdatePips();
            if (roundNumberText != null) roundNumberText.text = "Round " + game.RoundNumber;
            PlayCue(wasHit ? correctClip : wrongClip);
        }

        private void PlayCue(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        private void HandleStrikeTaken(Actor struckActor)
        {
            if (statusText == null) return;
            statusText.text = struckActor == Actor.Player
                ? "The bot caught you -- strike against you"
                : "You caught the bot -- strike landed";
        }

        private void HandleRoleFlipped(Actor newPointer)
        {
            if (statusText == null) return;
            statusText.text = newPointer == Actor.Player
                ? "Missed -- you're the Pointer now"
                : "Missed -- the bot is the Pointer now";
        }

        private void HandleGameWon(Actor winner)
        {
            if (statusText != null) statusText.text = string.Empty;
            if (winPanel != null) winPanel.SetActive(true);
            if (winText != null) winText.text = winner == Actor.Player ? "You win!" : "Bot wins!";

            // Keep the outcome around as a variable other scripts/scenes can read.
            ShadowBoxResult.Set(winner);
        }

        private void ContinueToNextScene()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("ShadowBoxUI: Continue pressed but Next Scene Name is empty.");
                return;
            }
            SceneManager.LoadScene(nextSceneName);
        }

        private void UpdateRoleLabels()
        {
            if (playerRoleText != null)
                playerRoleText.text = game.CurrentPointer == Actor.Player ? "Pointer\n(You)" : "Looker\n(You)";
            if (botRoleText != null)
                botRoleText.text = game.CurrentPointer == Actor.Bot ? "Pointer" : "Looker";
        }

        private void UpdatePips()
        {
            SetPips(playerStrikePips, game.PlayerStrikes);
            SetPips(botStrikePips, game.BotStrikes);
        }

        private void SetPips(Image[] pips, int strikesTaken)
        {
            if (pips == null) return;
            for (int i = 0; i < pips.Length; i++)
            {
                if (pips[i] == null) continue;
                pips[i].color = i < strikesTaken ? pipFilledColor : pipEmptyColor;
            }
        }

        private static string Arrow(PunchDirection dir)
        {
            switch (dir)
            {
                case PunchDirection.Up: return "\u2191";
                case PunchDirection.Down: return "\u2193";
                case PunchDirection.Left: return "\u2190";
                case PunchDirection.Right: return "\u2192";
                default: return "-";
            }
        }
    }
}
