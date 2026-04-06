using UnityEngine;

namespace Birthstone
{
    /// <summary>
    /// Main manager that spawns all 12 birthstones in a circular arrangement.
    /// Uses a pre-made Gem mesh and the "View to Tangent HLSL" shader.
    /// </summary>
    public class BirthstoneManager : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] float circleRadius = 5f;
        [SerializeField] float gemScale = 1.2f;
        [SerializeField] float labelOffset = 1.2f;

        [Header("Assets")]
        [SerializeField] Mesh gemMesh;
        [SerializeField] Shader viewToTangentShader;
        [SerializeField] Texture2D baseMap;

        [Header("Rotation")]
        [SerializeField] float gemRotationSpeed = 30f;
        [SerializeField] float platformRotationSpeed = 5f;
        [SerializeField] bool autoRotate = true;

        // Legacy fields kept for compatibility
        [HideInInspector] [SerializeField] Shader gemShader;
        [HideInInspector] [SerializeField] Shader pearlShader;

        Transform gemsParent;
        GameObject[] gemObjects;

        void Start()
        {
            SetupScene();
            SpawnBirthstones();
        }

        void SetupScene()
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            Camera.main.transform.position = new Vector3(0, 3f, -9f);
            Camera.main.transform.LookAt(Vector3.zero);

            if (FindAnyObjectByType<Light>() == null)
            {
                CreateLight("MainLight", Quaternion.Euler(50, -30, 0),
                            new Color(1f, 0.95f, 0.9f), 1.2f);
            }

            CreateLight("FillLight", Quaternion.Euler(30, 150, 0),
                        new Color(0.6f, 0.7f, 1f), 0.5f);
            CreateLight("RimLight", Quaternion.Euler(-20, 90, 0),
                        new Color(1f, 0.9f, 0.8f), 0.3f);

            gemsParent = new GameObject("Birthstones").transform;
        }

        void CreateLight(string name, Quaternion rotation, Color color, float intensity)
        {
            var lightObj = new GameObject(name);
            lightObj.transform.rotation = rotation;
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
        }

        void SpawnBirthstones()
        {
            if (gemMesh == null)
            {
                Debug.LogError("BirthstoneManager: Gem Mesh가 할당되지 않았습니다! Inspector에서 Gem.fbx를 할당해주세요.");
                return;
            }

            if (viewToTangentShader == null)
                viewToTangentShader = Shader.Find("Birthstone/View to Tangent Rainbow");

            if (viewToTangentShader == null)
            {
                Debug.LogError("BirthstoneManager: View to Tangent Rainbow 셰이더를 찾을 수 없습니다!");
                return;
            }

            var gems = BirthstoneData.Gems;
            gemObjects = new GameObject[gems.Length];

            for (int i = 0; i < gems.Length; i++)
            {
                float angle = (float)i / gems.Length * Mathf.PI * 2f - Mathf.PI / 2f;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * circleRadius,
                    0,
                    Mathf.Sin(angle) * circleRadius);

                gemObjects[i] = CreateGemObject(gems[i], position);
                gemObjects[i].transform.SetParent(gemsParent);

                CreateLabel(gems[i], position);
                CreatePedestal(position);
            }
        }

        GameObject CreateGemObject(BirthstoneData.GemInfo info, Vector3 position)
        {
            var obj = new GameObject(info.Name);
            obj.transform.position = position + Vector3.up * 1.0f;
            obj.transform.localScale = Vector3.one * gemScale;

            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = gemMesh;

            var meshRenderer = obj.AddComponent<MeshRenderer>();

            var mat = new Material(viewToTangentShader);
            mat.name = info.Name + "_Material";
            mat.SetColor("_ColorA", info.DeepColor);
            Color bright = Color.Lerp(info.BaseColor, Color.white, 0.5f);
            mat.SetColor("_ColorB", bright);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetColor("_SpecularColor", Color.white);
            mat.SetFloat("_Offset", 0.3f);

            if (baseMap != null)
                mat.SetTexture("_BaseMap", baseMap);

            meshRenderer.material = mat;

            var rotator = obj.AddComponent<GemRotator>();
            rotator.rotationSpeed = gemRotationSpeed;

            // Add collider for tap/click focus
            var collider = obj.AddComponent<MeshCollider>();
            collider.sharedMesh = gemMesh;

            return obj;
        }

        void CreateLabel(BirthstoneData.GemInfo info, Vector3 position)
        {
            var labelObj = new GameObject(info.Name + "_Label");
            labelObj.transform.position = position + Vector3.up * (1.0f + labelOffset + 0.7f);
            labelObj.transform.SetParent(gemsParent);

            var textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"{info.Month}월\n{info.NameKorean}\n{info.Name}";
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.9f, 0.9f, 0.95f);

            labelObj.AddComponent<BillboardText>();
        }

        void CreatePedestal(Vector3 position)
        {
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.position = position + Vector3.down * 0.15f;
            pedestal.transform.localScale = new Vector3(0.8f, 0.15f, 0.8f);
            pedestal.transform.SetParent(gemsParent);

            var renderer = pedestal.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.18f));
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.3f);
            renderer.material = mat;
        }

        void Update()
        {
            if (autoRotate && gemsParent != null)
            {
                gemsParent.Rotate(Vector3.up, platformRotationSpeed * Time.deltaTime);
            }
        }
    }
}
