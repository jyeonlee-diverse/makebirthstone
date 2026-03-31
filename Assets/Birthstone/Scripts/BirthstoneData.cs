using UnityEngine;

namespace Birthstone
{
    /// <summary>
    /// Data definitions for all 12 birthstones.
    /// </summary>
    public static class BirthstoneData
    {
        [System.Serializable]
        public struct GemInfo
        {
            public string Name;
            public string NameKorean;
            public int Month;
            public Color BaseColor;
            public Color DeepColor;
            public float RefractIndex;
            public float DispersionStrength;
            public float SparkleIntensity;
            public float Transparency;
            public GemMeshGenerator.GemCut Cut;
            public bool IsPearl;

            public GemInfo(string name, string nameKorean, int month,
                           Color baseColor, Color deepColor,
                           float refractIndex, float dispersion,
                           float sparkle, float transparency,
                           GemMeshGenerator.GemCut cut, bool isPearl = false)
            {
                Name = name;
                NameKorean = nameKorean;
                Month = month;
                BaseColor = baseColor;
                DeepColor = deepColor;
                RefractIndex = refractIndex;
                DispersionStrength = dispersion;
                SparkleIntensity = sparkle;
                Transparency = transparency;
                Cut = cut;
                IsPearl = isPearl;
            }
        }

        public static readonly GemInfo[] Gems = new GemInfo[]
        {
            // January - Garnet (deep red)
            new GemInfo(
                "Garnet", "가넷", 1,
                new Color(0.55f, 0.05f, 0.08f),
                new Color(0.3f, 0.02f, 0.05f),
                1.74f, 0.022f, 1.5f, 0.65f,
                GemMeshGenerator.GemCut.Cushion),

            // February - Amethyst (purple)
            new GemInfo(
                "Amethyst", "자수정", 2,
                new Color(0.55f, 0.15f, 0.7f),
                new Color(0.3f, 0.05f, 0.45f),
                1.544f, 0.013f, 1.2f, 0.7f,
                GemMeshGenerator.GemCut.Oval),

            // March - Aquamarine (light blue)
            new GemInfo(
                "Aquamarine", "아쿠아마린", 3,
                new Color(0.4f, 0.75f, 0.9f),
                new Color(0.2f, 0.5f, 0.7f),
                1.577f, 0.014f, 1.0f, 0.8f,
                GemMeshGenerator.GemCut.Emerald),

            // April - Diamond (clear/white)
            new GemInfo(
                "Diamond", "다이아몬드", 4,
                new Color(0.95f, 0.95f, 1.0f),
                new Color(0.85f, 0.85f, 0.95f),
                2.42f, 0.044f, 3.0f, 0.85f,
                GemMeshGenerator.GemCut.Brilliant),

            // May - Emerald (green)
            new GemInfo(
                "Emerald", "에메랄드", 5,
                new Color(0.1f, 0.6f, 0.25f),
                new Color(0.05f, 0.35f, 0.12f),
                1.58f, 0.014f, 0.8f, 0.65f,
                GemMeshGenerator.GemCut.Emerald),

            // June - Pearl (white/cream)
            new GemInfo(
                "Pearl", "진주", 6,
                new Color(0.95f, 0.92f, 0.88f),
                new Color(0.9f, 0.87f, 0.82f),
                1.53f, 0.0f, 0.0f, 0.0f,
                GemMeshGenerator.GemCut.Sphere, true),

            // July - Ruby (red)
            new GemInfo(
                "Ruby", "루비", 7,
                new Color(0.8f, 0.05f, 0.1f),
                new Color(0.5f, 0.02f, 0.08f),
                1.77f, 0.018f, 2.0f, 0.6f,
                GemMeshGenerator.GemCut.Oval),

            // August - Peridot (yellow-green)
            new GemInfo(
                "Peridot", "페리도트", 8,
                new Color(0.55f, 0.7f, 0.15f),
                new Color(0.35f, 0.5f, 0.08f),
                1.69f, 0.02f, 1.2f, 0.7f,
                GemMeshGenerator.GemCut.Cushion),

            // September - Sapphire (blue)
            new GemInfo(
                "Sapphire", "사파이어", 9,
                new Color(0.1f, 0.15f, 0.7f),
                new Color(0.05f, 0.08f, 0.45f),
                1.77f, 0.018f, 2.0f, 0.6f,
                GemMeshGenerator.GemCut.Brilliant),

            // October - Tourmaline (pink)
            new GemInfo(
                "Tourmaline", "투어멀린", 10,
                new Color(0.85f, 0.3f, 0.5f),
                new Color(0.6f, 0.15f, 0.3f),
                1.64f, 0.017f, 1.3f, 0.7f,
                GemMeshGenerator.GemCut.Pear),

            // November - Topaz (golden yellow)
            new GemInfo(
                "Topaz", "토파즈", 11,
                new Color(0.9f, 0.65f, 0.15f),
                new Color(0.7f, 0.45f, 0.08f),
                1.63f, 0.014f, 1.5f, 0.75f,
                GemMeshGenerator.GemCut.Marquise),

            // December - Tanzanite (blue-violet)
            new GemInfo(
                "Tanzanite", "탄자나이트", 12,
                new Color(0.25f, 0.15f, 0.75f),
                new Color(0.15f, 0.08f, 0.5f),
                1.69f, 0.019f, 1.8f, 0.65f,
                GemMeshGenerator.GemCut.Heart),
        };
    }
}
