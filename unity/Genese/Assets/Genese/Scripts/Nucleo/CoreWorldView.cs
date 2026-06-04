using UnityEngine;
using System.Collections.Generic;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Desenha o mundo do núcleo como terreno suave com MÚLTIPLOS BIOMAS coexistentes
    /// e transições gradativas (kernel 5×5 pesado). Aldeias usam BuildingBuilder com
    /// construções em escala grande e colisores em cada estrutura.
    /// 10 biomas fantásticos do Claude Design ciclados pelo HUD.
    /// </summary>
    public class CoreWorldView : MonoBehaviour
    {
        public CoreSim sim;
        public float cellSize    = 1f;
        public float heightScale = 10f;   // terreno mais dramático
        public bool  Ready { get; private set; }

        CG.Environment _env;
        Mesh    _mesh;
        Vector3[] _verts;
        Color[]   _cols;
        float   _refresh;
        int     _builtVersion = -1;

        static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        // ================================================================
        //  BIOMAS FANTÁSTICOS
        // ================================================================
        readonly struct FantasyTheme
        {
            public readonly Color sky0, sky1, ground, ground2;
            public readonly Color trunk, foliage, foliage2, rock, accent;
            public readonly bool night;
            public FantasyTheme(string sky0, string sky1, string ground, string ground2,
                string trunk, string foliage, string foliage2, string rock, string accent, bool night=false)
            {
                this.sky0=Hex(sky0);this.sky1=Hex(sky1);this.ground=Hex(ground);this.ground2=Hex(ground2);
                this.trunk=Hex(trunk);this.foliage=Hex(foliage);this.foliage2=Hex(foliage2);
                this.rock=Hex(rock);this.accent=Hex(accent);this.night=night;
            }
        }
        static readonly FantasyTheme[] _themes = new FantasyTheme[]
        {
            new FantasyTheme("#7FCC78","#4A7A3C","#7d9a44","#5A7A30","#6E5638","#2F6F62","#56A06A","#8A8377","#5FE0C2"),
            new FantasyTheme("#2A2342","#140E22","#2C2440","#1E1830","#C7BEDE","#8C5BAA","#B274D4","#5A5270","#7DF0C8",true),
            new FantasyTheme("#86B8CC","#3E6F9A","#5E86A4","#456C8C","#9FC4D6","#5FC6D8","#9AD8E6","#7C9AB0","#E6A8D8"),
            new FantasyTheme("#3A4A54","#1C272E","#2E3A40","#222C31","#9AA6AE","#3FA89A","#5FC0B0","#646E74","#E0C46A",true),
            new FantasyTheme("#CFE6F0","#8FB6D2","#DCE8F0","#BCCCDC","#9AA6AE","#A7D8C0","#CFE6DA","#C7D4DE","#7FB6E0"),
            new FantasyTheme("#1E4A5A","#0C2630","#244A52","#193238","#7A8C90","#3C8A8A","#5FC0B0","#3E5A60","#C0563A",true),
            new FantasyTheme("#0C1A18","#04100C","#0E2620","#0A1A16","#2A4A40","#1E6A52","#39C08A","#243A36","#5FE0C2",true),
            new FantasyTheme("#2A2440","#120E20","#241E38","#19142A","#8C7AB0","#8C5BAA","#B274D4","#4A4264","#E6A8F0",true),
            new FantasyTheme("#F2CCA8","#C99AD0","#D8C4C0","#BCA8B0","#BFE0E6","#9AD0DA","#CFEAEE","#B0A8B6","#E0A8C8"),
            new FantasyTheme("#3A3052","#1A1430","#2C2444","#1E1834","#6E5A8A","#5FA0B0","#8C5BAA","#5A5274","#E0C46A",true),
            new FantasyTheme("#6A5C7E","#2C2236","#3E3448","#2E2638","#8A8296","#A89AC0","#C4B8DA","#6A6276","#7DD8E0")
        };
        // Polo esquerdo (x < W/2) usa índice 1; polo direito usa índice 2.
        // Retorna 0 = "Clássico" se índice inválido.
        int ThemeAt(int cellX)
        {
            if (sim == null || _env == null) return 0;
            int idx = cellX < _env.W / 2 ? sim.FantasyThemeIndex : sim.FantasyThemeIndex2;
            return (idx > 0 && idx < _themes.Length) ? idx : 0;
        }
        int CultureAt(int cellX)
        {
            if (sim == null) return 0;
            return cellX < (_env != null ? _env.W / 2 : 64) ? sim.CultureIndex : sim.CultureIndex2;
        }
        // Compatibilidade: tema do polo esquerdo (usado por BuildTerrain atmosfera global)
        int ActiveTheme => ThemeAt(0);

        // ================================================================
        //  CORES DE BIOMA — cada polo usa seu próprio tema fantástico
        // ================================================================
        Color BiomeBaseColor(CG.Biome b, int t)
        {
            if (t > 0) return _themes[t].ground;
            switch (b)
            {
                case CG.Biome.Oceano:   return Hex("#2E6F8C");
                case CG.Biome.Pradaria: return Hex("#7d9a44");
                case CG.Biome.Floresta: return Hex("#2F6F62");
                case CG.Biome.Deserto:  return Hex("#D8C49A");
                case CG.Biome.Tundra:   return Hex("#D6E2E8");
                case CG.Biome.Montanha: return Hex("#8A8377");
                case CG.Biome.Pantano:  return Hex("#4a5a33");
                case CG.Biome.Vulcanico:return Hex("#3A2A28");
                default: return Color.magenta;
            }
        }
        Color FoliageColor(CG.Biome b, int cellX = -1)
        {
            int t = cellX >= 0 ? ThemeAt(cellX) : ActiveTheme;
            if (t > 0) return _themes[t].foliage;
            switch (b)
            {
                case CG.Biome.Floresta: return Hex("#2F6F62");
                case CG.Biome.Pradaria: return Hex("#56A06A");
                case CG.Biome.Deserto:  return Hex("#7FA05A");
                case CG.Biome.Tundra:   return Hex("#9FB4C4");
                case CG.Biome.Pantano:  return Hex("#4a6a3a");
                case CG.Biome.Vulcanico:return Hex("#7a4a3a");
                default: return Hex("#6E8A4A");
            }
        }

        // Kernel gaussiano 5×5 — transição natural entre biomas; cada polo usa seu tema
        Color BlendedColor(int x, int y)
        {
            int t = ThemeAt(x);
            int W = _env.W, H = _env.H;
            if (t > 0)
            {
                float alt = _env.Altitude[y * W + x];
                if (alt < CG.Environment.SeaLevel) return Hex("#1A3A4A");
                Color c = Color.Lerp(_themes[t].ground2, _themes[t].ground, (alt - CG.Environment.SeaLevel) / 0.45f);
                if (alt > CG.Environment.MountainAlt) c = Color.Lerp(c, _themes[t].rock, (alt - CG.Environment.MountainAlt) / 0.28f);
                return c;
            }
            // pesos gaussianos para kernel 5×5
            float[,] w = {
                {0.01f,0.04f,0.06f,0.04f,0.01f},
                {0.04f,0.10f,0.14f,0.10f,0.04f},
                {0.06f,0.14f,0.20f,0.14f,0.06f},
                {0.04f,0.10f,0.14f,0.10f,0.04f},
                {0.01f,0.04f,0.06f,0.04f,0.01f}
            };
            Color sum = Color.black; float total = 0f;
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = Mathf.Clamp(x+dx,0,W-1), ny = Mathf.Clamp(y+dy,0,H-1);
                    float ww = w[dy+2, dx+2];
                    // usa o tema da célula vizinha para transição de cor suave entre polos
                    sum += BiomeBaseColor((CG.Biome)_env.Bioma[ny * W + nx], ThemeAt(nx)) * ww;
                    total += ww;
                }
            Color result = sum / total;
            float a = _env.Altitude[y * W + x];
            if (a > CG.Environment.MountainAlt)
                result = Color.Lerp(result, Hex("#E0E4E0"), (a - CG.Environment.MountainAlt) * 3f);
            return result;
        }

        // ================================================================
        void Update()
        {
            if (sim == null || sim.Sim == null) return;
            if (sim.WorldVersion != _builtVersion) { Rebuild(); return; }
            _refresh += Time.deltaTime;
            if (_refresh > 2.5f && _mesh != null) { _refresh = 0f; RefreshMesh(); } // refresh mais espaçado
        }

        void Rebuild()
        {
            Ready = false;
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            _env = sim.Sim.Env;
            Random.InitState(20260603 + sim.WorldVersion);
            ApplyFantasyAtmosphere();
            BuildTerrain();
            BuildWater();
            Decorate();
            MultipleVillages();   // múltiplas aldeias espalhadas pelo mapa
            Landmarks();
            _builtVersion = sim.WorldVersion;
            Ready = true;
        }

        void ApplyFantasyAtmosphere()
        {
            int t = ActiveTheme;
            if (t == 0) { RenderSettings.ambientLight = new Color(0.52f, 0.52f, 0.52f); return; }
            var theme = _themes[t];
            Color ambient = theme.night ? theme.ground * 0.55f : Color.Lerp(theme.sky1, theme.ground, 0.4f);
            ambient.a = 1f;
            RenderSettings.ambientLight = ambient;
        }

        // ================================================================
        //  TERRENO
        // ================================================================
        void BuildTerrain()
        {
            int W = _env.W, H = _env.H;
            _verts = new Vector3[W * H];
            _cols  = new Color[W * H];
            var tris = new int[(W - 1) * (H - 1) * 6];
            float ox = W * 0.5f, oz = H * 0.5f;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    _verts[i] = new Vector3((x - ox) * cellSize, _env.Altitude[i] * heightScale, (y - oz) * cellSize);
                    _cols[i]  = BlendedColor(x, y);
                }
            int tri = 0;
            for (int y = 0; y < H - 1; y++)
                for (int x = 0; x < W - 1; x++)
                { int i = y*W+x; tris[tri++]=i; tris[tri++]=i+W; tris[tri++]=i+1; tris[tri++]=i+1; tris[tri++]=i+W; tris[tri++]=i+W+1; }
            _mesh = new Mesh { name="Terreno", indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 };
            _mesh.vertices=_verts; _mesh.colors=_cols; _mesh.triangles=tris;
            _mesh.RecalculateNormals(); _mesh.RecalculateBounds();
            var go = new GameObject("Terreno");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var sh = Shader.Find("Genese/VertexColorLit") ?? Shader.Find("Standard");
            go.AddComponent<MeshRenderer>().sharedMaterial = new Material(sh);
            go.AddComponent<MeshCollider>().sharedMesh = _mesh;
            go.AddComponent<CoreTerrainTag>();
        }

        void BuildWater()
        {
            int t = ActiveTheme;
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = "Agua"; Destroy(w.GetComponent<Collider>());
            w.transform.SetParent(transform, false);
            Color waterCol = t > 0 ? Color.Lerp(_themes[t].rock, _themes[t].foliage, 0.35f) : new Color(0.18f, 0.45f, 0.58f, 1f);
            w.GetComponent<MeshRenderer>().material = Prim.Mat(waterCol, Prim.Finish.Satin);
            w.transform.localScale    = new Vector3(_env.W * cellSize, 0.18f, _env.H * cellSize);
            w.transform.localPosition = new Vector3(0, CG.Environment.SeaLevel * heightScale, 0);
        }

        void RefreshMesh()
        {
            int W = _env.W, H = _env.H; float ox = W*0.5f, oz = H*0.5f;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                { int i = y*W+x; _verts[i].y = _env.Altitude[i]*heightScale; _cols[i] = BlendedColor(x,y); }
            _mesh.vertices = _verts; _mesh.colors = _cols; _mesh.RecalculateNormals();
        }

        // ================================================================
        //  DECORAÇÃO — clássica ou fantástica
        // ================================================================
        void Decorate()
        {
            int W = _env.W, H = _env.H;
            int treesL=0, rocksL=0, fantasyL=0; // polo esquerdo
            int treesR=0, rocksR=0, fantasyR=0; // polo direito
            for (int y = 1; y < H-1; y++)
                for (int x = 1; x < W-1; x++)
                {
                    int i = y*W+x;
                    var b = (CG.Biome)_env.Bioma[i];
                    if (b == CG.Biome.Oceano || _env.Altitude[i] < CG.Environment.SeaLevel) continue;
                    var pos = WorldPos(x + Random.Range(-0.4f, 0.4f), y + Random.Range(-0.4f, 0.4f));

                    int t = ThemeAt(x);
                    bool left = x < W / 2;
                    ref int fantasy = ref (left ? ref fantasyL : ref fantasyR);
                    ref int trees   = ref (left ? ref treesL   : ref treesR);
                    ref int rocks   = ref (left ? ref rocksL   : ref rocksR);

                    if (t > 0)
                    { if (fantasy < 150 && Random.value < 0.02f) { FantasyProp(pos, t); fantasy++; } }
                    else
                    {
                        if (trees < 180 && Random.value < TreeChance(b))      { Tree(pos, b, x); trees++; }
                        else if (rocks < 90 && Random.value < RockChance(b))  { Rock(pos, b); rocks++; }
                    }
                }
        }
        static float TreeChance(CG.Biome b) => b switch
        { CG.Biome.Floresta=>0.18f, CG.Biome.Pantano=>0.10f, CG.Biome.Pradaria=>0.055f,
          CG.Biome.Tundra=>0.065f, CG.Biome.Deserto=>0.032f, CG.Biome.Montanha=>0.025f,
          CG.Biome.Vulcanico=>0.025f, _=>0.04f };
        static float RockChance(CG.Biome b) =>
            (b == CG.Biome.Montanha || b == CG.Biome.Vulcanico) ? 0.12f : 0.032f;

        // ---- ÁRVORES por bioma ----
        void Tree(Vector3 pos, CG.Biome b, int cellX = 0)
        {
            var root = new GameObject("arvore").transform;
            root.SetParent(transform, false); root.position = pos; root.Rotate(0, Random.value*360f, 0);
            float s = Random.Range(1.2f, 2.6f);
            var trunkMat = Prim.Mat(Hex("#6E5638"));
            var fol = Prim.Mat(FoliageColor(b, cellX));
            var darkFol = Prim.Mat(FoliageColor(b, cellX) * 0.82f);

            if (b == CG.Biome.Deserto)
            {
                // cacto, palmeira ou arbusto espinhoso
                int type = Random.Range(0, 3);
                if (type == 0) { // palmeira
                    Cyl(root, trunkMat, 0.14f*s, 0.8f*s, new Vector3(0, 0.8f, 0)*s);
                    for (int k = 0; k < 5; k++) { float a = k/5f*360f; var fr = Prim.Cone(root, fol, 0.55f*s, 0.18f*s, 8); fr.transform.localPosition=new Vector3(0,1.6f,0)*s; fr.transform.localEulerAngles=new Vector3(-60,a,0); }
                } else if (type == 1) { // cacto
                    Cyl(root, fol, 0.16f*s, 0.9f*s, new Vector3(0,0.9f,0)*s);
                    for (int k = 0; k < 2; k++) { float xo=(k==0?-1:1)*0.2f*s; Cyl(root,fol,0.1f*s,0.4f*s,new Vector3(xo,0.7f,0)*s); Cyl(root,fol,0.1f*s,0.28f*s,new Vector3(xo,1.1f,0)*s); }
                } else { // arbusto
                    Prim.Rock(root, fol, 0.4f).transform.localScale = new Vector3(0.9f,0.5f,0.9f)*s;
                }
            }
            else if (b == CG.Biome.Floresta || b == CG.Biome.Tundra)
            {
                // conífera multi-camada
                Cyl(root, trunkMat, 0.16f*s, 0.6f*s, new Vector3(0,0.6f,0)*s);
                for (int k=0; k<4; k++) { float r=(0.72f-k*0.15f)*s; Prim.Cone(root,k%2==0?fol:darkFol,r,0.82f*s,7).transform.localPosition=new Vector3(0,(0.7f+k*0.52f),0)*s; }
            }
            else if (b == CG.Biome.Montanha)
            {
                // pinheiro retorcido + neve no topo
                Cyl(root, trunkMat, 0.12f*s, 0.7f*s, new Vector3(0,0.7f,0)*s);
                for (int k=0; k<3; k++) { Prim.Cone(root,k==2?Prim.Mat(Hex("#E6EEF2")):fol,(0.5f-k*0.12f)*s,0.6f*s,6).transform.localPosition=new Vector3(0,(0.8f+k*0.45f),0)*s; }
            }
            else if (b == CG.Biome.Pantano)
            {
                // mangrove: raízes expostas + copa esparsa
                for (int k=0; k<5; k++) { float a=k/5f*360f; Cyl(root,trunkMat,0.06f*s,0.5f*s,new Vector3(Mathf.Cos(a*Mathf.Deg2Rad)*0.22f*s,0.5f*s,Mathf.Sin(a*Mathf.Deg2Rad)*0.22f*s)).transform.localEulerAngles=new Vector3(15,a,0); }
                Cyl(root, trunkMat, 0.11f*s, 0.8f*s, new Vector3(0,0.8f,0)*s);
                for (int k=0; k<3; k++) { var bl=Prim.Sphere(root,fol); float r=Random.Range(0.5f,0.85f)*s; bl.transform.localScale=new Vector3(r*1.3f,r*0.7f,r*1.3f); bl.transform.localPosition=new Vector3(Random.Range(-0.3f,0.3f)*s,(1.6f+k*0.3f)*s,Random.Range(-0.3f,0.3f)*s); }
            }
            else if (b == CG.Biome.Vulcanico)
            {
                // árvore morta/queimada
                Cyl(root, Prim.Mat(Hex("#2A2020")), 0.13f*s, 0.8f*s, new Vector3(0,0.8f,0)*s);
                for (int k=0; k<3; k++) { float a=Random.Range(20,60); var br=Prim.Cylinder(root,Prim.Mat(Hex("#302020"))); br.transform.localScale=new Vector3(0.06f*s,0.35f*s,0.06f*s); br.transform.localPosition=new Vector3(0,(1.2f+k*0.15f),0)*s; br.transform.localEulerAngles=new Vector3(a,Random.value*360,0); }
            }
            else // Pradaria — árvore de copa arredondada
            {
                Cyl(root, trunkMat, 0.14f*s, 0.5f*s, new Vector3(0,0.5f,0)*s);
                for (int k=0; k<4; k++) { var bl=Prim.Sphere(root,k%2==0?fol:darkFol); float r=Random.Range(0.5f,0.82f)*s; bl.transform.localScale=Vector3.one*r*2; bl.transform.localPosition=new Vector3(Random.Range(-0.24f,0.24f)*s,(1.0f+k*0.38f)*s,Random.Range(-0.24f,0.24f)*s); }
            }

            // colisão (cápsula cobre o tronco)
            var cap = root.gameObject.AddComponent<CapsuleCollider>();
            cap.height = 2.2f*s; cap.radius = 0.22f*s; cap.center = new Vector3(0, 1.1f*s, 0);
        }

        void Rock(Vector3 pos, CG.Biome b)
        {
            var root = new GameObject("rocha").transform;
            root.SetParent(transform, false); root.position = pos; root.Rotate(0, Random.value*360f, 0);
            float s = Random.Range(0.9f, 1.9f);
            if (b == CG.Biome.Tundra || b == CG.Biome.Vulcanico)
            {
                bool lava = b == CG.Biome.Vulcanico;
                var col = lava ? Hex("#E0552A") : Hex("#CFEAF2");
                var mat = Prim.Mat(col, lava?Prim.Finish.Satin:Prim.Finish.Metallic, lava?(Color?)col:null, lava?0.7f:0f);
                for (int k=0; k<Random.Range(2,5); k++) { var sh=Prim.Cone(root,mat,0.16f*s,Random.Range(0.5f,1.1f)*s,4); sh.transform.localPosition=new Vector3(Random.Range(-0.28f,0.28f)*s,0,Random.Range(-0.28f,0.28f)*s); }
            }
            else
            {
                for (int k=0; k<Random.Range(1,4); k++) { var r=Prim.Rock(root,Prim.Mat(Hex("#8A8377")*(0.8f+Random.value*0.35f)),0.5f); float rad=Random.Range(0.5f,1.0f)*s*(k==0?1f:0.7f); r.transform.localScale=new Vector3(rad*1.25f,rad*0.85f,rad*1.25f); r.transform.localPosition=new Vector3(Random.Range(-0.35f,0.35f)*s,rad*0.3f,Random.Range(-0.35f,0.35f)*s); }
            }
            var bc = root.gameObject.AddComponent<BoxCollider>();
            bc.size = new Vector3(s*1.4f, s*0.9f, s*1.4f); bc.center = new Vector3(0, s*0.45f, 0);
        }

        void Grass(Vector3 pos, CG.Biome b)
        {
            var g = Prim.Cone(transform, Prim.Mat(FoliageColor(b)*1.05f), Random.Range(0.06f,0.12f), Random.Range(0.3f,0.55f), 4);
            g.transform.position = pos; g.transform.Rotate(0, Random.value*360f, 0);
        }

        // ---- PROPS FANTÁSTICOS — delega ao FantasyPropBuilder ----
        void FantasyProp(Vector3 pos, int t)
        {
            var root = new GameObject("fprop").transform;
            root.SetParent(transform, false);
            root.position = pos;
            root.Rotate(0, Random.value * 360f, 0);
            float s = Random.Range(1.4f, 3.2f); // props maiores = mais impressionantes
            var th = _themes[t];
            FantasyPropBuilder.Build(t, root, th.foliage, th.foliage2, th.rock, th.accent, th.trunk, s);
        }

        // ================================================================
        //  MÚLTIPLAS ALDEIAS distribuídas pelo mapa
        // ================================================================
        void MultipleVillages()
        {
            int W = _env.W, H = _env.H;
            var placed = new List<Vector2Int>();

            // Polo esquerdo: aldeia grande com CultureIndex
            int ciL = sim != null ? sim.CultureIndex : 0;
            Vector2Int posL = FindLandCenter(W/4, H/2, 6, ref placed);
            PlaceVillage(posL.x, posL.y, ciL, large: true);

            // Polo direito: aldeia grande com CultureIndex2
            int ciR = sim != null ? sim.CultureIndex2 : 0;
            Vector2Int posR = FindLandCenter(3*W/4, H/2, 6, ref placed);
            PlaceVillage(posR.x, posR.y, ciR, large: true);
        }

        Vector2Int FindLandCenter(int sx, int sy, int minSep, ref List<Vector2Int> placed)
        {
            int W = _env.W, H = _env.H;
            for (int r = 0; r < 12; r++)
            {
                int cx = Mathf.Clamp(sx + Random.Range(-r*2, r*2+1), 2, W-2);
                int cy = Mathf.Clamp(sy + Random.Range(-r*2, r*2+1), 2, H-2);
                if (_env.IsBarrier(cx, cy)) continue;
                bool ok = true;
                foreach (var p in placed) { if (Mathf.Abs(p.x-cx)+Mathf.Abs(p.y-cy) < minSep*4) { ok=false; break; } }
                if (ok) { placed.Add(new Vector2Int(cx, cy)); return new Vector2Int(cx, cy); }
            }
            placed.Add(new Vector2Int(sx, sy));
            return new Vector2Int(sx, sy);
        }

        void PlaceVillage(int cx, int cy, int ci, bool large)
        {
            float scale = large ? 2.4f : 1.7f;
            var center = WorldPos(cx, cy);
            var g = new GameObject("Aldeia_" + ci).transform; g.SetParent(transform, false); g.position = center;
            var pal = BuildingPal.ForCulture(ci);

            // fogueira central (grande)
            var froot = new GameObject("fogueira").transform; froot.SetParent(g, false); froot.localPosition = Vector3.zero;
            BuildingBuilder.Fogueira(froot, pal, scale * 0.9f);
            AddBuildingCollider(froot, 3f*scale, 1.5f*scale, 3f*scale);

            // pedra coração + luz
            var heart = Prim.Cube(g, Prim.Mat(Hex("#5FE0C2"), Prim.Finish.Satin, Hex("#5FE0C2"), 1.1f));
            heart.transform.localScale=Vector3.one*0.5f; heart.transform.localPosition=new Vector3(0,2.2f,0); heart.transform.localEulerAngles=new Vector3(45,0,45);
            var fl = new GameObject("Luz").transform; fl.SetParent(g, false); fl.localPosition=new Vector3(0,1.8f,0);
            var lt = fl.gameObject.AddComponent<Light>(); lt.type=LightType.Point; lt.color=pal.flame; lt.range=large?14f:10f; lt.intensity=2.0f;

            // edifícios ao redor
            VillageBuildings(g, ci, pal, scale, large);

            // muro/cerca ao redor da aldeia (dependente de cultura)
            if (large) VillageWall(g, ci, pal, scale);

            // monumento — snapped ao terreno
            float mox = large ? 5.5f : 4f, moz = large ? -5.5f : -4f;
            float mwx = g.position.x + mox, mwz = g.position.z + moz;
            var mroot = new GameObject("monumento").transform; mroot.SetParent(g, false); mroot.position = new Vector3(mwx, TerrainY(mwx, mwz), mwz);
            Monument(mroot, ci, pal, scale);
        }

        // Posições dos edifícios — 6 spots (mais para aldeias grandes)
        static readonly Vector2[] _spots6 = {
            new Vector2(-3.8f, 2.4f), new Vector2(4.0f, 2.0f),
            new Vector2(1.2f, -4.2f), new Vector2(-4.2f,-2.8f),
            new Vector2(-0.4f, 4.5f), new Vector2(3.8f, -3.5f)
        };
        static readonly Vector2[] _spots4 = {
            new Vector2(-2.6f, 1.8f), new Vector2(3.2f, 1.5f),
            new Vector2(0.8f, -3.2f), new Vector2(-3.4f,-2.0f)
        };

        void VillageBuildings(Transform g, int ci, BuildingPal pal, float scale, bool large)
        {
            string[] keys = ci switch
            {
                0 => new[]{ "choca","totem","casa_madeira","celeiro","poco","forno" },
                1 => new[]{ "forno","obelisco","choca","muro","poco","celeiro" },
                2 => new[]{ "castelo","torre","igreja","muralha","poco","celeiro" },
                3 => new[]{ "templo","altar","menir","santuario","poco","torre" },
                4 => new[]{ "templo","obelisco","torre","celeiro","poco","forno" },
                5 => new[]{ "torre","muro","celeiro","poco","farol","barco" },
                6 => new[]{ "doca","farol","barco","poco","celeiro","choca" },
                7 => new[]{ "tenda","cerca","fogueira","totem","poco","celeiro" },
                8 => new[]{ "obelisco","altar","muro","forno","menir","poco" },
                9 => new[]{ "templo","obelisco","santuario","menir","poco","torre" },
                _ => new[]{ "casa_madeira","celeiro","poco","forno","choca","totem" }
            };
            var spots = large ? _spots6 : _spots4;
            float[] scales = { 0.95f, 0.85f, 0.85f, 0.78f, 0.78f, 0.72f };
            for (int i = 0; i < spots.Length; i++)
            {
                var sp = spots[i];
                var root = new GameObject(keys[i]).transform; root.SetParent(g, false);
                // posição horizontal + Y snapped ao terreno (evita flutuar)
                float wx = g.position.x + sp.x * scale;
                float wz = g.position.z + sp.y * scale;
                root.position = new Vector3(wx, TerrainY(wx, wz), wz);
                root.Rotate(0, Random.value * 360f, 0);
                float s = scale * scales[i];
                BuildingBuilder.BuildByKey(keys[i], root, pal, s);
                AddBuildingCollider(root, s*2.5f, s*3.5f, s*2.5f);
            }
        }

        // Muro/cerca ao redor da aldeia conforme cultura
        void VillageWall(Transform g, int ci, BuildingPal pal, float scale)
        {
            float radius = 7.5f * scale;
            int posts = ci == 2 || ci == 4 ? 0 : (ci == 1 ? 0 : 8); // medievais usam muralha; tribais usam cerca
            if (ci == 2) // Medieval: muralha quadrada
            {
                for (int side = 0; side < 4; side++)
                {
                    float a = side * Mathf.PI * 0.5f;
                    var wr = new GameObject("muro_w").transform; wr.SetParent(g, false);
                    wr.localPosition = new Vector3(Mathf.Cos(a)*radius*0.85f, 0, Mathf.Sin(a)*radius*0.85f);
                    wr.localEulerAngles = new Vector3(0, a*Mathf.Rad2Deg+90, 0);
                    BuildingBuilder.Muro(wr, pal, scale * 0.7f);
                }
            }
            else if (posts > 0) // Cerca de troncos
            {
                for (int k = 0; k < posts; k++)
                {
                    float a = k / (float)posts * Mathf.PI * 2;
                    float pwx = g.position.x + Mathf.Cos(a)*radius;
                    float pwz = g.position.z + Mathf.Sin(a)*radius;
                    var pr = new GameObject("cerca_p").transform; pr.SetParent(g, false);
                    pr.position = new Vector3(pwx, TerrainY(pwx, pwz), pwz);
                    Cyl(pr, Prim.Mat(pal.wood), 0.1f*scale, 0.8f*scale, new Vector3(0, 0.8f*scale, 0));
                    Prim.Cone(pr, Prim.Mat(pal.wood), 0.12f*scale, 0.25f*scale, 4).transform.localPosition = new Vector3(0, 1.6f*scale, 0);
                }
            }
        }

        // ================================================================
        //  MONUMENTO icônico por cultura
        // ================================================================
        void Monument(Transform root, int ci, BuildingPal pal, float scale)
        {
            float s = scale * 1.1f;
            var wall = Prim.Mat(pal.wall); var acc = Prim.Mat(pal.accent, Prim.Finish.Satin);
            switch (ci)
            {
                case 1: // Árido — zigurate monumental
                    for (int i=0;i<5;i++) { var b=Prim.Cube(root,wall); b.transform.localScale=new Vector3((2.8f-i*0.48f)*s,0.48f*s,(2.8f-i*0.48f)*s); b.transform.localPosition=new Vector3(0,(0.24f+i*0.48f)*s,0); }
                    Prim.Cube(root,Prim.Mat(pal.accent,Prim.Finish.Satin,pal.accent,0.8f)).transform.localScale=Vector3.one*0.7f*s;
                    break;
                case 2: // Medieval — Torre de Castelo
                    BuildingBuilder.Torre(root, pal, s*1.2f);
                    break;
                case 3: // Arcana — Cristal bicônico flutuante
                    Cyl(root,wall,0.4f*s,0.35f*s,new Vector3(0,0.35f*s,0));
                    for (int i=0;i<2;i++) { var c=Prim.Cone(root,Prim.Mat(pal.accent,Prim.Finish.Satin,pal.accent,0.95f),0.55f*s,1.2f*s,4); c.transform.localPosition=new Vector3(0,2.4f*s,0); c.transform.localRotation=Quaternion.Euler(i*180,0,0); }
                    var aOrb=new GameObject("l").transform; aOrb.SetParent(root,false); aOrb.localPosition=new Vector3(0,2.4f*s,0);
                    var al=aOrb.gameObject.AddComponent<Light>(); al.type=LightType.Point; al.color=pal.accent; al.range=8*s; al.intensity=1.8f;
                    break;
                case 4: // Imperial — Arco do Triunfo
                    Cube(root, wall, new Vector3(0.55f*s,4.2f*s,0.55f*s), new Vector3(-1.4f*s,2.1f*s,0));
                    Cube(root, wall, new Vector3(0.55f*s,4.2f*s,0.55f*s), new Vector3(1.4f*s,2.1f*s,0));
                    Cube(root, wall, new Vector3(3.4f*s,0.7f*s,0.55f*s), new Vector3(0,4.0f*s,0));
                    Cube(root, Prim.Mat(pal.accent), new Vector3(2.5f*s,0.15f*s,0.55f*s), new Vector3(0,4.45f*s,0));
                    break;
                case 5: // Tecnológica — Antena + esfera radiante
                    Cyl(root,wall,0.16f*s,2.2f*s,new Vector3(0,2.2f*s,0));
                    for (int i=0;i<4;i++) { float a=i*90f*Mathf.Deg2Rad; var arm=Prim.Cylinder(root,Prim.Mat(pal.accent,Prim.Finish.Metallic)); arm.transform.localScale=new Vector3(0.05f*s,0.65f*s,0.05f*s); arm.transform.localPosition=new Vector3(0,2.6f*s,0); arm.transform.localEulerAngles=new Vector3(0,i*90,90); }
                    var sph=Prim.Sphere(root,Prim.Mat(pal.accent,Prim.Finish.Satin,pal.accent,1.2f)); sph.transform.localScale=Vector3.one*0.75f*s; sph.transform.localPosition=new Vector3(0,4.2f*s,0);
                    break;
                case 6: // Aquática — Farol luminoso
                    BuildingBuilder.Farol(root, pal, s);
                    break;
                case 7: // Nômade — Círculo de monólitos
                    for (int i=0;i<7;i++) { float a=i/7f*Mathf.PI*2; var mono=Prim.Rock(root,Prim.Mat(pal.stone),0.35f); mono.transform.localScale=new Vector3(0.5f*s,2.4f*s,0.55f*s); mono.transform.localPosition=new Vector3(Mathf.Cos(a)*2.4f*s,1.2f*s,Mathf.Sin(a)*2.4f*s); }
                    var lintel=Prim.Rock(root,Prim.Mat(pal.stone),0.2f); lintel.transform.localScale=new Vector3(2.2f*s,0.4f*s,0.5f*s); lintel.transform.localPosition=new Vector3(0,2.5f*s,2.4f*s);
                    break;
                case 8: // Subterrânea — Menir triplo
                    BuildingBuilder.Menir(root, pal, s*1.2f);
                    break;
                case 9: // Ordem — Coluna clássica perfeita
                    Cyl(root,wall,0.36f*s,3.2f*s,new Vector3(0,3.2f*s,0));
                    Cube(root,wall,new Vector3(1.0f*s,0.2f*s,1.0f*s),new Vector3(0,0.1f*s,0));
                    Cube(root,wall,new Vector3(1.1f*s,0.25f*s,1.1f*s),new Vector3(0,6.45f*s,0));
                    var capOrb=Prim.Sphere(root,Prim.Mat(pal.accent,Prim.Finish.Satin,pal.accent,1.2f)); capOrb.transform.localScale=Vector3.one*0.42f*s; capOrb.transform.localPosition=new Vector3(0,6.8f*s,0);
                    break;
                default: // Floresta — totens duplos
                    for (int i=0;i<3;i++) { var tk=Prim.Cylinder(root,i%2==0?wall:acc); tk.transform.localScale=new Vector3(0.5f*s,(0.8f+i*0.15f)*s,0.5f*s); tk.transform.localPosition=new Vector3((i-1)*0.92f*s,(0.8f+i*0.15f)*s,0); Prim.Cone(root,Prim.Mat(i==1?pal.flame:pal.accent,Prim.Finish.Satin),0.32f*s,0.28f*s,4).transform.localPosition=new Vector3((i-1)*0.92f*s,(1.65f+i*0.15f)*s,0); }
                    break;
            }
            AddBuildingCollider(root, s*4f, s*5f, s*4f);
        }

        // ================================================================
        //  MARCOS POR BIOMA
        // ================================================================
        void Landmarks()
        {
            var marcos = new GameObject("Marcos").transform; marcos.SetParent(transform, false);
            // Chave por (polo, tema/bioma): cada polo pode ter seus próprios marcos
            var done = new HashSet<long>();
            int W = _env.W, H = _env.H;
            for (int y = 5; y < H-5; y += 5)
                for (int x = 5; x < W-5; x += 5)
                {
                    int i = y*W+x;
                    var b = (CG.Biome)_env.Bioma[i];
                    if (b == CG.Biome.Oceano || _env.Altitude[i] < CG.Environment.SeaLevel) continue;
                    int t = ThemeAt(x);
                    int pole = x < W / 2 ? 0 : 1;
                    long key = (long)pole * 1000 + (t > 0 ? t : (int)b + 100);
                    if (done.Contains(key)) continue;
                    done.Add(key);
                    var pos = WorldPos(x, y);
                    var root = new GameObject(key.ToString()).transform; root.SetParent(marcos, false); root.position=pos; root.Rotate(0,Random.value*360f,0);
                    if (t > 0) FantasyLandmark(root, t);
                    else switch (b)
                    {
                        case CG.Biome.Vulcanico: Volcano(root); break;
                        case CG.Biome.Deserto:   Pyramid(root); break;
                        case CG.Biome.Tundra:    Iceberg(root); break;
                        case CG.Biome.Floresta:  GiantTree(root, FoliageColor(b)); break;
                        case CG.Biome.Montanha:  Peak(root); break;
                        case CG.Biome.Pantano:   Willow(root, FoliageColor(b)); break;
                        default: GiantTree(root, FoliageColor(b)); break;
                    }
                }
        }

        void FantasyLandmark(Transform root, int t)
        {
            var th = _themes[t];
            switch (t)
            {
                case 1: Cyl(root,Prim.Mat(th.trunk),0.7f,4.0f,new Vector3(0,4f,0)); var bC=Prim.Sphere(root,Prim.Mat(th.foliage)); bC.transform.localScale=new Vector3(6f,2.5f,6f); bC.transform.localPosition=new Vector3(0,8.5f,0); for (int k=0;k<10;k++) { float a=k/10f*Mathf.PI*2; var sp=Prim.Sphere(root,Prim.Mat(th.accent)); sp.transform.localScale=Vector3.one*0.5f; sp.transform.localPosition=new Vector3(Mathf.Cos(a)*2.8f,9.0f,Mathf.Sin(a)*2.8f); } break;
                case 2: FantasyPropBuilder.Build(2, root, th.foliage, th.foliage2, th.rock, th.accent, th.trunk, 4.5f); break;
                case 3: Cyl(root,Prim.Mat(th.trunk,Prim.Finish.Metallic),0.65f,4.5f,new Vector3(0,4.5f,0)); for (int k=0;k<4;k++){float a=k*Mathf.PI*0.5f; var arm=Prim.Cylinder(root,Prim.Mat(th.accent,Prim.Finish.Metallic)); arm.transform.localScale=new Vector3(0.09f,1.35f,0.09f); arm.transform.localPosition=new Vector3(Mathf.Cos(a)*1.35f,5.5f,Mathf.Sin(a)*1.35f); arm.transform.localEulerAngles=new Vector3(0,a*Mathf.Rad2Deg,90);} var orb2=Prim.Sphere(root,Prim.Mat(th.accent,Prim.Finish.Satin,th.accent,2.0f)); orb2.transform.localScale=Vector3.one*1.1f; orb2.transform.localPosition=new Vector3(0,9.5f,0); break;
                case 4: var wha=Prim.Sphere(root,Prim.Mat(th.foliage)); wha.transform.localScale=new Vector3(4f,2f,7f); wha.transform.localPosition=new Vector3(0,5.5f,0); break;
                case 5: Cyl(root,Prim.Mat(th.rock),0.55f,3.5f,new Vector3(0,3.5f,0)); for(int k=0;k<3;k++){float rr=(1.8f-k*0.45f); var ring=Prim.Cylinder(root,Prim.Mat(th.accent,Prim.Finish.Satin,th.accent,1.0f)); ring.transform.localScale=new Vector3(rr*2,0.14f,rr*2); ring.transform.localPosition=new Vector3(0,1.2f+k*1.6f,0);} break;
                case 6:  FantasyPropBuilder.Build(6,  root, th.foliage, th.foliage2, th.rock, th.accent, th.trunk, 4.5f); break;
                case 7:  FantasyPropBuilder.Build(7,  root, th.foliage, th.foliage2, th.rock, th.accent, th.trunk, 4.5f); break;
                case 8:  var gPr=Prim.Cone(root,Prim.Mat(th.foliage,Prim.Finish.Iridescent),2.2f,6.0f,4); gPr.transform.localRotation=Quaternion.Euler(0,45,0); var gPr2=Prim.Cone(root,Prim.Mat(th.foliage,Prim.Finish.Iridescent),2.2f,3.2f,4); gPr2.transform.localRotation=Quaternion.Euler(180,45,0); break;
                case 9:  for(int k=0;k<3;k++){float pk=(3-k)*1.4f; var mt=Prim.Cone(root,Prim.Mat(th.rock),pk*0.65f,pk*2.8f,4); mt.transform.localPosition=new Vector3((k-1)*2.2f,0,0);} break;
                case 10: FantasyPropBuilder.Build(10, root, th.foliage, th.foliage2, th.rock, th.accent, th.trunk, 4.5f); break;
            }
        }

        // ---- marcos clássicos ----
        void Volcano(Transform root)
        {
            Prim.Cone(root,Prim.Mat(Hex("#3A2A28")),5.2f,7.5f,18).transform.localPosition=Vector3.zero;
            var lava=Prim.Mat(Hex("#E0552A"),Prim.Finish.Satin,Hex("#E0552A"),1.6f);
            var cr=Prim.Cylinder(root,lava); cr.transform.localScale=new Vector3(1.8f,0.3f,1.8f); cr.transform.localPosition=new Vector3(0,7.2f,0);
            for(int i=0;i<6;i++){float a=i/6f*6.28f; var st=Prim.Cube(root,lava); st.transform.localScale=new Vector3(0.22f,2.8f,0.22f); st.transform.localPosition=new Vector3(Mathf.Cos(a)*1.8f,4.5f,Mathf.Sin(a)*1.8f); st.transform.localRotation=Quaternion.Euler(18,a*Mathf.Rad2Deg,0);}
            var lgo=new GameObject("luz").transform; lgo.SetParent(root,false); lgo.localPosition=new Vector3(0,7f,0);
            var l=lgo.gameObject.AddComponent<Light>(); l.type=LightType.Point; l.color=Hex("#E0552A"); l.range=18f; l.intensity=3.5f;
        }
        void Pyramid(Transform root)
        {
            var mat=Prim.Mat(Hex("#C9A86A"));
            for(int i=0;i<6;i++){var b=Prim.Cube(root,mat); b.transform.localScale=new Vector3(5.0f-i*0.76f,0.72f,5.0f-i*0.76f); b.transform.localPosition=new Vector3(0,0.36f+i*0.72f,0);}
            Prim.Cube(root,Prim.Mat(Hex("#E0C46A"),Prim.Finish.Satin)).transform.localScale=Vector3.one*0.8f;
        }
        void Iceberg(Transform root)
        {
            for(int i=0;i<6;i++){var ic=Prim.Cone(root,Prim.Mat(Hex("#CFEAF2"),Prim.Finish.Metallic),Random.Range(0.9f,1.8f),Random.Range(2.5f,5f),4); ic.transform.localPosition=new Vector3(Random.Range(-1.8f,1.8f),0,Random.Range(-1.8f,1.8f)); ic.transform.localRotation=Quaternion.Euler(Random.Range(-12,12),Random.value*360,Random.Range(-12,12));}
        }
        void GiantTree(Transform root, Color foliage)
        {
            Cyl(root,Prim.Mat(Hex("#6E5638")),0.5f,2.0f,new Vector3(0,2f,0));
            for(int i=0;i<6;i++){var bl=Prim.Sphere(root,Prim.Mat(foliage)); float r=Random.Range(1.8f,2.8f); bl.transform.localScale=Vector3.one*r; bl.transform.localPosition=new Vector3(Random.Range(-1f,1f),3.8f+i*0.85f,Random.Range(-1f,1f));}
        }
        void Peak(Transform root)
        {
            var p=Prim.Rock(root,Prim.Mat(Hex("#8A8377")),0.7f); p.transform.localScale=new Vector3(3.8f,6f,3.8f); p.transform.localPosition=new Vector3(0,2.5f,0);
            var cap=Prim.Rock(root,Prim.Mat(Hex("#E6EEF2")),0.6f); cap.transform.localScale=new Vector3(2.5f,1.8f,2.5f); cap.transform.localPosition=new Vector3(0,5f,0);
        }
        void Willow(Transform root, Color foliage)
        {
            Cyl(root,Prim.Mat(Hex("#4a4030")),0.4f,1.8f,new Vector3(0,1.8f,0));
            for(int i=0;i<5;i++){var bl=Prim.Sphere(root,Prim.Mat(foliage*0.85f)); float r=Random.Range(1.6f,2.4f); bl.transform.localScale=new Vector3(r,r*0.7f,r); bl.transform.localPosition=new Vector3(Random.Range(-0.9f,0.9f),3.5f+i*0.6f,Random.Range(-0.9f,0.9f));}
        }

        // ================================================================
        //  HELPERS
        // ================================================================
        static GameObject Cyl(Transform p, Material m, float r, float h, Vector3 pos)
        { var g=Prim.Cylinder(p,m); g.transform.localScale=new Vector3(r*2,h,r*2); g.transform.localPosition=pos; return g; }
        static GameObject Cube(Transform p, Material m, Vector3 size, Vector3 pos)
        { var g=Prim.Cube(p,m); g.transform.localScale=size; g.transform.localPosition=pos; return g; }
        static void AddBuildingCollider(Transform root, float w, float h, float d)
        { var c=root.gameObject.AddComponent<BoxCollider>(); c.size=new Vector3(w,h,d); c.center=new Vector3(0,h*0.5f,0); }

        float Alt(int x, int y) { x=Mathf.Clamp(x,0,_env.W-1); y=Mathf.Clamp(y,0,_env.H-1); return _env.Altitude[y*_env.W+x]; }

        /// <summary>Retorna a altura do terreno em coordenadas de mundo (x,z).</summary>
        float TerrainY(float worldX, float worldZ)
        {
            if (_env == null) return 0f;
            float ox = _env.W * 0.5f, oz = _env.H * 0.5f;
            return WorldPos(worldX / cellSize + ox, worldZ / cellSize + oz).y;
        }

        public Vector3 WorldPos(float gx, float gy)
        {
            if (_env == null) return Vector3.zero;
            float ox=_env.W*0.5f, oz=_env.H*0.5f;
            gx=Mathf.Clamp(gx,0,_env.W-1.001f); gy=Mathf.Clamp(gy,0,_env.H-1.001f);
            int x0=(int)gx, y0=(int)gy; float fx=gx-x0, fy=gy-y0;
            // Interpolação TRIANGULAR — casa com a topologia da mesh (2 triângulos por quad):
            // T1: (x0,y0)+(x0,y0+1)+(x0+1,y0)  T2: (x0+1,y0)+(x0,y0+1)+(x0+1,y0+1)
            float h;
            if (fx + fy <= 1f)
                h = (1f-fx-fy)*Alt(x0,y0) + fy*Alt(x0,y0+1) + fx*Alt(x0+1,y0);
            else
                h = (fx+fy-1f)*Alt(x0+1,y0+1) + (1f-fx)*Alt(x0,y0+1) + (1f-fy)*Alt(x0+1,y0);
            return new Vector3((gx-ox)*cellSize, h*heightScale, (gy-oz)*cellSize);
        }
        public bool WorldToCell(Vector3 world, out int cx, out int cy)
        {
            float ox=_env.W*0.5f, oz=_env.H*0.5f;
            cx=Mathf.RoundToInt(world.x/cellSize+ox);
            cy=Mathf.RoundToInt(world.z/cellSize+oz);
            return cx>=0&&cy>=0&&cx<_env.W&&cy<_env.H;
        }
    }
}
