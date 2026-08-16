using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrimeGame
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea]
        public string questionText;
        public string[] options = new string[4];
        public int correctOptionIndex;
    }

    /// <summary>
    /// Reward flow for the "did neither crime" ending: shows a QR code image,
    /// then a short multiple-choice quiz, then reveals a reward code the
    /// player can later type into MainMenuUI's Redeem Code panel.
    /// </summary>
    public class QuizController : MonoBehaviour
    {
        [Header("Questions")]
        [SerializeField] private List<QuizQuestion> questions;

        [Header("QR panel")]
        [SerializeField] private GameObject qrPanel;
        [SerializeField] private Image qrCodeImage;
        [SerializeField] private Button startQuizButton;

        [Header("Quiz panel")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text feedbackText;
        [Tooltip("Exactly 4 buttons, each with a TMP_Text child")]
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TMP_Text[] optionLabels;

        [Header("Results panel")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text rewardCodeText;
        [SerializeField] private string rewardCode = "SAFE2024";

        [SerializeField] private float feedbackDelay = 1.0f;

        private int _index;
        private int _score;
        private bool _answering;

        private void Awake()
        {
            if (startQuizButton != null) startQuizButton.onClick.AddListener(StartQuiz);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int captured = i; // avoid closure-over-loop-variable bug
                optionButtons[i].onClick.AddListener(() => SelectAnswer(captured));
            }
        }

        private void Start()
        {
            if (qrPanel != null) qrPanel.SetActive(true);
            if (quizPanel != null) quizPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }

        private void StartQuiz()
        {
            if (qrPanel != null) qrPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(true);

            _index = 0;
            _score = 0;
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (questions == null || questions.Count == 0) { ShowResults(); return; }

            QuizQuestion q = questions[_index];
            if (questionText != null) questionText.text = q.questionText;
            if (progressText != null) progressText.text = "question " + (_index + 1) + "/" + questions.Count;
            if (feedbackText != null) feedbackText.text = string.Empty;

            for (int i = 0; i < optionLabels.Length; i++)
            {
                if (optionLabels[i] != null)
                    optionLabels[i].text = i < q.options.Length ? q.options[i] : string.Empty;
            }

            _answering = true;
        }

        private void SelectAnswer(int optionIndex)
        {
            if (!_answering) return;
            _answering = false;

            bool correct = optionIndex == questions[_index].correctOptionIndex;
            if (correct) _score++;

            if (feedbackText != null)
                feedbackText.text = correct ? "Correct!" : "Not quite.";

            StartCoroutine(NextAfterDelay());
        }

        private IEnumerator NextAfterDelay()
        {
            yield return new WaitForSeconds(feedbackDelay);

            _index++;
            if (_index >= questions.Count) ShowResults();
            else ShowQuestion();
        }

        private void ShowResults()
        {
            if (quizPanel != null) quizPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(true);

            if (scoreText != null) scoreText.text = "You scored " + _score + "/" + questions.Count;
            if (rewardCodeText != null) rewardCodeText.text = rewardCode;
        }
    }
}
