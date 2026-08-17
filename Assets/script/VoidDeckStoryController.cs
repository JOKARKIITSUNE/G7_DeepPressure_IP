using System.Collections;
using UnityEngine;

public class VoidDeckStoryController : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform jaiden;
    [SerializeField] private MonoBehaviour playerMovement;

    [Header("Cutscene Points")]
    [SerializeField] private Transform playerCutscenePosition;
    [SerializeField] private Transform jaidenKickPoint;
    [SerializeField] private Transform fallenDustbinPoint;

    [Header("Cameras")]
    [SerializeField] private GameObject kickCutsceneCamera;

    [Header("Scene Objects")]
    [SerializeField] private Transform dustbin;
    [SerializeField] private GameObject police;
    [SerializeField] private AudioSource policeArrivalAudio;

    [Header("UI")]
    [SerializeField] private CrimeGame.DialogueLineUI dialogueUI;

    [Header("Timing")]
    [SerializeField] private float moveDuration = 0.15f;

    private Vector3 jaidenOriginalPosition;
    private Quaternion jaidenOriginalRotation;
    private GameObject gameplayCamera;

    private void Awake()
    {
        Camera activeMainCamera = Camera.main;
        if (activeMainCamera != null)
            gameplayCamera = activeMainCamera.gameObject;

        if (kickCutsceneCamera != null)
            kickCutsceneCamera.SetActive(false);
    }

    public void PlayJaidenKickSequence()
    {
        FindPlayerMovement();

        if (playerMovement != null)
            playerMovement.enabled = false;

        TryTeleportPlayerToCutscenePosition();
        ShowKickCamera();

        jaidenOriginalPosition = jaiden.position;
        jaidenOriginalRotation = jaiden.rotation;

        dialogueUI.ShowLines(
            "Jaiden",
            new[] { "Fine, I'll do it myself." },
            () => StartCoroutine(JaidenKickRoutine()));
    }

    private IEnumerator JaidenKickRoutine()
    {
        yield return MoveJaiden(
            jaiden.position,
            jaiden.rotation,
            jaidenKickPoint.position,
            jaidenKickPoint.rotation);

        dustbin.SetPositionAndRotation(
            fallenDustbinPoint.position,
            fallenDustbinPoint.rotation);

        yield return MoveJaiden(
            jaiden.position,
            jaiden.rotation,
            jaidenOriginalPosition,
            jaidenOriginalRotation);

        if (police != null)
            police.SetActive(true);

        if (policeArrivalAudio != null)
            policeArrivalAudio.Play();

        if (CrimeGame.TaskPanelUI.Instance != null)
            CrimeGame.TaskPanelUI.Instance.SetTask("Talk to Police.");

        RestoreGameplayCamera();

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private IEnumerator MoveJaiden(
        Vector3 startPosition,
        Quaternion startRotation,
        Vector3 endPosition,
        Quaternion endRotation)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float amount = elapsed / moveDuration;

            jaiden.position = Vector3.Lerp(
                startPosition, endPosition, amount);

            jaiden.rotation = Quaternion.Slerp(
                startRotation, endRotation, amount);

            elapsed += Time.deltaTime;
            yield return null;
        }

        jaiden.SetPositionAndRotation(endPosition, endRotation);
    }

    private void FindPlayerMovement()
    {
        if (playerMovement != null || player == null)
            return;

        MonoBehaviour[] scripts =
            player.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name == "ThirdPersonController")
            {
                playerMovement = script;
                break;
            }
        }
    }

    private void TryTeleportPlayerToCutscenePosition()
    {
        if (player == null || playerCutscenePosition == null)
            return;

        Transform controlledPlayer =
            playerMovement != null ? playerMovement.transform : player;

        Vector3 rayOrigin = playerCutscenePosition.position + Vector3.up;

        if (!Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                3f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning(
                "PlayerCutscenePosition is not safely above the deck. " +
                "Move it onto the floor before testing again.",
                playerCutscenePosition);
            return;
        }

        Vector3 safePosition = playerCutscenePosition.position;
        safePosition.y = hit.point.y + 0.1f;

        Vector3 lookDirection = jaiden.position - safePosition;
        lookDirection.y = 0f;

        Quaternion uprightRotation = lookDirection.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(lookDirection)
            : Quaternion.Euler(0f, playerCutscenePosition.eulerAngles.y, 0f);

        CharacterController characterController =
            controlledPlayer.GetComponent<CharacterController>();

        if (characterController != null)
            characterController.enabled = false;

        controlledPlayer.SetPositionAndRotation(safePosition, uprightRotation);

        if (characterController != null)
            characterController.enabled = true;
    }

    private void ShowKickCamera()
    {
        if (gameplayCamera == null)
        {
            Camera activeMainCamera = Camera.main;
            if (activeMainCamera != null)
                gameplayCamera = activeMainCamera.gameObject;
        }

        if (gameplayCamera != null && gameplayCamera != kickCutsceneCamera)
            gameplayCamera.SetActive(false);

        if (kickCutsceneCamera != null)
            kickCutsceneCamera.SetActive(true);
    }

    private void RestoreGameplayCamera()
    {
        if (kickCutsceneCamera != null)
            kickCutsceneCamera.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.SetActive(true);
    }
}
