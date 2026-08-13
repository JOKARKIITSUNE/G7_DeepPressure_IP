using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrimeGame
{
    /// <summary>
    /// In-game pause menu, separate from MainMenuUI. Offers Continue, Options,
    /// and Back to Title. Continue/Options/Back to Title sit directly on the
    /// pause panel; Options is the only separate sub-panel, shown on top when
    /// pressed. Pausing freezes gameplay via Time.timeScale.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("The overall pause overlay -- shown/hidden when the pause key is pressed. Continue/Options/Back to Title buttons live directly on this.")]
        [SerializeField] private GameObject pausePanel;
        [Tooltip("Sub-panel with the volume slider and fullscreen toggle -- shown on top of pausePanel")]
        [SerializeField] private GameObject optionsPanel;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button backToTitleButton;

        [Header("Options panel")]
        [SerializeField] private Button optionsBackButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        [Tooltip("Exact name of the main menu scene (must be in Build Settings > Scenes In Build)")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Key that opens/closes the pause menu")]
        [SerializeField] private Key pauseKey = Key.Escape;

        private const string VolumePrefKey = "MasterVolume";
        private const string FullscreenPrefKey = "Fullscreen";

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(Resume);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (backToTitleButton != null) backToTitleButton.onClick.AddListener(BackToTitle);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(CloseOptions);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void Start()
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            bool savedFullscreen = PlayerPrefs.GetInt(FullscreenPrefKey, Screen.fullScreen ? 1 : 0) == 1;
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(savedVolume);
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

            SetPaused(false);
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current[pauseKey].wasPressedThisFrame)
            {
                if (IsPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            SetPaused(true);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (pausePanel != null) pausePanel.SetActive(paused);
            CloseOptions();
            Time.timeScale = paused ? 0f : 1f;
        }

        private void OpenOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void CloseOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
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

        private void BackToTitle()
        {
            // Always unpause time before leaving, otherwise the next scene loads frozen.
            Time.timeScale = 1f;

            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogWarning("PauseMenuUI: Main Menu Scene Name is empty.");
                return;
            }
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}