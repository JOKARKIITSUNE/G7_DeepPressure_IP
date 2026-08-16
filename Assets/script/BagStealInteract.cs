using UnityEngine;
using UnityEngine.InputSystem;

namespace CrimeGame
{
    /// <summary>
    /// Lets the player complete an accepted theft by walking into the bag's
    /// trigger and pressing E. The bag cannot be stolen before the player has
    /// chosen Yes at the linked CrimeDecisionController.
    /// </summary>
    public class BagStealInteract : MonoBehaviour
    {
        [SerializeField] private CrimeDecisionController theftDecision;
        [SerializeField] private string playerTag = "Player";

        [Tooltip("Optional UI object that says something like 'Press E to steal'")]
        [SerializeField] private GameObject interactPrompt;

        [Tooltip("Optional bag model to hide after it has been stolen")]
        [SerializeField] private GameObject bagVisual;

        private bool _playerInRange;
        private bool _stolen;

        private void Start()
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }

        private void Update()
        {
            bool canSteal =
                !_stolen &&
                _playerInRange &&
                theftDecision != null &&
                theftDecision.IsCrimeActionPending;

            if (interactPrompt != null) interactPrompt.SetActive(canSteal);

            if (!canSteal || Keyboard.current == null) return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                _stolen = true;
                if (interactPrompt != null) interactPrompt.SetActive(false);
                theftDecision.CompleteCrimeAction();
                if (bagVisual != null) bagVisual.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag)) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}
