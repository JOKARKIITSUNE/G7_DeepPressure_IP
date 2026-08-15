using TMPro;
using UnityEngine;

namespace CrimeGame
{
    /// <summary>
    /// Simple task/objective HUD panel (e.g. "Head downstairs for lunch").
    /// Persists across scene loads as a singleton, so any script anywhere can
    /// call TaskPanelUI.Instance.SetTask("...") without needing a scene
    /// reference. Only needs to exist once -- add it to your first gameplay
    /// scene (e.g. Classroom); it will carry itself into every scene after that.
    /// </summary>
    public class TaskPanelUI : MonoBehaviour
    {
        public static TaskPanelUI Instance { get; private set; }

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text taskText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        /// <summary>Shows the panel with the given task text.</summary>
        public void SetTask(string text)
        {
            if (taskText != null) taskText.text = text;
            if (panelRoot != null) panelRoot.SetActive(!string.IsNullOrEmpty(text));
        }

        /// <summary>Hides the panel.</summary>
        public void ClearTask()
        {
            SetTask(string.Empty);
        }
    }
}
