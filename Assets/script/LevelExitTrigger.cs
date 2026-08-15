using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CrimeGame
{
    /// <summary>
    /// Place on a trigger collider at the edge of an area. While the player is
    /// inside it, pressing E loads the next scene. Optionally shows an
    /// "Press E to leave" prompt while in range.
    /// </summary>
    public class LevelExitTrigger : MonoBehaviour
    {
        [Tooltip("Exact name of the scene to load (must be in Build Settings > Scenes In Build)")]
        [SerializeField] private string nextSceneName;

        [SerializeField] private string playerTag = "Player";

        [Tooltip("Optional on-screen prompt (e.g. 'Press E to leave') shown while the player is in range")]
        [SerializeField] private GameObject interactPrompt;

        private bool _playerInRange;

        private void Start()
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }

        private void Update()
        {
            if (!_playerInRange || Keyboard.current == null) return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Leave();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }

        private void Leave()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("LevelExitTrigger: Next Scene Name is empty.");
                return;
            }
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
