using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Diorama procedural por bioma: terreno em malha com Perlin (colinas),
    /// MONTANHAS (baixa frequência) e RIO (canal sinuoso) + plano de ÁGUA.
    /// Cada bioma tem árvores, rochas, grama e props extras próprios; a cultura
    /// adiciona bandeiras e acentos. Generate(bioma, cultura) reconstrói tudo.
    /// </summary>
    public class WorldBuilder : MonoBehaviour
    {
        public static WorldBuilder Instance { get; private set; }

        const float R = 28f;                 // mapa maior
        public float Radius => R;
        public float WaterLevel => -0.6f;

        public Transform Ground { get; private set; }
        public Transform Flame { get; private set; }
        public Transform Heart { get; private set; }
        public Material FlameMat { get; private set; }

        Transform env;
        Material windowMat, doorMat;

        const float NoiseScale = 0.06f;
        Vector2 noiseOff;
        bool riverOn;
        float riverAmp, riverFreq, riverAmp2, riverFreq2, riverPhase;
        const float RiverWidth = 3.6f, RiverDepth = 3.4f;

        static readonly string[] FlowerCols = { "#E86A8A", "#E0C46A", "#9579B6", "#5FA9B0", "#E8A24A", "#D292B6" };

        void Awake() { Instance = this; }

        public float GroundHeight(float x, float z)
        {
            float r = Mathf.Sqrt(x * x + z * z);
            float core = Mathf.Clamp01(1f - Mathf.InverseLerp(R * 0.66f, R * 1.02f, r));
            float flat = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(3f, 8f, r)); // centro plano p/ aldeia

            float n = 0, f = NoiseScale, a = 1f, sum = 0f;
            for (int o = 0; o < 4; o++) { n += Mathf.PerlinNoise((x + noiseOff.x) * f, (z + noiseOff.y) * f) * a; sum += a; f *= 2.1f; a *= 0.5f; }
            float hills = (n / sum - 0.5f) * 2f * 1.6f;

            float m = Mathf.PerlinNoise((x + noiseOff.x) * 0.024f + 11f, (z + noiseOff.y) * 0.024f + 11f);
            float mountains = m * m * 7.5f;

            float h = (hills + mountains) * core * flat;
            if (riverOn)
            {
                float rz = riverAmp * Mathf.Sin(x * riverFreq + riverPhase) + riverAmp2 * Mathf.Sin(x * riverFreq2 + 1.7f);
                float channel = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0f, RiverWidth, Mathf.Abs(z - rz)));
                h -= channel * RiverDepth * flat;   // rio cava o vale (quando presente)
            }
            h -= (1f - core) * 4f;                  // borda desce (ilha)
            return h;
        }

        // ----------------------------------------------------------------------
        public void Generate(string biomeKey, string cultureKey)
        {
            if (env != null) Destroy(env.gameObject);
            env = new GameObject("Env").transform; env.SetParent(transform, false);
            Obstacles.Clear();
            noiseOff = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
            riverOn = Random.value < 0.4f;   // rio é um bônus (~40%), não regra
            riverAmp = R * 0.22f; riverFreq = Random.Range(0.06f, 0.1f); riverAmp2 = R * 0.1f; riverFreq2 = Random.Range(0.14f, 0.22f); riverPhase = Random.Range(0f, 6.28f);

            windowMat = Prim.Mat(Palette.Hex("#F0D9A0"), Prim.Finish.Satin, Palette.Hex("#F0D9A0"), 0.25f);
            doorMat = Prim.Mat(Palette.Hex("#3a2a22"));

            var b = WorldData.Biomes[biomeKey];
            var pr = WorldData.Profiles[biomeKey];
            var c = WorldData.Cultures.TryGetValue(cultureKey, out var cc) ? cc : WorldData.Cultures["floresta"];

            Terrain(b);
            Water(pr, biomeKey == "vulcanico");
            if (pr.Grass) Grass(b);
            for (int i = 0; i < pr.Trees; i++) { var (x, z) = ScatterLand(7f, R - 3f); TreeOf(pr, b, x, z, Random.Range(0.7f, 1.8f)); }
            for (int i = 0; i < pr.Rocks; i++) { var (x, z) = ScatterLand(5f, R - 2.5f); RockOf(pr, b, x, z, Random.Range(0.7f, 1.6f)); }
            Extras(pr, b);
            Landmark(biomeKey, b, pr);
            Village(c, cultureKey);
        }

        (float, float) Scatter(float min, float max) { float a = Random.value * 6.28f, r = Random.Range(min, max); return (Mathf.Cos(a) * r, Mathf.Sin(a) * r); }
        (float, float) ScatterLand(float min, float max)
        {
            for (int i = 0; i < 12; i++) { var p = Scatter(min, max); if (GroundHeight(p.Item1, p.Item2) > WaterLevel + 0.25f) return p; }
            return Scatter(min, max);
        }

        // ---- terreno + água ---------------------------------------------------
        void Terrain(Biome b)
        {
            const int G = 150; float ext = R + 3f, step = ext * 2f / G;
            int side = G + 1;
            var verts = new Vector3[side * side];
            var tris = new int[G * G * 6];
            for (int j = 0; j <= G; j++)
                for (int i = 0; i <= G; i++)
                {
                    float x = -ext + i * step, z = -ext + j * step;
                    verts[j * side + i] = new Vector3(x, GroundHeight(x, z), z);
                }
            int t = 0;
            for (int j = 0; j < G; j++)
                for (int i = 0; i < G; i++)
                {
                    int a = j * side + i, c = a + side;
                    tris[t++] = a; tris[t++] = c; tris[t++] = a + 1;
                    tris[t++] = a + 1; tris[t++] = c; tris[t++] = c + 1;
                }
            var mesh = new Mesh { name = "Terreno", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.vertices = verts; mesh.triangles = tris;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();

            var go = new GameObject("Terreno"); go.transform.SetParent(env, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Prim.Mat(b.Ground);
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            Ground = go.transform;
        }

        void Water(BiomeProfile pr, bool lava)
        {
            var sh = Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
            var col = pr.Water; col.a = lava ? 0.95f : 0.66f;
            var m = new Material(sh) { color = col };
            if (lava) { m.SetColor("_EmissionColor", pr.Water); m.EnableKeyword("_EMISSION"); }
            var w = Prim.Cylinder(env, m);
            w.transform.localScale = new Vector3(R * 2.05f, 0.02f, R * 2.05f);
            w.transform.localPosition = new Vector3(0, WaterLevel, 0);
            var col2 = w.GetComponent<Collider>(); if (col2) Destroy(col2);
        }

        void Grass(Biome b)
        {
            var mat = Prim.Mat(b.Foliage * 1.05f);
            for (int i = 0; i < 90; i++)
            {
                var (x, z) = ScatterLand(2f, R - 1.5f);
                var tuft = Prim.Cone(env, mat, Random.Range(0.05f, 0.09f), Random.Range(0.25f, 0.5f), 4);
                tuft.transform.localPosition = new Vector3(x, GroundHeight(x, z), z);
                tuft.transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            }
        }

        // ---- props extras por bioma ------------------------------------------
        void Extras(BiomeProfile pr, Biome b)
        {
            if (string.IsNullOrEmpty(pr.Extras)) return;
            var kinds = pr.Extras.Split(',');
            for (int i = 0; i < pr.ExtraCount; i++)
            {
                var k = kinds[Random.Range(0, kinds.Length)];
                bool water = k == "lilypad" || k == "reed";
                var (x, z) = water ? Scatter(4f, R - 2f) : ScatterLand(3f, R - 2f);
                float y = GroundHeight(x, z);
                if (water) y = Mathf.Max(y, WaterLevel + 0.02f);
                ExtraOf(k, b, x, z, y, Random.Range(0.8f, 1.3f));
            }
        }

        void ExtraOf(string k, Biome b, float x, float z, float y, float s)
        {
            var root = new GameObject(k).transform; root.SetParent(env, false);
            root.localPosition = new Vector3(x, y, z); root.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            switch (k)
            {
                case "flower":
                    Cyl(root, Prim.Mat(b.Foliage), 0.03f * s, 0.22f * s, 0.22f * s);
                    var fl = Prim.Sphere(root, Prim.Mat(Palette.Hex(FlowerCols[Random.Range(0, FlowerCols.Length)]), Prim.Finish.Satin));
                    fl.transform.localScale = Vector3.one * 0.18f * s; fl.transform.localPosition = new Vector3(0, 0.48f * s, 0);
                    break;
                case "bush":
                    for (int i = 0; i < 3; i++) { var sp = Prim.Sphere(root, Prim.Mat(b.Foliage)); float rr = Random.Range(0.2f, 0.32f) * s; sp.transform.localScale = Vector3.one * rr * 2; sp.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f) * s, rr * 0.8f, Random.Range(-0.2f, 0.2f) * s); }
                    break;
                case "mushroom":
                    Cyl(root, Prim.Mat(Palette.Hex("#E8E0CC")), 0.06f * s, 0.16f * s, 0.16f * s);
                    var cap = Prim.Sphere(root, Prim.Mat(Palette.Hex(Random.value < 0.5f ? "#C0563A" : "#B58A5A")));
                    cap.transform.localScale = new Vector3(0.34f * s, 0.2f * s, 0.34f * s); cap.transform.localPosition = new Vector3(0, 0.34f * s, 0);
                    break;
                case "log":
                    var lg = Prim.Cylinder(root, Prim.Mat(Palette.Hex("#5a4630")));
                    lg.transform.localScale = new Vector3(0.18f * s, 0.6f * s, 0.18f * s); lg.transform.localPosition = new Vector3(0, 0.18f * s, 0);
                    lg.transform.localRotation = Quaternion.Euler(90, Random.value * 360, 0);
                    break;
                case "fern":
                    for (int i = 0; i < 4; i++) { var c = Prim.Cone(root, Prim.Mat(b.Foliage), 0.06f * s, 0.4f * s, 5); c.transform.localPosition = new Vector3(0, 0.1f * s, 0); c.transform.localRotation = Quaternion.Euler(35, i * 90, 0); }
                    break;
                case "bone":
                    var skull = Prim.Sphere(root, Prim.Mat(Palette.Hex("#E6E0CE"))); skull.transform.localScale = Vector3.one * 0.28f * s; skull.transform.localPosition = new Vector3(0, 0.14f * s, 0);
                    for (int i = 0; i < 2; i++) { var rb = Prim.Cylinder(root, Prim.Mat(Palette.Hex("#D8D0BC"))); rb.transform.localScale = new Vector3(0.04f * s, 0.3f * s, 0.04f * s); rb.transform.localPosition = new Vector3(0.12f * (i == 0 ? 1 : -1) * s, 0.06f * s, 0); rb.transform.localRotation = Quaternion.Euler(0, 0, 70); }
                    break;
                case "reed":
                    for (int i = 0; i < Random.Range(4, 7); i++) { var rd = Prim.Cone(root, Prim.Mat(b.Foliage), 0.04f * s, Random.Range(0.6f, 1.0f) * s, 4); rd.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f) * s, 0, Random.Range(-0.2f, 0.2f) * s); }
                    break;
                case "lilypad":
                    var pad = Prim.Cylinder(root, Prim.Mat(b.Foliage * 1.1f)); pad.transform.localScale = new Vector3(0.5f * s, 0.02f, 0.5f * s);
                    break;
                case "iceChunk":
                    for (int i = 0; i < Random.Range(2, 4); i++) { var ic = Prim.Cone(root, Prim.Mat(Palette.Hex("#CFEAF2"), Prim.Finish.Metallic), 0.14f * s, Random.Range(0.3f, 0.6f) * s, 4); ic.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f) * s, 0, Random.Range(-0.2f, 0.2f) * s); ic.transform.localRotation = Quaternion.Euler(Random.Range(-15, 15), Random.value * 360, Random.Range(-15, 15)); }
                    break;
                case "lavaRock":
                    var lr = Prim.Cube(root, Prim.Mat(Palette.Hex("#2A2220"))); lr.transform.localScale = Vector3.one * Random.Range(0.4f, 0.7f) * s; lr.transform.localPosition = new Vector3(0, 0.2f * s, 0); lr.transform.localRotation = Random.rotation;
                    var glow = Prim.Cube(root, Prim.Mat(Palette.Hex("#E0552A"), Prim.Finish.Satin, Palette.Hex("#E0552A"), 1.3f)); glow.transform.localScale = Vector3.one * 0.18f * s; glow.transform.localPosition = new Vector3(0, 0.34f * s, 0);
                    break;
                default: // shrub
                    for (int i = 0; i < 2; i++) { var c = Prim.Cone(root, Prim.Mat(b.Foliage * 0.9f), 0.12f * s, 0.3f * s, 5); c.transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f) * s, 0, 0); }
                    break;
            }
        }

        // ---- árvores por tipo -------------------------------------------------
        void TreeOf(BiomeProfile pr, Biome b, float x, float z, float s)
        {
            var root = new GameObject("Arvore").transform; root.SetParent(env, false);
            root.localPosition = new Vector3(x, GroundHeight(x, z), z); root.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            var trunkMat = Prim.Mat(pr.Trunk);
            Color Fol() { Color.RGBToHSV(b.Foliage, out var h, out var sv, out var v); return Color.HSVToRGB(h, sv, Mathf.Clamp01(v + Random.Range(-0.06f, 0.06f))); }

            switch (pr.Tree)
            {
                case "conifer":
                    Cyl(root, trunkMat, 0.16f * s, 0.6f * s, 0.5f * s);
                    for (int i = 0; i < 3; i++) { var cone = Prim.Cone(root, Prim.Mat(Fol()), (0.72f - i * 0.18f) * s, 0.85f * s, 7); cone.transform.localPosition = new Vector3(0, (0.7f + i * 0.55f) * s, 0); }
                    break;
                case "cactus": Cactus(root, Prim.Mat(b.Foliage), s); break;
                case "dead":
                    Cyl(root, trunkMat, 0.13f * s, 0.7f * s, 0.7f * s);
                    for (int i = 0; i < Random.Range(3, 6); i++) { var br = Prim.Cylinder(root, trunkMat); br.transform.localScale = new Vector3(0.05f * s, Random.Range(0.25f, 0.5f) * s, 0.05f * s); br.transform.localPosition = new Vector3(0, Random.Range(0.9f, 1.5f) * s, 0); br.transform.localRotation = Quaternion.Euler(Random.Range(20, 70), Random.value * 360, 0); }
                    break;
                default:
                    Cyl(root, trunkMat, 0.16f * s, 0.5f * s, 0.5f * s);
                    for (int i = 0; i < 3; i++) { var bl = Prim.Sphere(root, Prim.Mat(Fol())); float rad = Random.Range(0.5f, 0.8f) * s; bl.transform.localScale = Vector3.one * (rad * 2); bl.transform.localPosition = new Vector3(Random.Range(-0.25f, 0.25f) * s, (1.0f + i * 0.45f) * s, Random.Range(-0.25f, 0.25f) * s); }
                    break;
            }
            Obstacles.Add(x, z, 0.42f * s);
        }

        void Cactus(Transform root, Material mat, float s)
        {
            var body = Prim.Capsule(root, mat); body.transform.localScale = new Vector3(0.58f * s, 0.9f * s, 0.58f * s); body.transform.localPosition = new Vector3(0, 0.9f * s, 0);
            for (int sx = -1; sx <= 1; sx += 2)
                if (Random.value < 0.85f)
                {
                    float ay = Random.Range(0.7f, 1.0f) * s;
                    var arm = Prim.Capsule(root, mat); arm.transform.localScale = new Vector3(0.3f * s, 0.42f * s, 0.3f * s); arm.transform.localPosition = new Vector3(0.33f * sx * s, ay, 0); arm.transform.localRotation = Quaternion.Euler(0, 0, sx * 52);
                    var tip = Prim.Capsule(root, mat); tip.transform.localScale = new Vector3(0.28f * s, 0.34f * s, 0.28f * s); tip.transform.localPosition = new Vector3(0.52f * sx * s, ay + 0.34f * s, 0);
                }
        }

        // ---- rochas por tipo --------------------------------------------------
        void RockOf(BiomeProfile pr, Biome b, float x, float z, float s)
        {
            var root = new GameObject("Rocha").transform; root.SetParent(env, false);
            root.localPosition = new Vector3(x, GroundHeight(x, z), z); root.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            float radius;
            if (pr.Rock == "crystal")
            {
                var col = pr.Special == default ? Palette.Hex("#9bd0e0") : pr.Special;
                var mat = Prim.Mat(col, Prim.Finish.Satin, col, 0.5f);
                for (int i = 0; i < Random.Range(3, 6); i++) { var shd = Prim.Cone(root, mat, Random.Range(0.1f, 0.22f) * s, Random.Range(0.5f, 1.2f) * s, 4); shd.transform.localPosition = new Vector3(Random.Range(-0.32f, 0.32f) * s, 0, Random.Range(-0.32f, 0.32f) * s); shd.transform.localRotation = Quaternion.Euler(Random.Range(-14, 14), Random.value * 360, Random.Range(-14, 14)); }
                radius = 0.45f * s;
            }
            else if (pr.Rock == "slab")
            {
                // rocha chata e arredondada de deserto (meio enterrada)
                var r = Prim.Rock(root, Prim.Mat(b.Accent), 0.45f);
                r.transform.localScale = new Vector3(Random.Range(1.3f, 2.1f) * s, Random.Range(0.4f, 0.6f) * s, Random.Range(1.1f, 1.8f) * s);
                r.transform.localPosition = new Vector3(0, 0.12f * s, 0);
                radius = 0.7f * s;
            }
            else // boulder: 1–3 lascas arredondadas e irregulares, meio enterradas
            {
                int lumps = Random.Range(1, 4);
                for (int i = 0; i < lumps; i++)
                {
                    var mat = Prim.Mat(Palette.Hex("#8A8377") * (0.82f + Random.value * 0.3f));
                    var r = Prim.Rock(root, mat, Random.Range(0.45f, 0.7f));
                    float rad = Random.Range(0.5f, 0.85f) * s * (i == 0 ? 1f : 0.7f);
                    r.transform.localScale = new Vector3(rad * Random.Range(1.0f, 1.3f), rad * Random.Range(0.7f, 0.95f), rad * Random.Range(1.0f, 1.3f));
                    r.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f) * s, rad * 0.3f, Random.Range(-0.4f, 0.4f) * s);
                    r.transform.localRotation = Quaternion.Euler(0, Random.value * 360, Random.Range(-8, 8));
                }
                radius = 0.62f * s;
            }
            Obstacles.Add(x, z, radius);
        }

        // ---- aldeia (cores + acentos + monumento da cultura) -----------------
        void Village(Culture c, string cultureKey)
        {
            var g = new GameObject("Aldeia").transform; g.SetParent(env, false);
            g.localPosition = new Vector3(0, GroundHeight(0, 0), 0);
            // anel de pedras arredondadas (acento cultural alternado)
            for (int i = 0; i < 10; i++)
            {
                float a = (float)i / 10 * 6.28f;
                var st = Prim.Rock(g, Prim.Mat(i % 2 == 0 ? Palette.Hex("#7a7468") : c.Accent), 0.4f);
                st.transform.localScale = new Vector3(0.36f, 0.18f, 0.36f); st.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.95f, 0.05f, Mathf.Sin(a) * 0.95f);
            }
            FlameMat = Prim.Mat(c.Flame, Prim.Finish.Satin, c.Flame, 1.4f);
            var flame = Prim.Cone(g, FlameMat, 0.24f, 0.8f, 12); flame.transform.localPosition = new Vector3(0, 0.16f, 0); Flame = flame.transform;
            var heart = Prim.Cube(g, Prim.Mat(Palette.PedraCoracao, Prim.Finish.Satin, Palette.PedraCoracao, 1.1f)); heart.transform.localScale = Vector3.one * 0.32f; heart.transform.localPosition = new Vector3(0, 1.4f, 0); heart.transform.localRotation = Quaternion.Euler(45, 0, 45); Heart = heart.transform;
            Obstacles.Add(0, 0, 1.0f);

            Vector2[] spots = { new Vector2(-4.4f, 2.8f), new Vector2(4.6f, 2.2f), new Vector2(1.2f, -4.8f), new Vector2(-4.8f, -2.4f), new Vector2(4.8f, -3.4f) };
            foreach (var sp in spots) Hut(g, c, sp.x, sp.y, Random.Range(0f, 360f), Random.Range(0.95f, 1.3f));

            // caminho de pedras do centro até a primeira casa
            for (float tt = 0.28f; tt < 0.95f; tt += 0.16f) { var p = spots[0] * tt; var st = Prim.Rock(g, Prim.Mat(Palette.Hex("#9a9488")), 0.3f); st.transform.localScale = new Vector3(0.45f, 0.1f, 0.45f); st.transform.localPosition = new Vector3(p.x, 0.04f, p.y); }

            var totem = Prim.Cylinder(g, Prim.Mat(c.Accent, Prim.Finish.Satin)); totem.transform.localScale = new Vector3(0.36f, 0.9f, 0.36f); totem.transform.localPosition = new Vector3(-3.2f, 0.9f, 1.2f);
            var cap = Prim.Cone(g, Prim.Mat(c.Accent, Prim.Finish.Satin, c.Accent, 0.4f), 0.3f, 0.5f, 6); cap.transform.localPosition = new Vector3(-3.2f, 1.85f, 1.2f);
            Obstacles.Add(-3.2f, 1.2f, 0.4f);

            Monument(g, c, cultureKey, new Vector3(3.4f, 0, -3.6f));
        }

        // monumento central característico de cada cultura
        void Monument(Transform g, Culture c, string key, Vector3 pos)
        {
            var root = new GameObject("Monumento").transform; root.SetParent(g, false); root.localPosition = pos;
            var wall = Prim.Mat(c.Wall); var acc = Prim.Mat(c.Accent, Prim.Finish.Satin);
            switch (key)
            {
                case "ziggurat":
                    for (int i = 0; i < 3; i++) Box(root, wall, new Vector3(2.2f - i * 0.6f, 0.5f, 2.2f - i * 0.6f), new Vector3(0, 0.25f + i * 0.5f, 0));
                    Box(root, acc, new Vector3(0.7f, 0.5f, 0.7f), new Vector3(0, 1.75f, 0));
                    break;
                case "torre":
                    Cyl(root, wall, 0.6f, 1.7f, 1.7f);
                    for (int i = 0; i < 8; i++) { float a = i / 8f * 6.28f; Box(root, wall, new Vector3(0.22f, 0.34f, 0.22f), new Vector3(Mathf.Cos(a) * 0.6f, 3.4f, Mathf.Sin(a) * 0.6f)); }
                    Roof(root, acc, 0.75f, 1.1f, 3.4f);
                    break;
                case "cristal":
                    Cyl(root, wall, 0.45f, 0.35f, 0.35f);
                    var up = Prim.Cone(root, Prim.Mat(c.Accent, Prim.Finish.Satin, c.Accent, 0.9f), 0.5f, 1.1f, 4); up.transform.localPosition = new Vector3(0, 2.3f, 0);
                    var dn = Prim.Cone(root, Prim.Mat(c.Accent, Prim.Finish.Satin, c.Accent, 0.9f), 0.5f, 1.1f, 4); dn.transform.localPosition = new Vector3(0, 2.3f, 0); dn.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    break;
                case "obelisco":
                    var ob = Prim.Cube(root, wall); ob.transform.localScale = new Vector3(0.7f, 3.6f, 0.7f); ob.transform.localPosition = new Vector3(0, 1.8f, 0);
                    Roof(root, acc, 0.5f, 0.8f, 3.6f);
                    break;
                case "antena":
                    Cyl(root, wall, 0.18f, 1.9f, 1.9f);
                    var ball = Prim.Sphere(root, Prim.Mat(c.Accent, Prim.Finish.Satin, c.Accent, 0.9f)); ball.transform.localScale = Vector3.one * 0.6f; ball.transform.localPosition = new Vector3(0, 3.9f, 0);
                    break;
                default: // totens
                    for (int i = 0; i < 3; i++) { var pst = Prim.Cylinder(root, Prim.Mat(i % 2 == 0 ? c.Wall : c.Accent)); pst.transform.localScale = new Vector3(0.46f, 0.7f + i * 0.12f, 0.46f); pst.transform.localPosition = new Vector3((i - 1) * 0.85f, 0.7f + i * 0.12f, 0); }
                    break;
            }
            Obstacles.Add(pos.x, pos.z, 1.1f);
        }

        void Hut(Transform parent, Culture c, float x, float z, float rot, float s)
        {
            var root = new GameObject("Casa").transform; root.SetParent(parent, false);
            root.localPosition = new Vector3(x, 0, z); root.localRotation = Quaternion.Euler(0, rot, 0); root.localScale = Vector3.one * s;
            var wallMat = Prim.Mat(c.Wall); var roofMat = Prim.Mat(c.Roof);

            float w0 = Random.Range(1.6f, 2.4f), d0 = Random.Range(1.6f, 2.4f), fh0 = Random.Range(1.2f, 1.9f);
            bool two = Random.value < 0.45f, win = Random.value < 0.9f;

            Box(root, wallMat, new Vector3(w0, fh0, d0), new Vector3(0, fh0 / 2, 0));
            if (win) WindowsRow(root, w0, d0, fh0, fh0 * 0.55f);
            float dh = Mathf.Min(0.95f, fh0 * 0.6f);
            Box(root, doorMat, new Vector3(0.5f, dh, 0.1f), new Vector3(0, dh / 2 + 0.02f, d0 / 2 + 0.03f));

            float topY = fh0, topW = w0, topD = d0;
            if (two) { float w1 = w0 * 0.9f, d1 = d0 * 0.9f, fh1 = fh0 * 0.78f; Box(root, wallMat, new Vector3(w1, fh1, d1), new Vector3(0, fh0 + fh1 / 2, 0)); if (win) WindowsRow(root, w1, d1, fh1, fh0 + fh1 * 0.5f); topY = fh0 + fh1; topW = w1; topD = d1; }

            int style = Random.value < 0.65f ? c.RoofStyle : Random.Range(0, 5);
            RoofStyle(root, roofMat, c, topW, topD, topY, style);

            if (Random.value < 0.5f) Box(root, Prim.Mat(Palette.Hex("#6b5a4a")), new Vector3(0.26f, 0.7f, 0.26f), new Vector3(topW * 0.3f, topY + 0.55f, topD * 0.22f)); // chaminé

            if (Random.value < 0.6f) // bandeira cultural
            {
                float top = topY + 1.1f;
                Cyl(root, Prim.Mat(Palette.Hex("#6E5638")), 0.03f, 0.5f, top + 0.5f);
                Box(root, Prim.Mat(c.Accent, Prim.Finish.Satin), new Vector3(0.42f, 0.28f, 0.03f), new Vector3(0.24f, top + 0.78f, 0));
            }

            Obstacles.Add(x, z, Mathf.Max(w0, d0) * 0.5f * s + 0.25f);
        }

        // telhados: 0 pirâmide · 1 hip · 2 laje/parapeito · 3 duas-águas · 4 cúpula
        void RoofStyle(Transform root, Material mat, Culture c, float topW, float topD, float topY, int style)
        {
            float half = Mathf.Max(topW, topD) * 0.5f;
            switch (style)
            {
                case 1:
                    Roof(root, mat, half + 0.22f, Random.Range(0.55f, 0.75f), topY);
                    break;
                case 2: // laje com parapeito
                    Box(root, mat, new Vector3(topW + 0.18f, 0.16f, topD + 0.18f), new Vector3(0, topY + 0.08f, 0));
                    float ph = 0.26f, ew = topW + 0.22f, ed = topD + 0.22f;
                    Box(root, mat, new Vector3(ew, ph, 0.12f), new Vector3(0, topY + 0.16f + ph / 2, ed / 2));
                    Box(root, mat, new Vector3(ew, ph, 0.12f), new Vector3(0, topY + 0.16f + ph / 2, -ed / 2));
                    Box(root, mat, new Vector3(0.12f, ph, ed), new Vector3(ew / 2, topY + 0.16f + ph / 2, 0));
                    Box(root, mat, new Vector3(0.12f, ph, ed), new Vector3(-ew / 2, topY + 0.16f + ph / 2, 0));
                    break;
                case 3: // duas águas (cumeeira ao longo de X)
                    float rh = Random.Range(0.7f, 1.0f);
                    float slope = Mathf.Sqrt(half * half + rh * rh) + 0.15f;
                    float ang = Mathf.Atan2(rh, half) * Mathf.Rad2Deg;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var plane = Prim.Cube(root, mat);
                        plane.transform.localScale = new Vector3(topW + 0.25f, 0.1f, slope);
                        plane.transform.localPosition = new Vector3(0, topY + rh * 0.5f, side * topD * 0.26f);
                        plane.transform.localRotation = Quaternion.Euler(side * ang, 0, 0);
                    }
                    break;
                case 4: // cúpula
                    var dome = Prim.Sphere(root, mat);
                    dome.transform.localScale = new Vector3(topW * 1.06f, Mathf.Max(topW, topD) * 0.62f, topD * 1.06f);
                    dome.transform.localPosition = new Vector3(0, topY, 0);
                    break;
                default: // pirâmide
                    Roof(root, mat, half + 0.14f, Random.Range(0.95f, 1.35f), topY);
                    break;
            }
        }

        void WindowsRow(Transform parent, float w, float d, float fh, float y)
        {
            int nf = w > 1.9f ? 2 : 1;
            for (int i = 0; i < nf; i++) { float ox = nf == 1 ? 0 : (i == 0 ? -w * 0.24f : w * 0.24f); Box(parent, windowMat, new Vector3(0.34f, 0.36f, 0.06f), new Vector3(ox, y, d / 2 + 0.02f)); }
            if (Random.value < 0.7f) Box(parent, windowMat, new Vector3(0.06f, 0.34f, 0.34f), new Vector3(w / 2 + 0.02f, y, 0));
            if (Random.value < 0.7f) Box(parent, windowMat, new Vector3(0.06f, 0.34f, 0.34f), new Vector3(-w / 2 - 0.02f, y, 0));
        }

        // ---- marco característico do bioma -----------------------------------
        void Landmark(string biomeKey, Biome b, BiomeProfile pr)
        {
            var (x, z) = ScatterLand(R * 0.45f, R * 0.7f);
            var root = new GameObject("Marco").transform; root.SetParent(env, false);
            root.localPosition = new Vector3(x, GroundHeight(x, z) - 0.2f, z);
            root.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            switch (biomeKey)
            {
                case "vulcanico": Volcano(root); Obstacles.Add(x, z, 5.5f); break;
                case "tundra": Iceberg(root); Obstacles.Add(x, z, 2.8f); break;
                case "deserto": Pyramid(root, b); Obstacles.Add(x, z, 2.8f); break;
                case "floresta": GiantTree(root, b, pr); Obstacles.Add(x, z, 1.6f); break;
                case "agua": Arch(root, b); Obstacles.Add(x, z, 2.2f); break;
                case "montanha": BigPeak(root); Obstacles.Add(x, z, 3f); break;
                case "pantano": GiantWillow(root, b, pr); Obstacles.Add(x, z, 1.8f); break;
                default: GiantTree(root, b, pr); Obstacles.Add(x, z, 1.6f); break;
            }
        }

        void Volcano(Transform root)
        {
            var dark = Prim.Mat(Palette.Hex("#3A2A28"));
            var cone = Prim.Cone(root, dark, 5f, 7f, 20); cone.transform.localPosition = Vector3.zero;
            var lava = Prim.Mat(Palette.Hex("#E0552A"), Prim.Finish.Satin, Palette.Hex("#E0552A"), 1.5f);
            var crater = Prim.Cylinder(root, lava); crater.transform.localScale = new Vector3(1.7f, 0.25f, 1.7f); crater.transform.localPosition = new Vector3(0, 6.7f, 0);
            for (int i = 0; i < 5; i++) { float a = i / 5f * 6.28f; var st = Prim.Cube(root, lava); st.transform.localScale = new Vector3(0.22f, 2.6f, 0.22f); st.transform.localPosition = new Vector3(Mathf.Cos(a) * 1.7f, 4.3f, Mathf.Sin(a) * 1.7f); st.transform.localRotation = Quaternion.Euler(16, a * Mathf.Rad2Deg, 0); }
            var lgo = new GameObject("LavaLight").transform; lgo.SetParent(root, false); lgo.localPosition = new Vector3(0, 6.6f, 0);
            var l = lgo.gameObject.AddComponent<Light>(); l.type = LightType.Point; l.color = Palette.Hex("#E0552A"); l.range = 16f; l.intensity = 3.2f;
            for (int i = 0; i < 3; i++) { var sm = Prim.Sphere(root, Prim.Mat(Palette.Hex("#6b6360"))); sm.transform.localScale = Vector3.one * Random.Range(1f, 1.7f); sm.transform.localPosition = new Vector3(Random.Range(-1f, 1f), 7.6f + i * 1.3f, Random.Range(-1f, 1f)); }
        }

        void Iceberg(Transform root)
        {
            for (int i = 0; i < 5; i++) { var ic = Prim.Cone(root, Prim.Mat(Palette.Hex("#CFEAF2"), Prim.Finish.Metallic), Random.Range(0.8f, 1.7f), Random.Range(2.2f, 4.5f), 4); ic.transform.localPosition = new Vector3(Random.Range(-1.4f, 1.4f), 0, Random.Range(-1.4f, 1.4f)); ic.transform.localRotation = Quaternion.Euler(Random.Range(-10, 10), Random.value * 360, Random.Range(-10, 10)); }
        }

        void Pyramid(Transform root, Biome b)
        {
            var mat = Prim.Mat(b.Accent);
            for (int i = 0; i < 5; i++) Box(root, mat, new Vector3(4.2f - i * 0.78f, 0.7f, 4.2f - i * 0.78f), new Vector3(0, 0.35f + i * 0.7f, 0));
            Box(root, Prim.Mat(Palette.Hex("#E0C46A"), Prim.Finish.Satin), new Vector3(0.8f, 0.6f, 0.8f), new Vector3(0, 3.8f, 0));
        }

        void GiantTree(Transform root, Biome b, BiomeProfile pr)
        {
            Cyl(root, Prim.Mat(pr.Trunk), 0.42f, 1.7f, 1.7f);
            if (pr.Tree == "conifer")
                for (int i = 0; i < 4; i++) { var cone = Prim.Cone(root, Prim.Mat(b.Foliage), 2.4f - i * 0.5f, 2.2f, 8); cone.transform.localPosition = new Vector3(0, 2.4f + i * 1.3f, 0); }
            else
                for (int i = 0; i < 5; i++) { var bl = Prim.Sphere(root, Prim.Mat(b.Foliage)); float rr = Random.Range(1.6f, 2.4f); bl.transform.localScale = Vector3.one * rr; bl.transform.localPosition = new Vector3(Random.Range(-0.8f, 0.8f), 3.4f + i * 0.7f, Random.Range(-0.8f, 0.8f)); }
        }

        void Arch(Transform root, Biome b)
        {
            for (int sx = -1; sx <= 1; sx += 2) { var p = Prim.Rock(root, Prim.Mat(b.Accent), 0.4f); p.transform.localScale = new Vector3(1.3f, 3.2f, 1.3f); p.transform.localPosition = new Vector3(sx * 1.7f, 1.6f, 0); }
            var top = Prim.Rock(root, Prim.Mat(b.Accent), 0.4f); top.transform.localScale = new Vector3(4.4f, 1.1f, 1.5f); top.transform.localPosition = new Vector3(0, 3.4f, 0);
        }

        void BigPeak(Transform root)
        {
            var p = Prim.Rock(root, Prim.Mat(Palette.Hex("#8A8377")), 0.7f); p.transform.localScale = new Vector3(3.2f, 5.4f, 3.2f); p.transform.localPosition = new Vector3(0, 2.2f, 0);
            var cap = Prim.Rock(root, Prim.Mat(Palette.Hex("#E6EEF2")), 0.6f); cap.transform.localScale = new Vector3(2.2f, 1.6f, 2.2f); cap.transform.localPosition = new Vector3(0, 4.4f, 0);
        }

        void GiantWillow(Transform root, Biome b, BiomeProfile pr)
        {
            Cyl(root, Prim.Mat(pr.Trunk), 0.4f, 1.6f, 1.6f);
            for (int i = 0; i < 4; i++) { var bl = Prim.Sphere(root, Prim.Mat(b.Foliage * 0.85f)); float rr = Random.Range(1.4f, 2f); bl.transform.localScale = new Vector3(rr, rr * 0.7f, rr); bl.transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), 3.2f + i * 0.5f, Random.Range(-0.7f, 0.7f)); }
            for (int i = 0; i < 8; i++) { float a = i / 8f * 6.28f; var dr = Prim.Cylinder(root, Prim.Mat(b.Foliage * 0.8f)); dr.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f); dr.transform.localPosition = new Vector3(Mathf.Cos(a) * 1.6f, 2.6f, Mathf.Sin(a) * 1.6f); }
        }

        // ---- helpers ----------------------------------------------------------
        void Cyl(Transform parent, Material mat, float radius, float halfH, float posY)
        {
            var c = Prim.Cylinder(parent, mat); c.transform.localScale = new Vector3(radius * 2, halfH, radius * 2); c.transform.localPosition = new Vector3(0, posY, 0);
        }
        void Box(Transform parent, Material mat, Vector3 scale, Vector3 pos)
        {
            var b = Prim.Cube(parent, mat); b.transform.localScale = scale; b.transform.localPosition = pos;
        }
        void Roof(Transform parent, Material mat, float radius, float height, float baseY)
        {
            var roof = Prim.Cone(parent, mat, radius, height, 4); roof.transform.localPosition = new Vector3(0, baseY, 0); roof.transform.localRotation = Quaternion.Euler(0, 45, 0);
        }
    }
}
