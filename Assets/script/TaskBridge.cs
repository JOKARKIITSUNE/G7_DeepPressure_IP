using UnityEngine;

namespace CrimeGame
{
    /// <summary>
    /// TaskPanelUI only exists as a persistent object created in your first
    /// scene (via DontDestroyOnLoad), so it can't be dragged into a UnityEvent
    /// in any *other* scene's Inspector -- it simply isn't in that scene's
    /// Hierarchy at edit time. Add this small bridge to each later scene
    /// instead; it has no state of its own and just forwards to whichever
    /// TaskPanelUI is currently alive.
    /// </summary>
    public class TaskBridge : MonoBehaviour
    {
        public void SetTask(string text)
        {
            if (TaskPanelUI.Instance != null) TaskPanelUI.Instance.SetTask(text);
        }

        public void ClearTask()
        {
            if (TaskPanelUI.Instance != null) TaskPanelUI.Instance.ClearTask();
        }
    }
}
