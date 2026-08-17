using UnityEngine;
using UnityEngine.UI;

namespace CrimeGame
{
    /// <summary>
    /// Displays the full-screen PNG matching the crimes recorded this run.
    /// The images are loaded from Assets/Resources/Endings.
    /// </summary>
    public class PoliceEndingController : MonoBehaviour
    {
        private const string BadEndingPath = "Endings/BadEnding";
        private const string NeutralEndingPath = "Endings/NeutralEnding";
        private const string GoodEndingPath = "Endings/GoodEnding";

        public EndingType ResolvedEnding { get; private set; }

        public void ShowTrackedEnding()
        {
            ResolvedEnding = CrimeTracker.GetEnding();

            string imagePath = ResolvedEnding switch
            {
                EndingType.BothCrimes => BadEndingPath,
                EndingType.OneCrime => NeutralEndingPath,
                EndingType.NoCrimes => GoodEndingPath,
                _ => string.Empty
            };

            Texture2D endingTexture = Resources.Load<Texture2D>(imagePath);
            if (endingTexture == null)
            {
                Debug.LogError($"PoliceEndingController: Could not load Resources/{imagePath}.png");
                return;
            }

            CreateEndingCanvas(endingTexture);
        }

        private static void CreateEndingCanvas(Texture endingTexture)
        {
            GameObject canvasObject = new GameObject(
                "EndingCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1050f, 600f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject backgroundObject = new GameObject(
                "Background",
                typeof(RectTransform),
                typeof(Image));
            backgroundObject.transform.SetParent(canvasObject.transform, false);

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            StretchToParent(backgroundRect);
            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color32(32, 32, 32, 255);

            GameObject imageObject = new GameObject(
                "EndingImage",
                typeof(RectTransform),
                typeof(RawImage),
                typeof(AspectRatioFitter));
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            StretchToParent(imageRect);

            RawImage image = imageObject.GetComponent<RawImage>();
            image.texture = endingTexture;
            image.raycastTarget = true;

            AspectRatioFitter fitter = imageObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = (float)endingTexture.width / endingTexture.height;
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
