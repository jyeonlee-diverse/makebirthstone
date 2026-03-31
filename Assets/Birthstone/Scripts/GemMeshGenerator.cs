using UnityEngine;

namespace Birthstone
{
    /// <summary>
    /// Generates procedural gem meshes of various cuts.
    /// </summary>
    public static class GemMeshGenerator
    {
        public enum GemCut
        {
            Brilliant,      // Classic diamond cut
            Emerald,        // Rectangular step cut
            Oval,           // Oval brilliant
            Round,          // Simple round
            Pear,           // Teardrop shape
            Marquise,       // Eye-shaped
            Cushion,        // Square with rounded corners
            Heart,          // Heart shape
            Sphere          // For pearls
        }

        /// <summary>
        /// Creates a gem mesh with the specified cut type.
        /// </summary>
        public static Mesh CreateGem(GemCut cut, float size = 1f)
        {
            switch (cut)
            {
                case GemCut.Brilliant:  return CreateBrilliantCut(size);
                case GemCut.Emerald:    return CreateEmeraldCut(size);
                case GemCut.Oval:       return CreateOvalCut(size);
                case GemCut.Round:      return CreateRoundCut(size);
                case GemCut.Pear:       return CreatePearCut(size);
                case GemCut.Marquise:   return CreateMarquiseCut(size);
                case GemCut.Cushion:    return CreateCushionCut(size);
                case GemCut.Heart:      return CreateHeartCut(size);
                case GemCut.Sphere:     return CreateSphere(size);
                default:                return CreateBrilliantCut(size);
            }
        }

        /// <summary>
        /// Classic round brilliant cut diamond - the most iconic gem shape.
        /// Crown (top), girdle (middle ring), pavilion (bottom cone).
        /// </summary>
        static Mesh CreateBrilliantCut(float size)
        {
            int sides = 16;
            float crownHeight = 0.35f * size;
            float pavilionDepth = 0.55f * size;
            float girdleRadius = 0.5f * size;
            float tableRadius = 0.3f * size;
            float crownMidRadius = 0.45f * size;
            float crownMidHeight = 0.2f * size;

            var builder = new MeshBuilder();

            // Table (top flat face)
            int tableCenter = builder.AddVertex(new Vector3(0, crownHeight, 0));
            int[] tableVerts = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                tableVerts[i] = builder.AddVertex(new Vector3(
                    Mathf.Cos(angle) * tableRadius,
                    crownHeight,
                    Mathf.Sin(angle) * tableRadius));
            }
            for (int i = 0; i < sides; i++)
            {
                builder.AddTriangle(tableCenter, tableVerts[i], tableVerts[(i + 1) % sides]);
            }

            // Crown mid ring
            int[] crownMidVerts = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / sides;
                crownMidVerts[i] = builder.AddVertex(new Vector3(
                    Mathf.Cos(angle) * crownMidRadius,
                    crownMidHeight,
                    Mathf.Sin(angle) * crownMidRadius));
            }

