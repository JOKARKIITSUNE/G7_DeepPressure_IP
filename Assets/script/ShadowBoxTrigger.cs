using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGames
{
    /// <summary>
    /// Opens the shadow box minigame screen when the player presses E.
    /// Optionally requires the player to be inside a trigger collider first
    /// (set RequireProximity to true and add a Collider with "Is Trigger" checked
    /// on this object) -- useful once you move this out of a test scene and into
    /// a real level where the minigame should only start near an NPC.
    /// </summary>
    public class ShadowBoxTrigger : MonoBehaviour
    {
        [SerializeField] private ShadowBoxUI shadowBoxUI;

        [Tooltip("If true, E only works while the player is inside this object's trigger collider. If false, E works any time (good for quick testing).")]
        [SerializeField] private bool requireProximity = false;

        [Tooltip("Tag used to detect the player when Require Proximity is on")]
        [SerializeField] private string playerTag = "Player";

        private bool _playerInRange;
        private bool _minigameOpen;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;

            if (requireProximity && !_playerInRange) return;

            if (_minigameOpen)
            {
                shadowBoxUI.HideMinigame();
                _minigameOpen = false;
            }
            else
            {
                shadowBoxUI.ShowMinigame();
                _minigameOpen = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag)) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag)) _playerInRange = false;
        }
    }
}
