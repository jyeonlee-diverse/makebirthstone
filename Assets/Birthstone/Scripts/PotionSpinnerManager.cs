using UnityEngine;
using UnityEngine.UI;

namespace Birthstone
{
    /// <summary>
    /// Potion clicker scene.
    /// Spawns a Potion2 prefab and lets the user flick-spin it.
    /// </summary>
    public class PotionSpinnerManager : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField] GameObject potionPrefab;

        [Header("Potion")]
        [SerializeField] float potionScale = 200f;
        [SerializeField] Vector3 potionPosition = new Vector3(0, -0.5f, 0);

        // UI
        Canvas canvas;
        Text spinCountText;
        Text speedText;
        Text titleText;

        GameObject currentPotion;
        GemSpinner spinner;

        void Start()
        {
            SetupCamera();
            SetupLighting();
            CreateUI();
            SpawnPotion();
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.03f, 0.06f);
            cam.transform.position = new Vector3(0, 0.5f, -3.5f);
            cam.transform.LookAt(new Vector3(0, 0, 0));
        }

        void SetupLighting()
        {
            if (FindAnyObjectByType<Light>() == null)
                CreateLight("MainLight", Quaternion.Euler(40, -30, 0), new Color(1f, 0.95f, 0.9f), 1.5f);

            CreateLight("FillLight", Quaternion.Euler(20, 150, 0), new Color(0.5f, 0.6f, 1f), 0.6f);
            CreateLight("RimLight", Quaternion.Euler(-10, 90, 0), new Color(1f, 0.85f, 0.7f), 0.4f);
        }

        void CreateLight(string name, Quaternion rotation, Color color, float intensity)
        {
            var obj = new GameObject(name);
            obj.transform.rotation = rotation;
            var light = obj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
        }

        void CreateUI()
        {
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Title
            titleText = CreateText(canvas.transform, "TitleText",
                "Potion Clicker",
                new Vector2(0, 400), new Vector2(800, 100), 52,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.8f, 0.6f, 1f));

            // Spin count
            spinCountText = CreateText(canvas.transform, "SpinCountText",
                "0", new Vector2(0, -350), new Vector2(600, 100), 72,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            // Spin label
            CreateText(canvas.transform, "SpinLabel",
                "SPINS", new Vector2(0, -420), new Vector2(400, 50), 28,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.7f));

            // Speed
            speedText = CreateText(canvas.transform, "SpeedText",
                "", new Vector2(0, -470), new Vector2(400, 40), 22,
                FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            // Hint
            CreateText(canvas.transform, "HintText",
                "손가락으로 휙 밀어보세요!",
                new Vector2(0, -540), new Vector2(600, 50), 26,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));
        }

        Text CreateText(Transform parent, string name, string content,
                        Vector2 pos, Vector2 size, int fontSize,
                        FontStyle style, TextAnchor anchor, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        void SpawnPotion()
        {
            if (potionPrefab == null)
            {
                Debug.LogError("PotionSpinnerManager: Potion 프리팹이 할당되지 않았습니다!");
                return;
            }

            currentPotion = Instantiate(potionPrefab, potionPosition, Quaternion.identity);
            currentPotion.name = "Potion";
            currentPotion.transform.localScale = Vector3.one * potionScale;

            // Add spinner
            spinner = currentPotion.AddComponent<GemSpinner>();
            spinner.OnSpinUpdate += OnSpinUpdate;
            spinner.OnFlick += OnFlick;
            spinner.AddSpin(75f, 15f);
        }

        void Update()
        {
            // Live scale update so Inspector changes apply immediately
            if (currentPotion != null)
                currentPotion.transform.localScale = Vector3.one * potionScale;
        }

        void OnSpinUpdate(float totalSpins)
        {
            spinCountText.text = Mathf.FloorToInt(totalSpins).ToString("N0");

            float speed = spinner.CurrentSpeed;
            if (speed > 500f)
                speedText.text = "!!! SUPER SPIN !!!";
            else if (speed > 200f)
                speedText.text = "~~ FAST ~~";
            else if (speed > 50f)
                speedText.text = "~ spinning ~";
            else
                speedText.text = "";
        }

        void OnFlick()
        {
            if (titleText != null)
                StartCoroutine(FlashText(titleText));
        }

        System.Collections.IEnumerator FlashText(Text text)
        {
            Color original = text.color;
            text.color = Color.white;
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                text.color = Color.Lerp(Color.white, original, t / 0.3f);
                yield return null;
            }
            text.color = original;
        }
    }
}
