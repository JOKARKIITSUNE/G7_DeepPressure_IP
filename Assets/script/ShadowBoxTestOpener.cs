using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGames
{
    /// <summary>
    /// TEMPORARY test script. Press E anywhere in the scene to open the shadow
    /// box minigame screen, press E again to close it. No player, tag, or
    /// trigger collider needed -- just drag this onto any empty GameObject.
    /// </summary>
    public class ShadowBoxTestOpener : MonoBehaviour
    {
        [SerializeField] private ShadowBoxUI shadowBoxUI;

        private bool _minigameOpen;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;

            if (_minigameOpen)
            {
                shadowBoxUI.HideMinigame();
            }
            else
            {
                shadowBoxUI.ShowMinigame();
            }
            _minigameOpen = !_minigameOpen;
        }
    }
}
