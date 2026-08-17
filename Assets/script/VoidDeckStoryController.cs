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

        [Header("Resistance minigame")]
        [SerializeField] private MiniGames.ShadowBoxUI minigameUI;
        [SerializeField] private MiniGames.ShadowBoxGame minigameGame;
        [Range(0f, 1f)]
        [SerializeField] private float resistanceBotSkill = 0.8f;
        [SerializeField] private int resistanceStrikesToLose = 3;
        [Min(0f)]
        [SerializeField] private float resistanceResultDelay = 2f;
        [SerializeField] private DialogueLineUI dialogueUI;
        [SerializeField] private string jaidenSpeaker = "Jaiden";
        [TextArea]
        [SerializeField] private string[] resistanceLossLines =
        {
            "Come on man, make up your mind!"
        };
        [TextArea]
        [SerializeField] private string[] resistanceWinLines =
        {
            "Fine, I'll do it myself."
        };
        [SerializeField] private string retryTask = "Talk to Jaiden.";

        [Header("Jaiden win cutscene")]
        [SerializeField] private Camera jaidenCutsceneCamera;
        [SerializeField] private Transform playerCharacter;
        [SerializeField] private Transform playerCutscenePoint;
        [SerializeField] private Transform jaiden;
        [SerializeField] private Transform jaidenKickPoint;
        [Min(0.01f)]
        [SerializeField] private float jaidenMoveDuration = 0.15f;

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

        [Header("Police arrival")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Camera policeSpawnCamera;
        [SerializeField] private Transform[] policeOfficers = new Transform[3];
        [SerializeField] private Transform[] policeSpawnPoints = new Transform[3];
        [Min(0f)]
        [SerializeField] private float pauseBeforePoliceCamera = 2f;
        [Min(0.01f)]
        [SerializeField] private float policeRiseDuration = 0.2f;
        [Min(0f)]
        [SerializeField] private float policeStartDepth = 3f;
        [Min(0f)]
        [SerializeField] private float pauseAfterPoliceArrival = 2f;
        [SerializeField] private AudioSource policeArrivalAudio;
        [SerializeField] private NPCInteract policeInteraction;
        [SerializeField] private string policeTask = "Talk to Police.";

        [Header("Player control")]
        [Tooltip("Optional. Assign the player's Third Person Controller component.")]
        [SerializeField] private MonoBehaviour playerMovement;

        public bool CanPlayerKick { get; private set; }

        private void Start()
        {
            if (dustbinInteraction != null)
                dustbinInteraction.SetKickAvailable(false);

            if (gameplayCamera == null)
                gameplayCamera = Camera.main;

            if (policeSpawnCamera != null)
                policeSpawnCamera.gameObject.SetActive(false);

            if (jaidenCutsceneCamera != null)
                jaidenCutsceneCamera.gameObject.SetActive(false);

            if (policeInteraction != null)
                policeInteraction.enabled = false;

            foreach (Transform officer in policeOfficers)
            {
                if (officer != null)
                    officer.gameObject.SetActive(false);
            }

            FindPlayerMovementIfNeeded();
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
            if (minigameUI == null || minigameGame == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Minigame UI or Game is not assigned.");
                CompleteResistanceLoss();
                return;
            }

            if (TaskPanelUI.Instance != null)
                TaskPanelUI.Instance.ClearTask();

            SetPlayerMovementEnabled(false);
            minigameGame.botSkill = resistanceBotSkill;
            minigameGame.strikesToLose = resistanceStrikesToLose;
            minigameGame.OnGameWon -= HandleResistanceResult;
            minigameGame.OnGameWon += HandleResistanceResult;
            minigameUI.ShowMinigame();
        }

        private void HandleResistanceResult(MiniGames.Actor winner)
        {
            minigameGame.OnGameWon -= HandleResistanceResult;
            StartCoroutine(ResolveResistanceResult(winner));
        }

        private IEnumerator ResolveResistanceResult(MiniGames.Actor winner)
        {
            yield return new WaitForSeconds(resistanceResultDelay);

            if (minigameUI != null)
                minigameUI.HideMinigame();

            if (winner == MiniGames.Actor.Bot)
            {
                ShowResistanceLossDialogue();
                yield break;
            }

            BeginJaidenWinCutscene();
        }

        private void ShowResistanceLossDialogue()
        {
            if (dialogueUI != null)
            {
                dialogueUI.ShowLines(
                    jaidenSpeaker,
                    resistanceLossLines,
                    CompleteResistanceLoss);
                return;
            }

            CompleteResistanceLoss();
        }

        private void CompleteResistanceLoss()
        {
            if (TaskPanelUI.Instance != null)
                TaskPanelUI.Instance.SetTask(retryTask);

            if (jaidenInteraction != null)
                jaidenInteraction.enabled = true;

            SetPlayerMovementEnabled(true);
        }

        private void BeginJaidenWinCutscene()
        {
            TeleportPlayerToCutscenePoint();
            SwitchToJaidenCamera();

            if (dialogueUI != null)
            {
                dialogueUI.ShowLines(
                    jaidenSpeaker,
                    resistanceWinLines,
                    StartJaidenKick);
                return;
            }

            StartJaidenKick();
        }

        private void StartJaidenKick()
        {
            StartCoroutine(JaidenKickSequence());
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
            SetPlayerMovementEnabled(false);
            StartCoroutine(PlayerKickSequence());
        }

        private IEnumerator PlayerKickSequence()
        {
            yield return MoveDustbinToFallenPoint();
            onPlayerKickFinished?.Invoke();
            yield return PoliceArrivalSequence();
        }

        private IEnumerator JaidenKickSequence()
        {
            if (jaiden == null || jaidenKickPoint == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Jaiden or Jaiden Kick Point is not assigned.");
                yield return MoveDustbinToFallenPoint();
                yield return PoliceArrivalSequence();
                yield break;
            }

            Vector3 originalPosition = jaiden.position;
            Quaternion originalRotation = jaiden.rotation;
            Vector3 directionToKickPoint = jaidenKickPoint.position - jaiden.position;
            directionToKickPoint.y = 0f;

            if (directionToKickPoint.sqrMagnitude > 0.001f)
                jaiden.rotation = Quaternion.LookRotation(directionToKickPoint.normalized);

            yield return MoveCharacter(
                jaiden,
                originalPosition,
                jaidenKickPoint.position,
                jaidenMoveDuration);

            yield return MoveDustbinToFallenPoint();

            yield return MoveCharacter(
                jaiden,
                jaidenKickPoint.position,
                originalPosition,
                jaidenMoveDuration);

            jaiden.SetPositionAndRotation(originalPosition, originalRotation);
            yield return PoliceArrivalSequence();
        }

        private static IEnumerator MoveCharacter(
            Transform character,
            Vector3 start,
            Vector3 end,
            float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                character.position = Vector3.Lerp(
                    start,
                    end,
                    Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            character.position = end;
        }

        private IEnumerator MoveDustbinToFallenPoint()
        {
            if (dustbin == null || fallenDustbinPoint == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Dustbin or Fallen Dustbin Point is not assigned.");
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
        }

        private void TeleportPlayerToCutscenePoint()
        {
            if (playerCharacter == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                    playerCharacter = playerObject.transform;
            }

            if (playerCharacter == null || playerCutscenePoint == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Player Character or Player Cutscene Point is not assigned.");
                return;
            }

            CharacterController characterController =
                playerCharacter.GetComponent<CharacterController>();
            bool controllerWasEnabled =
                characterController != null && characterController.enabled;

            if (controllerWasEnabled)
                characterController.enabled = false;

            playerCharacter.position = playerCutscenePoint.position;

            if (jaiden != null)
            {
                Vector3 directionToJaiden = jaiden.position - playerCharacter.position;
                directionToJaiden.y = 0f;
                if (directionToJaiden.sqrMagnitude > 0.001f)
                {
                    playerCharacter.rotation =
                        Quaternion.LookRotation(directionToJaiden.normalized);
                }
            }
            else
            {
                playerCharacter.rotation = playerCutscenePoint.rotation;
            }

            if (controllerWasEnabled)
                characterController.enabled = true;
        }

        private void SwitchToJaidenCamera()
        {
            if (gameplayCamera != null)
                gameplayCamera.enabled = false;

            if (policeSpawnCamera != null)
            {
                policeSpawnCamera.enabled = false;
                policeSpawnCamera.gameObject.SetActive(false);
            }

            if (jaidenCutsceneCamera != null)
            {
                jaidenCutsceneCamera.gameObject.SetActive(true);
                jaidenCutsceneCamera.enabled = true;
            }
            else
            {
                Debug.LogWarning("VoidDeckStoryController: Jaiden Cutscene Camera is not assigned.");
            }
        }

        private IEnumerator PoliceArrivalSequence()
        {
            yield return new WaitForSeconds(pauseBeforePoliceCamera);

            SwitchToPoliceCamera();

            Vector3[] finalPositions = new Vector3[policeOfficers.Length];
            Quaternion[] finalRotations = new Quaternion[policeOfficers.Length];
            for (int i = 0; i < policeOfficers.Length; i++)
            {
                Transform officer = policeOfficers[i];
                if (officer == null) continue;

                Transform spawnPoint =
                    policeSpawnPoints != null && i < policeSpawnPoints.Length
                        ? policeSpawnPoints[i]
                        : null;

                finalPositions[i] =
                    spawnPoint != null ? spawnPoint.position : officer.position;
                finalRotations[i] =
                    spawnPoint != null ? spawnPoint.rotation : officer.rotation;

                officer.position = finalPositions[i] - Vector3.up * policeStartDepth;
                officer.rotation = finalRotations[i];
                officer.gameObject.SetActive(true);
            }

            if (policeArrivalAudio != null)
                policeArrivalAudio.Play();

            float elapsed = 0f;
            while (elapsed < policeRiseDuration)
            {
                elapsed += Time.deltaTime;
                float amount = Mathf.Clamp01(elapsed / policeRiseDuration);

                for (int i = 0; i < policeOfficers.Length; i++)
                {
                    Transform officer = policeOfficers[i];
                    if (officer == null) continue;

                    Vector3 undergroundPosition =
                        finalPositions[i] - Vector3.up * policeStartDepth;
                    officer.position = Vector3.Lerp(
                        undergroundPosition,
                        finalPositions[i],
                        amount);
                }

                yield return null;
            }

            for (int i = 0; i < policeOfficers.Length; i++)
            {
                if (policeOfficers[i] != null)
                {
                    policeOfficers[i].position = finalPositions[i];
                    policeOfficers[i].rotation = finalRotations[i];
                }
            }

            yield return new WaitForSeconds(pauseAfterPoliceArrival);

            RestoreGameplayCamera();

            if (policeInteraction != null)
                policeInteraction.enabled = true;

            if (TaskPanelUI.Instance != null)
                TaskPanelUI.Instance.SetTask(policeTask);

            SetPlayerMovementEnabled(true);
        }

        private void SwitchToPoliceCamera()
        {
            if (gameplayCamera != null)
                gameplayCamera.enabled = false;

            if (jaidenCutsceneCamera != null)
            {
                jaidenCutsceneCamera.enabled = false;
                jaidenCutsceneCamera.gameObject.SetActive(false);
            }

            if (policeSpawnCamera != null)
            {
                policeSpawnCamera.gameObject.SetActive(true);
                policeSpawnCamera.enabled = true;
            }
            else
            {
                Debug.LogWarning("VoidDeckStoryController: Police Spawn Camera is not assigned.");
            }
        }

        private void RestoreGameplayCamera()
        {
            if (jaidenCutsceneCamera != null)
            {
                jaidenCutsceneCamera.enabled = false;
                jaidenCutsceneCamera.gameObject.SetActive(false);
            }

            if (policeSpawnCamera != null)
            {
                policeSpawnCamera.enabled = false;
                policeSpawnCamera.gameObject.SetActive(false);
            }

            if (gameplayCamera != null)
                gameplayCamera.enabled = true;
        }

        private void FindPlayerMovementIfNeeded()
        {
            if (playerMovement != null) return;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;

            MonoBehaviour[] scripts =
                playerObject.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour script in scripts)
            {
                string typeName = script.GetType().Name;
                if (typeName == "ThirdPersonController" ||
                    typeName == "FirstPersonController" ||
                    typeName == "movement")
                {
                    playerMovement = script;
                    return;
                }
            }
        }

        private void SetPlayerMovementEnabled(bool movementEnabled)
        {
            FindPlayerMovementIfNeeded();
            if (playerMovement != null)
                playerMovement.enabled = movementEnabled;
        }
    }
}
