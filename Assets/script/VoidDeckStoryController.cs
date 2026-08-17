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
            SetPlayerMovementEnabled(false);
            StartCoroutine(MoveDustbinToFallenPoint());
        }

        private IEnumerator MoveDustbinToFallenPoint()
        {
            if (dustbin == null || fallenDustbinPoint == null)
            {
                Debug.LogWarning("VoidDeckStoryController: Dustbin or Fallen Dustbin Point is not assigned.");
                FinishDustbinKick();
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

            FinishDustbinKick();
        }

        private void FinishDustbinKick()
        {
            onPlayerKickFinished?.Invoke();
            StartCoroutine(PoliceArrivalSequence());
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
