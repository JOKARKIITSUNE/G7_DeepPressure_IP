using TMPro;
using UnityEngine;

namespace CrimeGame
{
    /// <summary>
    /// Drop this into the police confrontation scene. On Start, it reads
    /// CrimeTracker.GetEnding() and shows the matching outcome text. Wire the
    /// "no crimes" outcome to continue into your QR code / quiz scene.
    /// </summary>
    public class PoliceEndingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TMP_Text endingText;

        [Header("Outcome text")]
        [TextArea(3, 6)]
        [SerializeField]
        private string bothCrimesText =
            "The police corner your friend outside the flat. He doesn't hesitate -- " +
            "\"It was all him, officer. My hands are clean.\" They believe him. " +
            "You're the one who gets taken in.";

        [TextArea(3, 6)]
        [SerializeField]
        private string oneCrimeText =
            "The officers already have a report with both your names on it. " +
            "There's no talking your way out of this one -- you're both arrested.";

        [TextArea(3, 6)]
        [SerializeField]
        private string noCrimesText =
            "The police only want your friend. You watch as they're led away, " +
            "and for once, you're not part of the story. Your hands are clean -- for real.";

        [Header("Continue to reward (only for the no-crimes ending)")]
        [SerializeField] private GameObject continueToRewardButton;

        public EndingType ResolvedEnding { get; private set; }

        private void Start()
        {
            ResolvedEnding = CrimeTracker.GetEnding();
            ShowEnding(ResolvedEnding);
        }

        private void ShowEnding(EndingType ending)
        {
            if (endingPanel != null) endingPanel.SetActive(true);

            string text = ending switch
            {
                EndingType.BothCrimes => bothCrimesText,
                EndingType.OneCrime => oneCrimeText,
                EndingType.NoCrimes => noCrimesText,
                _ => string.Empty
            };

            if (endingText != null) endingText.text = text;

            // Only the "did nothing" ending leads to the QR code / quiz reward.
            if (continueToRewardButton != null)
            {
                continueToRewardButton.SetActive(ending == EndingType.NoCrimes);
            }
        }
    }
}
