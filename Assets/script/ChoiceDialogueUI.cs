using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrimeGame
{
    /// <summary>
    /// Reusable Yes/No prompt panel. Call Show(prompt, onYes, onNo) from any
    /// scene to pop it up -- no need to build a new dialogue UI per decision.
    /// </summary>
    public class ChoiceDialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action _onYes;
        private Action _onNo;

        private void Awake()
        {
            if (yesButton != null) yesButton.onClick.AddListener(() => Choose(true));
            if (noButton != null) noButton.onClick.AddListener(() => Choose(false));
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Show(string prompt, Action onYes, Action onNo)
        {
            if (promptText != null) promptText.text = prompt;
            _onYes = onYes;
            _onNo = onNo;
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void Choose(bool yes)
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            Action callback = yes ? _onYes : _onNo;
            _onYes = null;
            _onNo = null;
            callback?.Invoke();
        }
    }
}
