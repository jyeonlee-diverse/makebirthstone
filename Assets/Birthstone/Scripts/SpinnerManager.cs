using UnityEngine;
using UnityEngine.UI;

namespace Birthstone
{
    /// <summary>
    /// Manages the birthstone spinner scene:
    /// 1. Birth month input UI
    /// 2. Gem creation with View to Tangent Rainbow shader
    /// 3. Spin counter display
    /// </summary>
    public class SpinnerManager : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField] Mesh gemMesh;
        [SerializeField] Shader viewToTangentShader;
        [SerializeField] Texture2D baseMap;

        [Header("Gem")]
        [SerializeField] float gemScale = 1.4f;
        [SerializeField] Vector3 gemPosition = new Vector3(0, 0, 0);

        // UI references (created at runtime)
        Canvas canvas;
        GameObject inputPanel;
        GameObject spinnerPanel;
        Text spinCountText;
        Text gemNameText;
        Text speedText;
        Button[] monthButtons;
        Button backButton;

        GameObject currentGem;
        GemSpinner spinner;
        BirthstoneData.GemInfo currentGemInfo;

        void Start()
        {
            SetupCamera();
            SetupLighting();
            CreateUI();
            ShowInputPanel();
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.06f);
            cam.transform.position = new Vector3(0, 0.5f, -3.5f);
            cam.transform.LookAt(new Vector3(0, 0, 0));
        }

        void SetupLighting()
        {
            if (FindAnyObjectByType<Light>() == null)
            {
                CreateLight("MainLight", Quaternion.Euler(40, -30, 0), new Color(1f, 0.95f, 0.9f), 1.5f);
            }
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

        #region UI Creation

        void CreateUI()
        {
            // Canvas
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            CreateInputPanel();
            CreateSpinnerPanel();
        }

        void CreateInputPanel()
        {
            inputPanel = CreatePanel("InputPanel");

            // Title
            CreateText(inputPanel.transform, "TitleText",
                "나의 탄생석 찾기",
                new Vector2(0, 350), new Vector2(800, 100), 52,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            // Subtitle
            CreateText(inputPanel.transform, "SubtitleText",
                "생일 월을 선택하세요",
                new Vector2(0, 280), new Vector2(800, 60), 32,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f));

            // Month buttons grid (4x3)
            monthButtons = new Button[12];
            string[] monthNames = {
                "1월\n가넷", "2월\n자수정", "3월\n아쿠아마린",
                "4월\n다이아몬드", "5월\n에메랄드", "6월\n진주",
                "7월\n루비", "8월\n페리도트", "9월\n사파이어",
                "10월\n투어멀린", "11월\n토파즈", "12월\n탄자나이트"
            };

            for (int i = 0; i < 12; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float x = (col - 1) * 240;
                float y = 100 - row * 160;

                var gem = BirthstoneData.Gems[i];
                Color btnColor = Color.Lerp(gem.BaseColor, Color.white, 0.2f);

                monthButtons[i] = CreateButton(inputPanel.transform, $"Month_{i + 1}",
                    monthNames[i], new Vector2(x, y), new Vector2(220, 140),
                    btnColor, 24);

                int monthIndex = i;
                monthButtons[i].onClick.AddListener(() => OnMonthSelected(monthIndex));
            }
        }

        void CreateSpinnerPanel()
        {
            spinnerPanel = CreatePanel("SpinnerPanel");

            // Gem name
            gemNameText = CreateText(spinnerPanel.transform, "GemNameText",
                "", new Vector2(0, 400), new Vector2(800, 120), 48,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            // Spin count
            spinCountText = CreateText(spinnerPanel.transform, "SpinCountText",
                "0", new Vector2(0, -350), new Vector2(600, 100), 72,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            // Spin label
            CreateText(spinnerPanel.transform, "SpinLabel",
                "SPINS", new Vector2(0, -420), new Vector2(400, 50), 28,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.7f));

            // Speed indicator
            speedText = CreateText(spinnerPanel.transform, "SpeedText",
                "", new Vector2(0, -470), new Vector2(400, 40), 22,
                FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            // Hint
            CreateText(spinnerPanel.transform, "HintText",
                "손가락으로 휙 밀어보세요!",
                new Vector2(0, -540), new Vector2(600, 50), 26,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            // Back button
            backButton = CreateButton(spinnerPanel.transform, "BackButton",
                "← 다시 선택", new Vector2(-380, 440), new Vector2(240, 80),
                new Color(0.3f, 0.3f, 0.35f), 28);
            backButton.onClick.AddListener(OnBackPressed);

            spinnerPanel.SetActive(false);
        }

        GameObject CreatePanel(string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return panel;
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

        Button CreateButton(Transform parent, string name, string label,
                           Vector2 pos, Vector2 size, Color bgColor, int fontSize)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var image = obj.AddComponent<Image>();
            image.color = bgColor;

            var button = obj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
            button.colors = colors;

            // Button text
            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(obj.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 5);
            textRt.offsetMax = new Vector2(-5, -5);

            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return button;
        }

        #endregion

        #region Flow

        void ShowInputPanel()
        {
            inputPanel.SetActive(true);
            spinnerPanel.SetActive(false);

            if (currentGem != null)
                Destroy(currentGem);
        }

        void OnMonthSelected(int monthIndex)
        {
            currentGemInfo = BirthstoneData.Gems[monthIndex];
            inputPanel.SetActive(false);
            spinnerPanel.SetActive(true);

            SpawnGem();
        }

        void OnBackPressed()
        {
            ShowInputPanel();
        }

        #endregion

        #region Gem

        void SpawnGem()
        {
            if (currentGem != null)
                Destroy(currentGem);

            // Load shader
            if (viewToTangentShader == null)
                viewToTangentShader = Shader.Find("Birthstone/View to Tangent Rainbow");

            if (gemMesh == null || viewToTangentShader == null)
            {
                Debug.LogError("SpinnerManager: Gem Mesh 또는 Shader가 할당되지 않았습니다!");
                return;
            }

            currentGem = new GameObject(currentGemInfo.Name);
            currentGem.transform.position = gemPosition;
            currentGem.transform.localScale = Vector3.one * gemScale;

            var meshFilter = currentGem.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = gemMesh;

            var meshRenderer = currentGem.AddComponent<MeshRenderer>();
            var mat = new Material(viewToTangentShader);
            mat.name = currentGemInfo.Name + "_Material";
            // ColorA = deep shadow color, ColorB = bright highlight color
            // Brighten BaseColor so the basemap texture pattern is clearly visible
            mat.SetColor("_ColorA", currentGemInfo.DeepColor);
            Color bright = Color.Lerp(currentGemInfo.BaseColor, Color.white, 0.5f);
            mat.SetColor("_ColorB", bright);
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetColor("_SpecularColor", Color.white);
            mat.SetFloat("_Offset", 0.3f);
            if (baseMap != null)
                mat.SetTexture("_BaseMap", baseMap);
            meshRenderer.material = mat;

            // Add spinner
            spinner = currentGem.AddComponent<GemSpinner>();
            spinner.OnSpinUpdate += OnSpinUpdate;
            spinner.OnFlick += OnFlick;

            // Initial gentle spin
            spinner.AddSpin(200f, 50f);

            // Update UI
            gemNameText.text = $"{currentGemInfo.Month}월 탄생석\n{currentGemInfo.NameKorean}";
            gemNameText.color = Color.Lerp(currentGemInfo.BaseColor, Color.white, 0.5f);
            spinCountText.text = "0";
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
            // Flash effect on gem name
            if (gemNameText != null)
                StartCoroutine(FlashText(gemNameText));
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

        #endregion
    }
}
