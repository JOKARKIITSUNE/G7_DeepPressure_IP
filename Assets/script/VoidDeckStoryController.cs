using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CrimeGame
{
    public class VoidDeckStoryController : MonoBehaviour
    {
        [Header("Opening decision")]
        [SerializeField] private ChoiceDialogueUI choiceDialogue;
        [SerializeField] private NPCInteract jaidenInteraction;
        [TextArea]
        [SerializeField] private string choiceText = "Wanna trash this void deck?";

        [Header("Player kick path")]
        [SerializeField] private DustbinKickInteract dustbinInteraction;
        [SerializeField] private Transform dustbin;
        [SerializeField] private Transform fallenDustbinPoint;
        [SerializeField] private string kickTask = "Kick the dustbin.";
        [Min(0.01f)]
        [SerializeField] private float kickMoveDuration = 0.2f;

        [Header("Next story step")]
        [Tooltip("Fires after the dustbin reaches its fallen position.")]
        public UnityEvent onPlayerKickFinished;

        public bool CanPlayerKick { get; private set; }

        private void Start()
        {
            if (dustbinInteraction != null)
                dustbinInteraction.SetKickAvailable(false);
        }

        public void BeginDecision()
        {
            if (choiceDialogue == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Choice Dialogue is not assigned.");
                return;
            }

            if (jaidenInteraction != null)
                jaidenInteraction.enabled = false;

            choiceDialogue.Show(choiceText, HandleYes, HandleNo);
        }

        private void HandleYes()
        {
            CanPlayerKick = true;

            if (dustbinInteraction != null)
                dustbinInteraction.SetKickAvailable(true);

            if (TaskPanelUI.Instance != null)
                TaskPanelUI.Instance.SetTask(kickTask);
        }

        private void HandleNo()
        {
            // The resistance-minigame path will be connected in the next step.
            // Re-enable Jaiden for now so testing No cannot leave the story stuck.
            if (jaidenInteraction != null)
                jaidenInteraction.enabled = true;
        }

        public void PlayerKickedDustbin()
        {
            if (!CanPlayerKick) return;

            CanPlayerKick = false;
            if (dustbinInteraction != null)
                dustbinInteraction.SetKickAvailable(false);

            if (TaskPanelUI.Instance != null)
                TaskPanelUI.Instance.ClearTask();

            CrimeTracker.MarkVandalized();
            StartCoroutine(MoveDustbinToFallenPoint());
        }

        private IEnumerator MoveDustbinToFallenPoint()
        {
            if (dustbin == null || fallenDustbinPoint == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Dustbin or Fallen Dustbin Point is not assigned.");
                onPlayerKickFinished?.Invoke();
                yield break;
            }

            Vector3 startPosition = dustbin.position;
            Quaternion startRotation = dustbin.rotation;
            float elapsed = 0f;

            while (elapsed < kickMoveDuration)
            {
                elapsed += Time.deltaTime;
                float amount = Mathf.Clamp01(elapsed / kickMoveDuration);
                dustbin.position = Vector3.Lerp(startPosition, fallenDustbinPoint.position, amount);
                dustbin.rotation = Quaternion.Slerp(startRotation, fallenDustbinPoint.rotation, amount);
                yield return null;
            }

            dustbin.SetPositionAndRotation(
                fallenDustbinPoint.position,
                fallenDustbinPoint.rotation);

            onPlayerKickFinished?.Invoke();
        }
    }
}
