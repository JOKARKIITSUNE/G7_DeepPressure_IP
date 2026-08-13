using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrimeGame
{
    /// <summary>
    /// In-game pause menu, separate from MainMenuUI. Only offers Continue and
    /// Back to Title -- drop this on an object in each gameplay scene (or make
    /// it persistent with DontDestroyOnLoad if you'd rather have one instance
    /// for the whole game).
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button backToTitleButton;

        [Tooltip("Exact name of the main menu scene (must be in Build Settings > Scenes In Build)")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Key that opens/closes the pause menu")]
        [SerializeField] private Key pauseKey = Key.Escape;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(Resume);
            if (backToTitleButton != null) backToTitleButton.onClick.AddListener(BackToTitle);
        }

        private void Start()
        {
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
            Time.timeScale = paused ? 0f : 1f;
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
