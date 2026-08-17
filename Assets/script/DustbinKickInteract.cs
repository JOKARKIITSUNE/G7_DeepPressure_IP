using UnityEngine;
using UnityEngine.InputSystem;

namespace CrimeGame
{
    public class DustbinKickInteract : MonoBehaviour
    {
        [SerializeField] private VoidDeckStoryController storyController;
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private string playerTag = "Player";

        private bool playerInRange;
        private bool kickAvailable;
        private bool alreadyKicked;

        private void Start()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            bool canKick = kickAvailable && playerInRange && !alreadyKicked;
            if (interactPrompt != null)
                interactPrompt.SetActive(canKick);

            if (!canKick || Keyboard.current == null) return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                alreadyKicked = true;
                if (interactPrompt != null)
                    interactPrompt.SetActive(false);

                if (storyController != null)
                    storyController.PlayerKickedDustbin();
                else
                    Debug.LogWarning("DustbinKickInteract: Story Controller is not assigned.");
            }
        }

        public void SetKickAvailable(bool available)
        {
            kickAvailable = available;

            if (!available && interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
                playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            playerInRange = false;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}
