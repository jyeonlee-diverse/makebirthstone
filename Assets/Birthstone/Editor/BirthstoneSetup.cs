using UnityEditor;
using UnityEngine;

namespace Birthstone.Editor
{
    /// <summary>
    /// Editor menu to set up the birthstone scene.
    /// Auto-assigns Gem mesh and View to Tangent shader.
    /// </summary>
    public static class BirthstoneSetup
    {
        const string GemMeshPath = "Assets/USB Project/Chapter 1/View to Tangent/Meshes/Gem.fbx";
        const string ShaderPath = "Assets/Birthstone/Shaders/ViewToTangent_Rainbow.shader";
        const string BaseMapPath = "Assets/USB Project/Chapter 1/View to Tangent/Textures/Pattern_01.png";

        [MenuItem("Birthstone/Setup Scene")]
        public static void SetupScene()
        {
            // Check if manager already exists
            if (Object.FindAnyObjectByType<BirthstoneManager>() != null)
            {
                if (!EditorUtility.DisplayDialog("Birthstone Setup",
                    "BirthstoneManager already exists in the scene. Replace it?",
                    "Replace", "Cancel"))
                    return;

                var existing = Object.FindAnyObjectByType<BirthstoneManager>();
                Object.DestroyImmediate(existing.gameObject);
            }

            // Load assets
            var gemMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GemMeshPath);
            if (gemMesh == null)
            {
                // Gem.fbx contains sub-assets; load the mesh from inside the fbx
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(GemMeshPath);
                foreach (var asset in allAssets)
                {
                    if (asset is Mesh m)
                    {
                        gemMesh = m;
                        break;
                    }
                }
            }

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseMapPath);

            if (gemMesh == null)
            {
                Debug.LogError($"Gem mesh not found at: {GemMeshPath}");
                return;
            }
            if (shader == null)
            {
                Debug.LogError($"Shader not found at: {ShaderPath}");
                return;
            }

            // Create manager and assign references
            var managerObj = new GameObject("BirthstoneManager");
            var manager = managerObj.AddComponent<BirthstoneManager>();

            var so = new SerializedObject(manager);
            so.FindProperty("gemMesh").objectReferenceValue = gemMesh;
            so.FindProperty("viewToTangentShader").objectReferenceValue = shader;
            if (baseMap != null)
                so.FindProperty("baseMap").objectReferenceValue = baseMap;
            so.ApplyModifiedProperties();

            // Add camera controller to main camera
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<BirthstoneCamera>() == null)
            {
                cam.gameObject.AddComponent<BirthstoneCamera>();
            }

            Selection.activeGameObject = managerObj;
            EditorUtility.SetDirty(managerObj);

            Debug.Log($"Birthstone scene setup complete!\n" +
                      $"  Gem Mesh: {gemMesh.name}\n" +
                      $"  Shader: {shader.name}\n" +
                      $"  Base Map: {(baseMap != null ? baseMap.name : "none")}\n" +
                      $"Press Play to see the birthstones.");
        }

        [MenuItem("Birthstone/Setup Spinner Scene")]
        public static void SetupSpinnerScene()
        {
            // Remove existing SpinnerManager
            var existing = Object.FindAnyObjectByType<SpinnerManager>();
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            // Load assets
            Mesh gemMesh = null;
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(GemMeshPath);
            foreach (var asset in allAssets)
            {
                if (asset is Mesh m) { gemMesh = m; break; }
            }

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseMapPath);

            if (gemMesh == null || shader == null)
            {
                Debug.LogError("Gem mesh or shader not found!");
                return;
            }

            // Remove BirthstoneManager/Camera if present
            var bm = Object.FindAnyObjectByType<BirthstoneManager>();
            if (bm != null) Object.DestroyImmediate(bm.gameObject);
            var bc = Object.FindAnyObjectByType<BirthstoneCamera>();
            if (bc != null) Object.DestroyImmediate(bc);

            // Create SpinnerManager
            var managerObj = new GameObject("SpinnerManager");
            var manager = managerObj.AddComponent<SpinnerManager>();

            var so = new SerializedObject(manager);
            so.FindProperty("gemMesh").objectReferenceValue = gemMesh;
            so.FindProperty("viewToTangentShader").objectReferenceValue = shader;
            if (baseMap != null)
                so.FindProperty("baseMap").objectReferenceValue = baseMap;
            so.ApplyModifiedProperties();

            // Make sure EventSystem exists for UI
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            Selection.activeGameObject = managerObj;
            EditorUtility.SetDirty(managerObj);

            Debug.Log("Spinner scene setup complete! Press Play to start.");
        }

        [MenuItem("Birthstone/Create Single Gem")]
        public static void CreateSingleGem()
        {
            var menu = new GenericMenu();
            foreach (var gem in BirthstoneData.Gems)
            {
                var info = gem;
                menu.AddItem(new GUIContent($"{info.Month}월 - {info.NameKorean} ({info.Name})"),
                    false, () => SpawnSingleGem(info));
            }
            menu.ShowAsContext();
        }

        static void SpawnSingleGem(BirthstoneData.GemInfo info)
        {
            var gemMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GemMeshPath);
            if (gemMesh == null)
            {
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(GemMeshPath);
                foreach (var asset in allAssets)
                {
                    if (asset is Mesh m) { gemMesh = m; break; }
                }
            }

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseMapPath);

            if (gemMesh == null || shader == null)
            {
                Debug.LogError("Gem mesh or shader not found!");
                return;
            }

            var obj = new GameObject(info.Name);
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = gemMesh;

            var meshRenderer = obj.AddComponent<MeshRenderer>();
            var mat = new Material(shader);
            mat.name = info.Name + "_Material";
            mat.SetColor("_ColorA", info.DeepColor);
            mat.SetColor("_ColorB", info.BaseColor);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetColor("_SpecularColor", Color.white);
            mat.SetFloat("_Offset", 0.3f);
            if (baseMap != null)
                mat.SetTexture("_BaseMap", baseMap);
            meshRenderer.material = mat;

            if (SceneView.lastActiveSceneView != null)
                obj.transform.position = SceneView.lastActiveSceneView.pivot;

            obj.AddComponent<GemRotator>();
            Selection.activeGameObject = obj;
            Undo.RegisterCreatedObjectUndo(obj, "Create " + info.Name);

            Debug.Log($"Created {info.NameKorean} ({info.Name}) - {info.Month}월 탄생석");
        }
    }
}
