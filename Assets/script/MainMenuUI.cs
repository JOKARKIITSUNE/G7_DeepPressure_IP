using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrimeGame
{
    /// <summary>
    /// Drives the menu screen. Works as either:
    ///  - the title screen (Is Pause Menu = false): Start Game / Quit Game
    ///  - an in-game pause menu (Is Pause Menu = true): Continue / Back to Title
    /// Options and Redeem Code panels are identical in both modes. Drop this
    /// same script into your title scene and into your gameplay scene(s),
    /// just flip Is Pause Menu and wire the fields for that context.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Mode")]
        [Tooltip("False = title screen (Start/Quit). True = in-game pause menu (Continue/Back to Title).")]
        [SerializeField] private bool isPauseMenu;

        [Tooltip("Only used when Is Pause Menu is true. The whole menu overlay, hidden until Escape is pressed.")]
        [SerializeField] private GameObject menuRoot;

        [Header("Panels")]
        [SerializeField] private GameObject mainButtonsPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject redeemPanel;

        [Header("Main buttons")]
        [Tooltip("Start Game (title) or Continue (pause menu)")]
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;

        [Tooltip("Quit Game (title) or Back to Title (pause menu)")]
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        [SerializeField] private Button optionsButton;
        [SerializeField] private Button redeemButton;

        [Header("Options panel")]
        [SerializeField] private Button optionsBackButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Redeem code panel")]
        [SerializeField] private Button redeemBackButton;
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private Button submitCodeButton;
        [SerializeField] private TMP_Text redeemFeedbackText;

        [Tooltip("Codes are matched case-insensitively, whitespace trimmed")]
        [SerializeField] private string[] validCodes;

        [Header("Redeem reward")]
        [Tooltip("Shown on the title screen once the player has ever redeemed a valid code")]
        [SerializeField] private GameObject crownIcon;

        [Header("Scenes")]
        [Tooltip("Title mode: scene loaded by the primary button. Must be in Build Settings > Scenes In Build.")]
        [SerializeField] private string firstSceneName = "Classroom";

        [Tooltip("Pause mode: scene loaded by the secondary (Back to Title) button. Must be in Build Settings > Scenes In Build.")]
        [SerializeField] private string titleSceneName = "MainMenu";

        private const string VolumePrefKey = "MasterVolume";
        private const string FullscreenPrefKey = "Fullscreen";

        private bool _menuOpen;

        private void Awake()
        {
            if (primaryButton != null) primaryButton.onClick.AddListener(OnPrimaryPressed);
            if (secondaryButton != null) secondaryButton.onClick.AddListener(OnSecondaryPressed);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (redeemButton != null) redeemButton.onClick.AddListener(OpenRedeem);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(CloseToMain);
            if (redeemBackButton != null) redeemBackButton.onClick.AddListener(CloseToMain);
            if (submitCodeButton != null) submitCodeButton.onClick.AddListener(SubmitCode);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void Start()
        {
            if (primaryButtonLabel != null) primaryButtonLabel.text = isPauseMenu ? "Continue" : "Start Game";
            if (secondaryButtonLabel != null) secondaryButtonLabel.text = isPauseMenu ? "Back to Title" : "Quit Game";

            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            bool savedFullscreen = PlayerPrefs.GetInt(FullscreenPrefKey, Screen.fullScreen ? 1 : 0) == 1;
            AudioListener.volume = savedVolume;
            Screen.fullScreen = savedFullscreen;
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(savedVolume);
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
            if (redeemFeedbackText != null) redeemFeedbackText.text = string.Empty;

            RedeemCodeResult.Load();
            UpdateCrownVisibility();

            if (isPauseMenu)
            {
                _menuOpen = false;
                if (menuRoot != null) menuRoot.SetActive(false);
            }
            else
            {
                ShowMain();
            }
        }

        private void Update()
        {
            if (!isPauseMenu) return;
            if (Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_menuOpen) ResumeGame();
                else OpenPauseMenu();
            }
        }

        private void OpenPauseMenu()
        {
            _menuOpen = true;
            if (menuRoot != null) menuRoot.SetActive(true);
            ShowMain();
            Time.timeScale = 0f;
        }

        private void ResumeGame()
        {
            _menuOpen = false;
            Time.timeScale = 1f;
            if (menuRoot != null) menuRoot.SetActive(false);
        }

        private void OnPrimaryPressed()
        {
            if (isPauseMenu) ResumeGame();
            else StartGame();
        }

        private void OnSecondaryPressed()
        {
            if (isPauseMenu) BackToTitle();
            else QuitGame();
        }

        private void StartGame()
        {
            if (string.IsNullOrEmpty(firstSceneName))
            {
                Debug.LogWarning("MainMenuUI: First Scene Name is empty.");
                return;
            }
            SceneManager.LoadScene(firstSceneName);
        }

        private void BackToTitle()
        {
            if (string.IsNullOrEmpty(titleSceneName))
            {
                Debug.LogWarning("MainMenuUI: Title Scene Name is empty.");
                return;
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(titleSceneName);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenOptions()
        {
            SetPanels(main: false, options: true, redeem: false);
        }

        private void OpenRedeem()
        {
            if (redeemFeedbackText != null) redeemFeedbackText.text = string.Empty;
            if (codeInputField != null) codeInputField.text = string.Empty;
            SetPanels(main: false, options: false, redeem: true);
        }

        private void CloseToMain()
        {
            ShowMain();
        }

        private void ShowMain()
        {
            SetPanels(main: true, options: false, redeem: false);
        }

        private void SetPanels(bool main, bool options, bool redeem)
        {
            if (mainButtonsPanel != null) mainButtonsPanel.SetActive(main);
            if (optionsPanel != null) optionsPanel.SetActive(options);
            if (redeemPanel != null) redeemPanel.SetActive(redeem);
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumePrefKey, value);
        }

        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt(FullscreenPrefKey, value ? 1 : 0);
        }

        private void SubmitCode()
        {
            if (codeInputField == null) return;

            string entered = codeInputField.text.Trim();

            foreach (string code in validCodes)
            {
                if (string.Equals(entered, code.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    RedeemCodeResult.Set(entered);
                    UpdateCrownVisibility();
                    if (redeemFeedbackText != null) redeemFeedbackText.text = "Code accepted!";
                    return;
                }
            }

            if (redeemFeedbackText != null) redeemFeedbackText.text = "That code isn't valid.";
        }

        private void UpdateCrownVisibility()
        {
            if (crownIcon != null) crownIcon.SetActive(RedeemCodeResult.HasRedeemed);
        }
    }
}