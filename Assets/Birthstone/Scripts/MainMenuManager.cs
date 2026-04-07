using UnityEngine;
using UnityEngine.UI;

namespace Birthstone
{
    /// <summary>
    /// Main menu that lets the user choose between
    /// Birthstone Clicker and Potion Clicker modes.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Assets - Birthstone")]
        [SerializeField] Mesh gemMesh;
        [SerializeField] Shader viewToTangentShader;
        [SerializeField] Texture2D baseMap;

        [Header("Assets - Potion")]
        [SerializeField] GameObject potionPrefab;
        [SerializeField] float potionScale = 200f;

        [Header("Gem")]
        [SerializeField] float gemScale = 1.4f;

        // UI
        Canvas canvas;
        GameObject menuPanel;
        GameObject birthstonePanel;
        GameObject spinnerPanel;
        GameObject potionPanel;

        // Birthstone mode
        Text gemNameText;
        Text gemSpinCountText;
        Text gemSpeedText;
        Button[] monthButtons;
        GameObject currentGem;
        GemSpinner gemSpinner;
        BirthstoneData.GemInfo currentGemInfo;

        // Potion mode
        Text potionSpinCountText;
        Text potionSpeedText;
        Text potionTitleText;
        GameObject currentPotion;
        GemSpinner potionSpinner;

        void Start()
        {
            SetupCamera();
            SetupLighting();
            BuildUI();
            ShowMenu();
        }

        void Update()
        {
            if (currentPotion != null)
                currentPotion.transform.localScale = Vector3.one * potionScale;
        }

        #region Scene Setup

        void SetupCamera()
        {
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.06f);
            cam.transform.position = new Vector3(0, 0.5f, -3.5f);
            cam.transform.LookAt(Vector3.zero);
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

        #endregion

        #region UI Build

        void BuildUI()
        {
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            BuildMenuPanel();
            BuildBirthstonePanel();
            BuildSpinnerPanel();
            BuildPotionPanel();
        }

        // ---- Main Menu ----
        void BuildMenuPanel()
        {
            menuPanel = CreatePanel("MenuPanel");

            CreateText(menuPanel.transform, "AppTitle",
                "Clicker",
                new Vector2(0, 300), new Vector2(800, 100), 60,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            CreateText(menuPanel.transform, "AppSubtitle",
                "모드를 선택하세요",
                new Vector2(0, 220), new Vector2(800, 60), 32,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f));

            // Birthstone button
            var btnGem = CreateButton(menuPanel.transform, "BtnBirthstone",
                "Birthstone\n탄생석 클리커",
                new Vector2(0, 50), new Vector2(600, 200),
                new Color(0.2f, 0.3f, 0.6f), 36);
            btnGem.onClick.AddListener(OnBirthstoneMode);

            // Potion button
            var btnPotion = CreateButton(menuPanel.transform, "BtnPotion",
                "Potion\n물약 클리커",
                new Vector2(0, -200), new Vector2(600, 200),
                new Color(0.4f, 0.2f, 0.5f), 36);
            btnPotion.onClick.AddListener(OnPotionMode);
        }

        // ---- Birthstone: Month Select ----
        void BuildBirthstonePanel()
        {
            birthstonePanel = CreatePanel("BirthstonePanel");

            CreateText(birthstonePanel.transform, "BsTitle",
                "나의 탄생석 찾기",
                new Vector2(0, 350), new Vector2(800, 100), 52,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            CreateText(birthstonePanel.transform, "BsSubtitle",
                "생일 월을 선택하세요",
                new Vector2(0, 280), new Vector2(800, 60), 32,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f));

            string[] monthNames = {
                "1월\n가넷", "2월\n자수정", "3월\n아쿠아마린",
                "4월\n다이아몬드", "5월\n에메랄드", "6월\n진주",
                "7월\n루비", "8월\n페리도트", "9월\n사파이어",
                "10월\n투어멀린", "11월\n토파즈", "12월\n탄자나이트"
            };

            monthButtons = new Button[12];
            for (int i = 0; i < 12; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float x = (col - 1) * 240;
                float y = 100 - row * 160;

                var gem = BirthstoneData.Gems[i];
                Color btnColor = Color.Lerp(gem.BaseColor, Color.white, 0.2f);

                monthButtons[i] = CreateButton(birthstonePanel.transform, $"Month_{i + 1}",
                    monthNames[i], new Vector2(x, y), new Vector2(220, 140),
                    btnColor, 24);

                int idx = i;
                monthButtons[i].onClick.AddListener(() => OnMonthSelected(idx));
            }

            var backBtn = CreateButton(birthstonePanel.transform, "BsBack",
                "← 메인 메뉴", new Vector2(-380, 440), new Vector2(240, 80),
                new Color(0.3f, 0.3f, 0.35f), 28);
            backBtn.onClick.AddListener(ShowMenu);

            birthstonePanel.SetActive(false);
        }

