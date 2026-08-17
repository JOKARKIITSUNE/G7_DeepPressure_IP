using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CrimeGame
{
    /// <summary>
    /// Reusable NPC interact point. Player walks into range, an optional
    /// "Press E to talk" prompt appears, pressing E plays the configured
    /// dialogue lines through DialogueLineUI, and OnDialogueComplete fires
    /// once the lines are dismissed -- hook a CrimeDecisionController's
    /// TriggerDecision() to it for food court/HDB, or anything else for a
    /// plain conversation like the classroom tutorial.
    /// </summary>
    public class NPCInteract : MonoBehaviour
    {
        [Header("Speaker")]
        [SerializeField] private string npcName = "Friend";

        [TextArea]
        [Tooltip("Shown one at a time, in order, before OnDialogueComplete fires")]
        [SerializeField] private string[] dialogueLines;

        [Header("References")]
        [SerializeField] private DialogueLineUI dialogueUI;

        [Header("Proximity")]
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Optional on-screen prompt (e.g. 'Press E to talk') shown while in range")]
        [SerializeField] private GameObject interactPrompt;

        [Tooltip("Disable this interaction permanently after its dialogue finishes")]
        [SerializeField] private bool disableAfterDialogue;

        [Tooltip("Fires after the last dialogue line is dismissed")]
        public UnityEvent onDialogueComplete;

        private bool _playerInRange;
        private bool _dialogueActive;

        private void Update()
        {
            if (!_playerInRange || _dialogueActive || Keyboard.current == null) return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartDialogue();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = true;
            if (!_dialogueActive && interactPrompt != null) interactPrompt.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }

        private void StartDialogue()
        {
            _dialogueActive = true;
            if (interactPrompt != null) interactPrompt.SetActive(false);

            dialogueUI.ShowLines(npcName, dialogueLines, HandleDialogueComplete);
        }

        private void HandleDialogueComplete()
        {
            _dialogueActive = false;
            onDialogueComplete?.Invoke();

            if (!enabled || disableAfterDialogue)
            {
                if (interactPrompt != null) interactPrompt.SetActive(false);
                enabled = false;
                return;
            }

            if (_playerInRange && interactPrompt != null) interactPrompt.SetActive(true);
        }

        private void OnDisable()
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}