            // Crown star facets (table to crown mid)
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableVerts[i], crownMidVerts[i], tableVerts[next]);
            }

            // Girdle vertices
            int[] girdleVerts = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                girdleVerts[i] = builder.AddVertex(new Vector3(
                    Mathf.Cos(angle) * girdleRadius,
                    0,
                    Mathf.Sin(angle) * girdleRadius));
            }

            // Crown bezel facets (crown mid to girdle)
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(crownMidVerts[i], girdleVerts[next], girdleVerts[i]);
                builder.AddTriangle(crownMidVerts[i], crownMidVerts[(i + 1) % sides], girdleVerts[next]);
            }

            // Pavilion (bottom cone)
            int culet = builder.AddVertex(new Vector3(0, -pavilionDepth, 0));
            // Pavilion mid ring
            float pavMidRadius = 0.25f * size;
            float pavMidDepth = 0.35f * size;
            int[] pavMidVerts = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / sides;
                pavMidVerts[i] = builder.AddVertex(new Vector3(
                    Mathf.Cos(angle) * pavMidRadius,
                    -pavMidDepth,
                    Mathf.Sin(angle) * pavMidRadius));
            }

            // Girdle to pavilion mid
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(girdleVerts[i], girdleVerts[next], pavMidVerts[i]);
                builder.AddTriangle(pavMidVerts[i], girdleVerts[next], pavMidVerts[(i + 1) % sides]);
            }

            // Pavilion mid to culet
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(pavMidVerts[i], pavMidVerts[next], culet);
            }

            return builder.Build("BrilliantCut");
        }

        /// <summary>
        /// Emerald/step cut - rectangular with chamfered corners and stepped facets.
        /// </summary>
        static Mesh CreateEmeraldCut(float size)
        {
            var builder = new MeshBuilder();

            float w = 0.4f * size;
            float h = 0.55f * size;
            float chamfer = 0.12f * size;
            float crownH = 0.25f * size;
            float pavH = 0.45f * size;
            float tableScale = 0.65f;
            float midScale = 0.85f;
            float midH = 0.12f * size;

            // Generate octagonal profile (rectangle with chamfered corners)
            Vector2[] GenOctagon(float ww, float hh, float cc)
            {
                return new Vector2[]
                {
                    new Vector2(-ww + cc, -hh),
                    new Vector2(ww - cc, -hh),
                    new Vector2(ww, -hh + cc),
                    new Vector2(ww, hh - cc),
                    new Vector2(ww - cc, hh),
                    new Vector2(-ww + cc, hh),
                    new Vector2(-ww, hh - cc),
                    new Vector2(-ww, -hh + cc),
                };
            }

            var table = GenOctagon(w * tableScale, h * tableScale, chamfer * tableScale);
            var crownMid = GenOctagon(w * midScale, h * midScale, chamfer * midScale);
            var girdle = GenOctagon(w, h, chamfer);

            int sides = 8;

            // Table face
            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0));
            int[] tableV = new int[sides];
            for (int i = 0; i < sides; i++)
                tableV[i] = builder.AddVertex(new Vector3(table[i].x, crownH, table[i].y));

            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            // Crown mid
            int[] midV = new int[sides];
            for (int i = 0; i < sides; i++)
                midV[i] = builder.AddVertex(new Vector3(crownMid[i].x, midH, crownMid[i].y));

            // Table to mid
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], midV[i], tableV[next]);
                builder.AddTriangle(tableV[next], midV[i], midV[next]);
            }

            // Girdle
            int[] girdleV = new int[sides];
            for (int i = 0; i < sides; i++)
                girdleV[i] = builder.AddVertex(new Vector3(girdle[i].x, 0, girdle[i].y));

            // Mid to girdle
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(midV[i], girdleV[i], midV[next]);
                builder.AddTriangle(midV[next], girdleV[i], girdleV[next]);
            }

            // Pavilion
            int culet = builder.AddVertex(new Vector3(0, -pavH, 0));
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(girdleV[i], culet, girdleV[next]);
            }

            return builder.Build("EmeraldCut");
        }

        /// <summary>
        /// Oval brilliant cut.
        /// </summary>
        static Mesh CreateOvalCut(float size)
        {
            int sides = 24;
            float radiusX = 0.55f * size;
            float radiusZ = 0.4f * size;
            float crownH = 0.25f * size;
            float pavH = 0.5f * size;

            return CreateEllipticalCut(sides, radiusX, radiusZ, crownH, pavH, "OvalCut");
        }

        /// <summary>
        /// Simple round cut with fewer facets.
        /// </summary>
        static Mesh CreateRoundCut(float size)
        {
            int sides = 12;
            float radius = 0.45f * size;
            float crownH = 0.3f * size;
            float pavH = 0.5f * size;

            return CreateEllipticalCut(sides, radius, radius, crownH, pavH, "RoundCut");
        }

        /// <summary>
        /// Pear (teardrop) shape.
        /// </summary>
        static Mesh CreatePearCut(float size)
        {
            int sides = 24;
            float crownH = 0.25f * size;
            float pavH = 0.5f * size;

            var builder = new MeshBuilder();

            // Generate pear outline
            Vector3[] GenerateRing(float y, float scale)
            {
                Vector3[] ring = new Vector3[sides];
                for (int i = 0; i < sides; i++)
                {
                    float t = (float)i / sides;
                    float angle = t * Mathf.PI * 2f;
                    // Pear shape: wider at bottom, narrow at top
                    float r = (0.35f + 0.15f * Mathf.Cos(angle)) * size * scale;
                    float x = Mathf.Sin(angle) * r;
                    float z = Mathf.Cos(angle) * r * 1.3f;
                    ring[i] = new Vector3(x, y, z);
                }
                return ring;
            }

            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0));
            var tableRing = GenerateRing(crownH, 0.6f);
            var girdleRing = GenerateRing(0, 1f);

            int[] tableV = new int[sides];
            int[] girdleV = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                tableV[i] = builder.AddVertex(tableRing[i]);
                girdleV[i] = builder.AddVertex(girdleRing[i]);
            }

            // Table
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            // Crown
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], girdleV[i], tableV[next]);
                builder.AddTriangle(tableV[next], girdleV[i], girdleV[next]);
            }

            // Pavilion
            int culet = builder.AddVertex(new Vector3(0, -pavH, 0));
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(girdleV[i], culet, girdleV[(i + 1) % sides]);

            return builder.Build("PearCut");
        }

        /// <summary>
        /// Marquise (eye/navette) shaped cut.
        /// </summary>
        static Mesh CreateMarquiseCut(float size)
        {
            int sides = 24;
            float crownH = 0.2f * size;
            float pavH = 0.45f * size;

            var builder = new MeshBuilder();

            Vector3[] GenerateRing(float y, float scale)
            {
                Vector3[] ring = new Vector3[sides];
                for (int i = 0; i < sides; i++)
                {
                    float angle = (float)i / sides * Mathf.PI * 2f;
                    float rx = 0.25f * size * scale;
                    float rz = 0.55f * size * scale;
                    ring[i] = new Vector3(
                        Mathf.Cos(angle) * rx,
                        y,
                        Mathf.Sin(angle) * rz);
                }
                return ring;
            }

            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0));
            var tableRing = GenerateRing(crownH, 0.6f);
            var girdleRing = GenerateRing(0, 1f);

            int[] tableV = new int[sides];
            int[] girdleV = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                tableV[i] = builder.AddVertex(tableRing[i]);
                girdleV[i] = builder.AddVertex(girdleRing[i]);
            }

            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], girdleV[i], tableV[next]);
                builder.AddTriangle(tableV[next], girdleV[i], girdleV[next]);
            }

            int culet = builder.AddVertex(new Vector3(0, -pavH, 0));
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(girdleV[i], culet, girdleV[(i + 1) % sides]);

            return builder.Build("MarquiseCut");
        }

        /// <summary>
        /// Cushion cut - square with rounded corners.
        /// </summary>
        static Mesh CreateCushionCut(float size)
        {
            int sides = 20;
            float crownH = 0.25f * size;
            float pavH = 0.5f * size;

            var builder = new MeshBuilder();

            Vector3[] GenerateRing(float y, float scale)
            {
                Vector3[] ring = new Vector3[sides];
                for (int i = 0; i < sides; i++)
                {
                    float angle = (float)i / sides * Mathf.PI * 2f;
                    // Superellipse for rounded square
                    float n = 3.0f;
                    float ca = Mathf.Cos(angle);
                    float sa = Mathf.Sin(angle);
                    float r = Mathf.Pow(
                        Mathf.Pow(Mathf.Abs(ca), n) + Mathf.Pow(Mathf.Abs(sa), n),
                        -1f / n) * 0.45f * size * scale;
                    ring[i] = new Vector3(ca * r, y, sa * r);
                }
                return ring;
            }

            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0));
            var tableRing = GenerateRing(crownH, 0.65f);
            var girdleRing = GenerateRing(0, 1f);

            int[] tableV = new int[sides];
            int[] girdleV = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                tableV[i] = builder.AddVertex(tableRing[i]);
                girdleV[i] = builder.AddVertex(girdleRing[i]);
            }

            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], girdleV[i], tableV[next]);
                builder.AddTriangle(tableV[next], girdleV[i], girdleV[next]);
            }

            int culet = builder.AddVertex(new Vector3(0, -pavH, 0));
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(girdleV[i], culet, girdleV[(i + 1) % sides]);

            return builder.Build("CushionCut");
        }

        /// <summary>
        /// Heart-shaped cut.
        /// </summary>
        static Mesh CreateHeartCut(float size)
        {
            int sides = 32;
            float crownH = 0.2f * size;
            float pavH = 0.45f * size;

            var builder = new MeshBuilder();

            Vector3[] GenerateRing(float y, float scale)
            {
                Vector3[] ring = new Vector3[sides];
                for (int i = 0; i < sides; i++)
                {
                    float t = (float)i / sides * Mathf.PI * 2f;
                    // Heart curve parametric
                    float x = 16f * Mathf.Pow(Mathf.Sin(t), 3);
                    float z = 13f * Mathf.Cos(t)
                            - 5f * Mathf.Cos(2f * t)
                            - 2f * Mathf.Cos(3f * t)
                            - Mathf.Cos(4f * t);
                    ring[i] = new Vector3(x * 0.025f * size * scale, y, z * 0.025f * size * scale);
                }
                return ring;
            }

            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0.02f * size));
            var tableRing = GenerateRing(crownH, 0.6f);
            var girdleRing = GenerateRing(0, 1f);

            int[] tableV = new int[sides];
            int[] girdleV = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                tableV[i] = builder.AddVertex(tableRing[i]);
                girdleV[i] = builder.AddVertex(girdleRing[i]);
            }

            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], girdleV[i], tableV[next]);
                builder.AddTriangle(tableV[next], girdleV[i], girdleV[next]);
            }

            int culet = builder.AddVertex(new Vector3(0, -pavH, -0.1f * size));
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(girdleV[i], culet, girdleV[(i + 1) % sides]);

            return builder.Build("HeartCut");
        }

        /// <summary>
        /// Sphere mesh for pearls.
        /// </summary>
        static Mesh CreateSphere(float size)
        {
            int latSegments = 24;
            int lonSegments = 24;
            float radius = 0.45f * size;

            var builder = new MeshBuilder();

            // Generate vertices
            int[,] vertGrid = new int[latSegments + 1, lonSegments + 1];

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = lat * Mathf.PI / latSegments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / lonSegments;
                    float x = sinTheta * Mathf.Cos(phi) * radius;
                    float y = cosTheta * radius;
                    float z = sinTheta * Mathf.Sin(phi) * radius;

                    vertGrid[lat, lon] = builder.AddVertex(new Vector3(x, y, z));
                }
            }

            // Generate triangles
            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int a = vertGrid[lat, lon];
                    int b = vertGrid[lat, lon + 1];
                    int c = vertGrid[lat + 1, lon];
                    int d = vertGrid[lat + 1, lon + 1];

                    if (lat != 0)
                        builder.AddTriangle(a, b, c);
                    if (lat != latSegments - 1)
                        builder.AddTriangle(b, d, c);
                }
            }

            return builder.Build("Sphere");
        }

        /// <summary>
        /// Helper to create elliptical brilliant cuts (oval, round).
        /// </summary>
        static Mesh CreateEllipticalCut(int sides, float radiusX, float radiusZ,
                                         float crownH, float pavH, string name)
        {
            var builder = new MeshBuilder();
            float tableScale = 0.6f;

            int tableCenter = builder.AddVertex(new Vector3(0, crownH, 0));
            int[] tableV = new int[sides];
            int[] girdleV = new int[sides];

            for (int i = 0; i < sides; i++)
            {
                float angle = (float)i / sides * Mathf.PI * 2f;
                float cx = Mathf.Cos(angle);
                float cz = Mathf.Sin(angle);

                tableV[i] = builder.AddVertex(new Vector3(
                    cx * radiusX * tableScale, crownH, cz * radiusZ * tableScale));
                girdleV[i] = builder.AddVertex(new Vector3(
                    cx * radiusX, 0, cz * radiusZ));
            }

            // Table
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(tableCenter, tableV[i], tableV[(i + 1) % sides]);

            // Crown
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                builder.AddTriangle(tableV[i], girdleV[i], tableV[next]);
                builder.AddTriangle(tableV[next], girdleV[i], girdleV[next]);
            }

            // Pavilion
            int culet = builder.AddVertex(new Vector3(0, -pavH, 0));
            for (int i = 0; i < sides; i++)
                builder.AddTriangle(girdleV[i], culet, girdleV[(i + 1) % sides]);

            return builder.Build(name);
        }

        /// <summary>
        /// Helper class to build meshes procedurally with auto-calculated normals.
        /// </summary>
        class MeshBuilder
        {
            System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

            public int AddVertex(Vector3 position)
            {
                vertices.Add(position);
                return vertices.Count - 1;
            }

            public void AddTriangle(int a, int b, int c)
            {
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
            }

            public Mesh Build(string name)
            {
                Mesh mesh = new Mesh();
                mesh.name = name;
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }
        }
    }
}