        // ---- Birthstone: Spinner ----
        void BuildSpinnerPanel()
        {
            spinnerPanel = CreatePanel("SpinnerPanel");

            gemNameText = CreateText(spinnerPanel.transform, "GemName",
                "", new Vector2(0, 400), new Vector2(800, 120), 48,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            gemSpinCountText = CreateText(spinnerPanel.transform, "GemSpinCount",
                "0", new Vector2(0, -350), new Vector2(600, 100), 72,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            CreateText(spinnerPanel.transform, "GemSpinLabel",
                "SPINS", new Vector2(0, -420), new Vector2(400, 50), 28,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.7f));

            gemSpeedText = CreateText(spinnerPanel.transform, "GemSpeed",
                "", new Vector2(0, -470), new Vector2(400, 40), 22,
                FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            CreateText(spinnerPanel.transform, "GemHint",
                "손가락으로 휙 밀어보세요!",
                new Vector2(0, -540), new Vector2(600, 50), 26,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            var backBtn = CreateButton(spinnerPanel.transform, "SpinnerBack",
                "← 다시 선택", new Vector2(-380, 440), new Vector2(240, 80),
                new Color(0.3f, 0.3f, 0.35f), 28);
            backBtn.onClick.AddListener(ShowBirthstoneSelect);

            spinnerPanel.SetActive(false);
        }

        // ---- Potion: Spinner ----
        void BuildPotionPanel()
        {
            potionPanel = CreatePanel("PotionPanel");

            potionTitleText = CreateText(potionPanel.transform, "PotionTitle",
                "Potion Clicker",
                new Vector2(0, 400), new Vector2(800, 100), 52,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.8f, 0.6f, 1f));

            potionSpinCountText = CreateText(potionPanel.transform, "PotionSpinCount",
                "0", new Vector2(0, -350), new Vector2(600, 100), 72,
                FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            CreateText(potionPanel.transform, "PotionSpinLabel",
                "SPINS", new Vector2(0, -420), new Vector2(400, 50), 28,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.7f));

            potionSpeedText = CreateText(potionPanel.transform, "PotionSpeed",
                "", new Vector2(0, -470), new Vector2(400, 40), 22,
                FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            CreateText(potionPanel.transform, "PotionHint",
                "손가락으로 휙 밀어보세요!",
                new Vector2(0, -540), new Vector2(600, 50), 26,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.6f));

            var backBtn = CreateButton(potionPanel.transform, "PotionBack",
                "← 메인 메뉴", new Vector2(-380, 440), new Vector2(240, 80),
                new Color(0.3f, 0.3f, 0.35f), 28);
            backBtn.onClick.AddListener(ShowMenu);

            potionPanel.SetActive(false);
        }

        #endregion

        #region Navigation

        void HideAll()
        {
            menuPanel.SetActive(false);
            birthstonePanel.SetActive(false);
            spinnerPanel.SetActive(false);
            potionPanel.SetActive(false);
            ClearObjects();
        }

        void ClearObjects()
        {
            if (currentGem != null) { Destroy(currentGem); currentGem = null; }
            if (currentPotion != null) { Destroy(currentPotion); currentPotion = null; }
        }

        void ShowMenu()
        {
            HideAll();
            menuPanel.SetActive(true);
        }

        void OnBirthstoneMode()
        {
            ShowBirthstoneSelect();
        }

        void ShowBirthstoneSelect()
        {
            HideAll();
            birthstonePanel.SetActive(true);
        }

        void OnMonthSelected(int monthIndex)
        {
            currentGemInfo = BirthstoneData.Gems[monthIndex];
            birthstonePanel.SetActive(false);
            spinnerPanel.SetActive(true);
            SpawnGem();
        }

        void OnPotionMode()
        {
            HideAll();
            potionPanel.SetActive(true);
            SpawnPotion();
        }

        #endregion

        #region Spawn

        void SpawnGem()
        {
            if (currentGem != null) Destroy(currentGem);

            if (viewToTangentShader == null)
                viewToTangentShader = Shader.Find("Birthstone/View to Tangent Rainbow");

            if (gemMesh == null || viewToTangentShader == null)
            {
                Debug.LogError("Gem Mesh 또는 Shader가 할당되지 않았습니다!");
                return;
            }

            currentGem = new GameObject(currentGemInfo.Name);
            currentGem.transform.position = Vector3.zero;
            currentGem.transform.localScale = Vector3.one * gemScale;

            var mf = currentGem.AddComponent<MeshFilter>();
            mf.sharedMesh = gemMesh;

            var mr = currentGem.AddComponent<MeshRenderer>();
            var mat = new Material(viewToTangentShader);
            mat.name = currentGemInfo.Name + "_Material";
            mat.SetColor("_ColorA", currentGemInfo.DeepColor);
            Color bright = Color.Lerp(currentGemInfo.BaseColor, Color.white, 0.5f);
            mat.SetColor("_ColorB", bright);
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetColor("_SpecularColor", Color.white);
            mat.SetFloat("_Offset", 0.3f);
            if (baseMap != null) mat.SetTexture("_BaseMap", baseMap);
            mr.material = mat;

            gemSpinner = currentGem.AddComponent<GemSpinner>();
            gemSpinner.OnSpinUpdate += OnGemSpinUpdate;
            gemSpinner.OnFlick += OnGemFlick;
            gemSpinner.AddSpin(200f, 50f);

            gemNameText.text = $"{currentGemInfo.Month}월 탄생석\n{currentGemInfo.NameKorean}";
            gemNameText.color = Color.Lerp(currentGemInfo.BaseColor, Color.white, 0.5f);
            gemSpinCountText.text = "0";
        }

        void SpawnPotion()
        {
            if (currentPotion != null) Destroy(currentPotion);

            if (potionPrefab == null)
            {
                Debug.LogError("Potion 프리팹이 할당되지 않았습니다!");
                return;
            }

            currentPotion = Instantiate(potionPrefab, new Vector3(0, -0.5f, 0), Quaternion.identity);
            currentPotion.name = "Potion";
            currentPotion.transform.localScale = Vector3.one * potionScale;

            potionSpinner = currentPotion.AddComponent<GemSpinner>();
            potionSpinner.OnSpinUpdate += OnPotionSpinUpdate;
            potionSpinner.OnFlick += OnPotionFlick;
            potionSpinner.AddSpin(150f, 30f);

            potionSpinCountText.text = "0";
        }

        #endregion

        #region Callbacks

        void OnGemSpinUpdate(float totalSpins)
        {
            gemSpinCountText.text = Mathf.FloorToInt(totalSpins).ToString("N0");
            UpdateSpeedText(gemSpeedText, gemSpinner.CurrentSpeed);
        }

        void OnGemFlick()
        {
            if (gemNameText != null) StartCoroutine(FlashText(gemNameText));
        }

        void OnPotionSpinUpdate(float totalSpins)
        {
            potionSpinCountText.text = Mathf.FloorToInt(totalSpins).ToString("N0");
            UpdateSpeedText(potionSpeedText, potionSpinner.CurrentSpeed);
        }

        void OnPotionFlick()
        {
            if (potionTitleText != null) StartCoroutine(FlashText(potionTitleText));
        }

        void UpdateSpeedText(Text text, float speed)
        {
            if (speed > 500f) text.text = "!!! SUPER SPIN !!!";
            else if (speed > 200f) text.text = "~~ FAST ~~";
            else if (speed > 50f) text.text = "~ spinning ~";
            else text.text = "";
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

        #region UI Helpers

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
    }
}
