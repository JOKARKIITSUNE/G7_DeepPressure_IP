using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrimeGame
{
    /// <summary>
    /// Reusable linear dialogue box: shows a speaker name and a sequence of
    /// lines, one at a time, advanced by pressing E or clicking Continue.
    /// Any NPC can reuse this instead of building its own dialogue UI.
    /// </summary>
    public class DialogueLineUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private Button continueButton;

        [Tooltip("Key that advances to the next line")]
        [SerializeField] private Key advanceKey = Key.E;

        private string[] _lines;
        private int _index;
        private Action _onComplete;
        private bool _active;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(Advance);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (!_active || Keyboard.current == null) return;
            if (Keyboard.current[advanceKey].wasPressedThisFrame) Advance();
        }

        /// <summary>Starts showing the given lines. Calls onComplete once the last line is dismissed.</summary>
        public void ShowLines(string speaker, string[] lines, Action onComplete)
        {
            if (lines == null || lines.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _lines = lines;
            _index = 0;
            _onComplete = onComplete;
            _active = true;

            if (nameText != null) nameText.text = speaker;
            if (panelRoot != null) panelRoot.SetActive(true);
            ShowCurrentLine();
        }

        public void Advance()
        {
            if (!_active) return;

            _index++;
            if (_index >= _lines.Length) Close();
            else ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            if (lineText != null) lineText.text = _lines[_index];
        }

        private void Close()
        {
            _active = false;
            if (panelRoot != null) panelRoot.SetActive(false);

            Action callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }
    }
}
