using UnityEngine;

namespace CrimeGame
{
    /// <summary>
    /// Drop this into any scene to set the task panel's starting objective the
    /// moment the scene loads, without needing an NPC interaction first.
    /// </summary>
    public class SceneObjective : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string startingTask = "head downstairs for lunch";

        private void Start()
        {
            if (TaskPanelUI.Instance != null)
            {
                TaskPanelUI.Instance.SetTask(startingTask);
            }
        }
    }
}
