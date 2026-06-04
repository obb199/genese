using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Fábrica de primitivas low-poly (esfera, cilindro, cubo, cone) e materiais.
    /// Render pipeline: Built-in (shader "Standard"). Sem colliders (visual puro).
    /// </summary>
    public static class Prim
    {
        static Shader _standard;
        static Shader Standard => _standard != null ? _standard : (_standard = Shader.Find("Standard"));

        public enum Finish { Matte, Satin, Metallic, Iridescent }

        public static Material Mat(Color color, Finish finish = Finish.Matte, Color? emissive = null, float emissiveI = 0f)
        {
            var m = new Material(Standard) { color = color };
            switch (finish)
            {
                case Finish.Matte:      m.SetFloat("_Glossiness", 0.35f); m.SetFloat("_Metallic", 0.0f); break;
                case Finish.Satin:      m.SetFloat("_Glossiness", 0.6f);  m.SetFloat("_Metallic", 0.0f); break;
                case Finish.Metallic:   m.SetFloat("_Glossiness", 0.7f);  m.SetFloat("_Metallic", 0.85f); break;
                case Finish.Iridescent: m.SetFloat("_Glossiness", 0.8f);  m.SetFloat("_Metallic", 0.5f);
                    emissive ??= color; emissiveI = Mathf.Max(emissiveI, 0.25f); break;
            }
            if (emissive.HasValue && emissiveI > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emissive.Value * emissiveI);
            }
            return m;
        }

        static GameObject Make(PrimitiveType type, Transform parent, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            var col = go.GetComponent<Collider>();
            if (col) Object.Destroy(col);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (parent) go.transform.SetParent(parent, false);
            return go;
        }

        public static GameObject Sphere(Transform parent, Material mat) => Make(PrimitiveType.Sphere, parent, mat);
        public static GameObject Cylinder(Transform parent, Material mat) => Make(PrimitiveType.Cylinder, parent, mat);
        public static GameObject Cube(Transform parent, Material mat) => Make(PrimitiveType.Cube, parent, mat);
        public static GameObject Capsule(Transform parent, Material mat) => Make(PrimitiveType.Capsule, parent, mat);

        // Rocha/forma orgânica: esfera com vértices deslocados por ruído (irregular, natural).
        public static GameObject Rock(Transform parent, Material mat, float lumps = 0.5f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var col = go.GetComponent<Collider>(); if (col) Object.Destroy(col);
            var mf = go.GetComponent<MeshFilter>();
            var mesh = Object.Instantiate(mf.sharedMesh);
            var v = mesh.vertices;
            float sx = Random.Range(0f, 100f), sy = Random.Range(0f, 100f);
            for (int i = 0; i < v.Length; i++)
            {
                var n = v[i].normalized;
                float j = 1f + (Mathf.PerlinNoise(n.x * 2.5f + sx, n.z * 2.5f + sy) - 0.5f) * lumps
                            + (Mathf.PerlinNoise(n.y * 4f + sy, n.x * 4f + sx) - 0.5f) * lumps * 0.5f;
                v[i] = n * 0.5f * j;
            }
            mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            mf.mesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (parent) go.transform.SetParent(parent, false);
            return go;
        }

        // Cone procedural (não existe primitiva nativa). Ápice em +Y, base em y=0.
        public static GameObject Cone(Transform parent, Material mat, float radius = 0.5f, float height = 1f, int seg = 10)
        {
            var go = new GameObject("Cone");
            if (parent) go.transform.SetParent(parent, false);
            var mf = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            mf.sharedMesh = ConeMesh(radius, height, seg);
            return go;
        }

        public static Mesh ConeMesh(float radius, float height, int seg)
        {
            var verts = new Vector3[seg + 2];
            var tris = new int[seg * 6];
            verts[0] = new Vector3(0, height, 0);           // ápice
            verts[seg + 1] = Vector3.zero;                  // centro da base
            for (int i = 0; i < seg; i++)
            {
                float a = (float)i / seg * Mathf.PI * 2f;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            }
            int t = 0;
            for (int i = 0; i < seg; i++)
            {
                int a = i + 1, b = (i + 1) % seg + 1;
                tris[t++] = 0; tris[t++] = b; tris[t++] = a;          // lateral
                tris[t++] = seg + 1; tris[t++] = a; tris[t++] = b;    // base
            }
            var mesh = new Mesh { name = "ConeMesh" };
            mesh.vertices = verts; mesh.triangles = tris;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
