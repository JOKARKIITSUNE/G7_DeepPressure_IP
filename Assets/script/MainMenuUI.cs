using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrimeGame
{
    /// <summary>
    /// Drives the main menu screen: Start Game, Quit Game, Options, and Redeem Code.
    /// Options and Redeem Code are separate panels that swap in over the main
    /// buttons -- wire up the hierarchy per the setup steps in chat.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainButtonsPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject redeemPanel;

        [Header("Main buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
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

        [Header("Start game")]
        [Tooltip("Exact name of the scene to load when Start Game is pressed (must be in Build Settings > Scenes In Build)")]
        [SerializeField] private string firstSceneName = "Classroom";

        private const string VolumePrefKey = "MasterVolume";
        private const string FullscreenPrefKey = "Fullscreen";

        private void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
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
            ShowMain();

            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            bool savedFullscreen = PlayerPrefs.GetInt(FullscreenPrefKey, Screen.fullScreen ? 1 : 0) == 1;

            AudioListener.volume = savedVolume;
            Screen.fullScreen = savedFullscreen;

            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(savedVolume);
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

            if (redeemFeedbackText != null) redeemFeedbackText.text = string.Empty;
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
                    if (redeemFeedbackText != null) redeemFeedbackText.text = "Code accepted!";
                    return;
                }
            }

            if (redeemFeedbackText != null) redeemFeedbackText.text = "That code isn't valid.";
        }
    }
}
