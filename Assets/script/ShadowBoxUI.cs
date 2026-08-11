using UnityEngine;
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
        [SerializeField] private Text playerRoleText;
        [SerializeField] private Text botRoleText;
        [SerializeField] private Text roundNumberText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text revealText;

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

        [Header("Win panel")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Text winText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (upButton != null) upButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Up));
            if (downButton != null) downButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Down));
            if (leftButton != null) leftButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Left));
            if (rightButton != null) rightButton.onClick.AddListener(() => game.CaptureInput(PunchDirection.Right));
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
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
            if (statusText != null) statusText.text = "your move";
            UpdateRoleLabels();
            UpdatePips();
            if (roundNumberText != null) roundNumberText.text = "round " + game.RoundNumber;
        }

        private void HandleRoundResolved(PunchDirection playerDir, PunchDirection botDir, bool wasHit)
        {
            if (revealText != null)
            {
                revealText.text = "you: " + Arrow(playerDir) + "    bot: " + Arrow(botDir);
            }
            UpdateRoleLabels();
            UpdatePips();
            if (roundNumberText != null) roundNumberText.text = "round " + game.RoundNumber;
        }

        private void HandleStrikeTaken(Actor struckActor)
        {
            if (statusText == null) return;
            statusText.text = struckActor == Actor.Player
                ? "the bot caught you -- strike against you"
                : "you caught the bot -- strike landed";
        }

        private void HandleRoleFlipped(Actor newPointer)
        {
            if (statusText == null) return;
            statusText.text = newPointer == Actor.Player
                ? "missed -- you're the pointer now"
                : "missed -- bot is the pointer now";
        }

        private void HandleGameWon(Actor winner)
        {
            if (statusText != null) statusText.text = string.Empty;
            if (winPanel != null) winPanel.SetActive(true);
            if (winText != null) winText.text = winner == Actor.Player ? "you win!" : "bot wins";
        }

        private void UpdateRoleLabels()
        {
            if (playerRoleText != null)
                playerRoleText.text = game.CurrentPointer == Actor.Player ? "pointer" : "looker";
            if (botRoleText != null)
                botRoleText.text = game.CurrentPointer == Actor.Bot ? "pointer" : "looker";
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
