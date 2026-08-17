using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CrimeGame
{
    public enum CrimeType { Theft, Vandalism }

    /// <summary>
    /// Drives a single crime decision point (food court theft, HDB vandalism,
    /// etc). Yes -> creates a pending crime task that is completed by interacting
    /// with the target object. No -> triggers the shadow box minigame on a harder
    /// difficulty as the "resistance" mechanic: win it and the crime is avoided,
    /// lose it and the crime is marked anyway.
    /// </summary>
    public class CrimeDecisionController : MonoBehaviour
    {
        [Header("Which crime this decision represents")]
        [SerializeField] private CrimeType crimeType;

        [TextArea]
        [SerializeField] private string promptText = "Should we do it?";

        [Header("Accepted crime task")]
        [TextArea]
        [SerializeField] private string acceptedTaskText = "Steal from the unattended bag.";

        public bool IsCrimeActionPending { get; private set; }

        [Header("References")]
        [SerializeField] private ChoiceDialogueUI choiceDialogue;
        [SerializeField] private MiniGames.ShadowBoxUI minigameUI;
        [SerializeField] private MiniGames.ShadowBoxGame minigameGame;

        [Header("Resistance minigame difficulty (used when the player says No)")]
        [Range(0f, 1f)]
        [SerializeField] private float hardModeBotSkill = 0.8f;
        [SerializeField] private int hardModeStrikesToLose = 3;

        [Tooltip("How long to leave the win/lose result on screen before continuing")]
        [SerializeField] private float resultDisplayDelay = 1.2f;

        [Header("Resistance win follow-up")]
        [SerializeField] private DialogueLineUI resistanceWinDialogueUI;
        [SerializeField] private NPCInteract initiatingNpcInteraction;
        [SerializeField] private string resistanceWinSpeaker = "Jaiden";
        [TextArea]
        [SerializeField] private string[] resistanceWinDialogueLines =
        {
            "Ok, nevermind, I'll catch up with you later."
        };
        [TextArea]
        [SerializeField] private string[] resistanceLossDialogueLines =
        {
            "Come on man, make up your mind!"
        };
        [SerializeField] private string resistanceWinTask = "Go home.";
        [SerializeField] private GameObject exitTriggerAfterResistanceWin;

        [Tooltip("Fires once the decision is fully resolved (crime marked or avoided), so the scene can continue dialogue/movement")]
        public UnityEvent onDecisionComplete;

        /// <summary>Call this from an NPC interact script to start the decision.</summary>
        public void TriggerDecision()
        {
            if (initiatingNpcInteraction != null) initiatingNpcInteraction.enabled = false;
            choiceDialogue.Show(promptText, onYes: HandleYes, onNo: HandleNo);
        }

        private void HandleYes()
        {
            IsCrimeActionPending = true;

            if (TaskPanelUI.Instance != null)
            {
                TaskPanelUI.Instance.SetTask(acceptedTaskText);
            }
        }

        public void CompleteCrimeAction()
        {
            if (!IsCrimeActionPending) return;

            IsCrimeActionPending = false;
            MarkCrime();

            if (TaskPanelUI.Instance != null)
            {
                TaskPanelUI.Instance.ClearTask();
            }

            onDecisionComplete?.Invoke();
        }

        private void HandleNo()
        {
            minigameGame.botSkill = hardModeBotSkill;
            minigameGame.strikesToLose = hardModeStrikesToLose;
            minigameGame.OnGameWon += HandleMinigameResult;
            minigameUI.ShowMinigame();
        }

        private void HandleMinigameResult(MiniGames.Actor winner)
        {
            minigameGame.OnGameWon -= HandleMinigameResult;
            StartCoroutine(ResolveAfterDelay(winner));
        }

        private IEnumerator ResolveAfterDelay(MiniGames.Actor winner)
        {
            yield return new WaitForSeconds(resultDisplayDelay);

            minigameUI.HideMinigame();

            // Player lost the resistance minigame (bot won) -> crime happens anyway.
            if (winner == MiniGames.Actor.Bot)
            {
                MarkCrime();
                ShowResistanceLossFollowUp();
                yield break;
            }

            ShowResistanceWinFollowUp();
        }

        private void ShowResistanceWinFollowUp()
        {
            if (initiatingNpcInteraction != null) initiatingNpcInteraction.enabled = false;

            if (resistanceWinDialogueUI != null)
            {
                resistanceWinDialogueUI.ShowLines(
                    resistanceWinSpeaker,
                    resistanceWinDialogueLines,
                    CompleteResistanceWinFollowUp);
                return;
            }

            CompleteResistanceWinFollowUp();
        }

        private void ShowResistanceLossFollowUp()
        {
            if (resistanceWinDialogueUI != null)
            {
                resistanceWinDialogueUI.ShowLines(
                    resistanceWinSpeaker,
                    resistanceLossDialogueLines,
                    CompleteResistanceLossFollowUp);
                return;
            }

            CompleteResistanceLossFollowUp();
        }

        private void CompleteResistanceLossFollowUp()
        {
            if (initiatingNpcInteraction != null) initiatingNpcInteraction.enabled = true;
            onDecisionComplete?.Invoke();
        }

        private void CompleteResistanceWinFollowUp()
        {
            if (TaskPanelUI.Instance != null)
            {
                TaskPanelUI.Instance.SetTask(resistanceWinTask);
            }

            if (exitTriggerAfterResistanceWin != null)
            {
                exitTriggerAfterResistanceWin.SetActive(true);
            }

            onDecisionComplete?.Invoke();
        }

        private void MarkCrime()
        {
            if (crimeType == CrimeType.Theft) CrimeTracker.MarkStole();
            else CrimeTracker.MarkVandalized();
        }
    }
}
