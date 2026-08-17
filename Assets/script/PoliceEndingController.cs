using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private const string MainMenuScene = "start";

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

            CreateMainMenuButton(canvasObject.transform);
        }

        private static void CreateMainMenuButton(Transform parent)
        {
            GameObject buttonObject = new GameObject(
                "BackToMainMenuButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0f, 35f);
            buttonRect.sizeDelta = new Vector2(280f, 58f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = Color.white;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(ReturnToMainMenu);

            GameObject labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchToParent(labelRect);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "BACK TO MAIN MENU";
            label.fontSize = 25f;
            label.color = new Color32(32, 32, 32, 255);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        private static void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            CrimeTracker.Reset();
            SceneManager.LoadScene(MainMenuScene);
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
