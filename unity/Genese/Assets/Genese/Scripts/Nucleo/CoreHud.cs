using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    public class CoreHud : MonoBehaviour
    {
        public CoreSim       sim;
        public CoreWorldView world;
        public CoreCreatureView creatures;
        public CoreDayNight  dayNight;
        public Camera        cam;
        public SaveManager   saveManager;

        public static bool IsMouseOverPanel { get; private set; }

        // ── Estado interno ────────────────────────────────────────────────────
        int    _tab       = 0;
        int    _civTab    = 0;
        string _mode      = null;
        string _msg       = "";
        float  _msgTimer;
        string _msgColor  = "#ffffff";
        int    _cellX = -1, _cellY = -1, _creatureId = -1;
        int    _highlightCiv = -1;       // civ selecionada para destaque (-1 = nenhuma)
        int    _lastHudWorld = -1;       // detecta troca de mundo para resetar estilos
        Vector3 _down; bool _validDown;
        Vector2 _scrollCivs, _scrollCron, _scrollGenome, _scrollInspect;
        float  _fpsAccum; int _fpsSamples; float _fps;

        readonly CG.Rng _nudgeRng = new CG.Rng(0xCAFE);

        enum OverlayMode { Bioma, Comida, Agua, Temperatura, Densidade, Grupos }
        OverlayMode _overlay = OverlayMode.Bioma;

        // ── Layout (~75% do original) ─────────────────────────────────────────
        const int PW    = 230;   // 300 → 230
        const int RMAR  = 40;
        const int PAD   = 6;
        const int TAB_H = 22;
        const int HDR_H = 30;
        const int BTM_H = 120;
        static int ContentH => Screen.height - PAD * 2 - HDR_H - TAB_H - BTM_H;
        public static int PanelX => Screen.width - PW - RMAR;

        // Paleta de civs — deve ser idêntica a CoreCreatureView.CivTints
        internal static readonly Color[] CivColors =
        {
            new Color(0.30f, 0.75f, 1.00f),
            new Color(1.00f, 0.55f, 0.10f),
            new Color(0.75f, 0.30f, 1.00f),
            new Color(0.20f, 0.90f, 0.40f),
            new Color(1.00f, 0.85f, 0.20f),
        };
        static Color CivColor(int ci) => CivColors[ci % CivColors.Length];

        // ── Cores ─────────────────────────────────────────────────────────────
        static readonly Color C_BG     = new Color(0.06f, 0.07f, 0.11f, 1f);
        static readonly Color C_SEC    = new Color(0.11f, 0.13f, 0.19f, 1f);
        static readonly Color C_ACCENT = new Color(0.40f, 0.72f, 1.00f, 1f);
        static readonly Color C_GOLD   = new Color(1.00f, 0.85f, 0.35f, 1f);
        static readonly Color C_GREEN  = new Color(0.30f, 0.88f, 0.52f, 1f);
        static readonly Color C_RED    = new Color(1.00f, 0.38f, 0.38f, 1f);
        static readonly Color C_PURPLE = new Color(0.78f, 0.50f, 1.00f, 1f);
        static readonly Color C_TEXT   = new Color(0.90f, 0.92f, 0.96f, 1f);
        static readonly Color C_DIM    = new Color(0.58f, 0.62f, 0.70f, 1f);

        // ── Estilos ───────────────────────────────────────────────────────────
        GUIStyle _sLabel, _sDim, _sBold, _sHeader, _sSecTitle,
                 _sBtn, _sBtnOn, _sBtnSmall, _sWrap, _sTabOn, _sTabOff;
        Texture2D _texW;
        bool _stylesBuilt;

        // ─────────────────────────────────────────────────────────────────────
        void Update()
        {
            if (sim == null) return;
            if (_msgTimer > 0) { _msgTimer -= Time.deltaTime; if (_msgTimer <= 0) _msg = ""; }

            // FPS (atualiza a cada 0.5s)
            _fpsAccum += Time.deltaTime; _fpsSamples++;
            if (_fpsAccum >= 0.5f) { _fps = _fpsSamples / _fpsAccum; _fpsAccum = 0; _fpsSamples = 0; }

            // Propaga seleção para CoreCreatureView
            if (creatures != null) creatures.HighlightCivIdx = _highlightCiv;

            // Atenção: pool 10× maior (máx 500) e regen 2× extra
            if (sim?.Sim?.Influence != null)
            {
                var infl2 = sim.Sim.Influence;
                float bigMax = infl2.MaxAttention(sim.Sim.Pop.Belief.Fervor) * 10f;
                if (infl2.Attention < bigMax)
                    infl2.Attention = Mathf.Min(bigMax, infl2.Attention + Time.deltaTime * 6f);
            }

            // Força rebuild de estilos quando o mundo muda (resolve HUD vazia sem novo mapa)
            if (sim != null && sim.WorldVersion != _lastHudWorld) { _stylesBuilt = false; _lastHudWorld = sim.WorldVersion; }

            // Detecta hover sobre painel (coordenadas GUI: y=0 em cima)
            Vector2 mp  = Input.mousePosition;
            Vector2 gui = new Vector2(mp.x, Screen.height - mp.y);
            var rPanel  = new Rect(PanelX, PAD, PW + RMAR, Screen.height - PAD * 2);
            var rCron   = new Rect(PAD, Screen.height - 115 - PAD, 260, 115);
            var rInsp   = new Rect(PAD, PAD, 260, 130);
            IsMouseOverPanel = rPanel.Contains(gui) || rCron.Contains(gui) || rInsp.Contains(gui);

            if (Input.GetMouseButtonDown(0) && !IsMouseOverPanel) { _down = Input.mousePosition; _validDown = true; }
            if (Input.GetMouseButtonDown(0) &&  IsMouseOverPanel)   _validDown = false;
            if (Input.GetMouseButtonUp(0) && _validDown && Vector3.Distance(_down, Input.mousePosition) < 10f) Pick();
        }

        void Pick()
        {
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f)) return;
            var ct = hit.collider.GetComponentInParent<CoreCreatureTag>();
            if (ct != null) { _creatureId = ct.Id; _cellX = -1; return; }
            if (hit.collider.GetComponent<CoreTerrainTag>() == null) return;
            if (_mode != null) { ApplyNudge(hit.point); return; }
            if (world.WorldToCell(hit.point, out int x, out int y))
            {
                _cellX = x; _cellY = y; _creatureId = -1;
                sim.Sim?.Influence.AddFocus(x, y, 0.5f);
            }
        }

        // Custo dos poderes divinos (fora do InfluenceSystem.Cost)
        static float DivineCost(string mode) => mode switch
        {
            "montanha" => 45f, "plantar" => 20f, "chuva"   => 25f,
            "raio"     => 40f, "queimar" => 30f, "demolir" => 35f,
            _          => 0f
        };

        void ApplyNudge(Vector3 wp)
        {
            if (!world.WorldToCell(wp, out int x, out int y)) return;
            var infl = sim.Sim.Influence; var env = sim.Sim.Env;

            // ── Poderes divinos (custo custom) ───────────────────────────────
            float divinCost = DivineCost(_mode);
            if (divinCost > 0f)
            {
                if (!infl.SpendCustom(divinCost)) { Msg($"Atenção insuficiente ({infl.Attention:0}/{divinCost:0})", C_RED); return; }
                ApplyDivine(wp, x, y, env);
                return;
            }

            // ── Nudges clássicos ─────────────────────────────────────────────
            CG.NudgeType nt;
            switch (_mode)
            {
                case "faisca":     nt = CG.NudgeType.Faisca;     break;
                case "inspiracao": nt = CG.NudgeType.Inspiracao; break;
                case "protecao":   nt = CG.NudgeType.Protecao;   break;
                case "pressao":    nt = CG.NudgeType.Pressao;    break;
                default:           nt = CG.NudgeType.Sinal;      break;
            }
            if (!infl.CanApply(nt)) { Msg($"Atenção insuficiente ({infl.Attention:0}/{CG.InfluenceSystem.Cost[(int)nt]:0})", C_RED); return; }
            infl.Spend(nt);
            switch (_mode)
            {
                case "sinal":
                    Fx.Signal(wp);
                    for (int dy2=-2;dy2<=2;dy2++) for (int dx2=-2;dx2<=2;dx2++) { int nx=x+dx2,ny=y+dy2; if (nx<0||ny<0||nx>=env.W||ny>=env.H) continue; env.Comida[env.Idx(nx,ny)]=Mathf.Min(1f,env.Comida[env.Idx(nx,ny)]+0.5f); }
                    sim.Sim.Pop.Belief.RecordNudge(+1); Msg("Sinal — recursos brotam", C_GREEN); break;
                case "faisca":
                    Fx.Faisca(wp); var cf=Near(x,y);
                    if (cf!=null){cf.Genome=CG.Reproduction.Reproduce(cf.Genome,cf.Genome,_nudgeRng,1f);creatures.MarkDirty(cf.Id);Msg($"Faísca — #{cf.Id} mutou",C_PURPLE);}
                    sim.Sim.Pop.Belief.RecordNudge(0); break;
                case "inspiracao":
                    Fx.Inspiracao(wp); int n2=0;
                    foreach(var c2 in sim.Sim.Pop.Creatures){float dx2=c2.X-(x+.5f),dy2=c2.Y-(y+.5f);if(dx2*dx2+dy2*dy2<9f){c2.Energy=Mathf.Min(1f,c2.Energy+0.5f);n2++;}}
                    sim.Sim.Pop.Belief.RecordNudge(+1); Msg($"Inspiração — {n2} criatura(s)",C_GOLD); break;
                case "protecao":
                    var cp=Near(x,y); if(cp!=null){infl.ApplyProtection(cp.Id);Msg($"Proteção — #{cp.Id} por 80t",C_ACCENT);}
                    sim.Sim.Pop.Belief.RecordNudge(+1); break;
                case "pressao":
                    for(int dy2=-3;dy2<=3;dy2++) for(int dx2=-3;dx2<=3;dx2++){int nx=x+dx2,ny=y+dy2;if(nx<0||ny<0||nx>=env.W||ny>=env.H)continue;int idx=env.Idx(nx,ny);env.BaseTemp[idx]=Mathf.Clamp01(env.BaseTemp[idx]+0.05f);env.BaseUmid[idx]=Mathf.Clamp01(env.BaseUmid[idx]+0.08f);}
                    sim.Sim.Pop.Belief.RecordNudge(0); Msg("Pressão — clima alterado",C_ACCENT); break;
            }
        }

        void ApplyDivine(Vector3 wp, int x, int y, CG.Environment env)
        {
            switch (_mode)
            {
                case "montanha":
                    world.RaiseTerrain(x, y, 3.5f, 0.22f);
                    SpawnFx(wp, new Color(0.7f,0.65f,0.5f), 12, 8f);   // poeira
                    Msg("Montanha — terreno elevado", new Color(0.8f,0.75f,0.6f));
                    sim.Sim.Pop.Belief.RecordNudge(0);
                    break;

                case "plantar":
                    world.AddFood(x, y, 4f, 0.55f);
                    SpawnFx(wp, new Color(0.2f,0.85f,0.3f), 10, 5f);   // verdura
                    Msg("Plantar — comida e água crescem", C_GREEN);
                    sim.Sim.Pop.Belief.RecordNudge(+1);
                    break;

                case "chuva":
                    world.AddFood(x, y, 5f, 0.35f);
                    // Aumenta balanço de água na área
                    for(int dy2=-4;dy2<=4;dy2++) for(int dx2=-4;dx2<=4;dx2++)
                    {
                        int nx=x+dx2,ny=y+dy2; if(nx<0||ny<0||nx>=env.W||ny>=env.H) continue;
                        env.BalancoAgua[env.Idx(nx,ny)]=Mathf.Clamp(env.BalancoAgua[env.Idx(nx,ny)]+0.4f,-1f,1f);
                    }
                    SpawnFx(wp, new Color(0.3f,0.6f,1f), 16, 7f);      // gotas
                    Msg("Chuva — seca aliviada", C_ACCENT);
                    sim.Sim.Pop.Belief.RecordNudge(+1);
                    break;

                case "raio":
                    // Raio — mata criaturas num raio de 3 células (energia → 0)
                    int atingidas = 0;
                    foreach(var civ2 in sim.Sim.Civs)
                        foreach(var c2 in civ2.Pop.Creatures)
                        {
                            float dx2=c2.X-(x+.5f), dy2=c2.Y-(y+.5f);
                            if(dx2*dx2+dy2*dy2<9f && c2.Alive)
                            { c2.Energy = 0f; atingidas++; }  // mortal
                        }
                    SpawnFxBolt(wp);
                    Msg($"Raio — {atingidas} criatura(s) fulminada(s)", new Color(1f,0.9f,0.2f));
                    sim.Sim.Pop.Belief.RecordNudge(-1);
                    break;

                case "queimar":
                    // Fogo — queima ambiente E mata criaturas na área
                    world.DrainFood(x, y, 4f, 0.7f);
                    int queimadas = 0;
                    for(int dy2=-3;dy2<=3;dy2++) for(int dx2=-3;dx2<=3;dx2++)
                    {
                        int nx=x+dx2,ny=y+dy2; if(nx<0||ny<0||nx>=env.W||ny>=env.H) continue;
                        env.BalancoAgua[env.Idx(nx,ny)]=Mathf.Max(-1f, env.BalancoAgua[env.Idx(nx,ny)]-0.3f);
                    }
                    foreach(var civ2 in sim.Sim.Civs)
                        foreach(var c2 in civ2.Pop.Creatures)
                        {
                            float dx2=c2.X-(x+.5f), dy2=c2.Y-(y+.5f);
                            if(dx2*dx2+dy2*dy2<16f && c2.Alive)   // raio 4
                            { c2.Energy = Mathf.Max(0f, c2.Energy - 0.75f); queimadas++; }
                        }
                    SpawnFx(wp, new Color(1f,0.4f,0.05f), 14, 6f);
                    Msg($"Queimar — {queimadas} criatura(s) atingida(s)", C_RED);
                    sim.Sim.Pop.Belief.RecordNudge(-1);
                    break;

                case "demolir":
                    int n3 = world.DemolishBuildings(wp, 5f);
                    SpawnFx(wp, new Color(0.7f,0.6f,0.4f), 18, 7f);    // detritos
                    Msg(n3>0 ? $"Demolir — {n3} estrutura(s) destruída(s)" : "Demolir — nada para destruir aqui", n3>0?C_RED:C_DIM);
                    sim.Sim.Pop.Belief.RecordNudge(-1);
                    break;
            }
        }

        // Efeito visual: esferas que sobem e somem (genérico)
        void SpawnFx(Vector3 pos, Color col, int count, float spread)
        {
            StartCoroutine(FxRoutine(pos, col, count, spread));
        }

        System.Collections.IEnumerator FxRoutine(Vector3 pos, Color col, int count, float spread)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = col; mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", col*0.8f);
            var gos = new GameObject[count];
            for(int i=0;i<count;i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(go.GetComponent<Collider>());
                go.GetComponent<MeshRenderer>().material = mat;
                go.transform.position = pos + new Vector3(
                    Random.Range(-spread*.5f, spread*.5f), Random.Range(0f,2f),
                    Random.Range(-spread*.5f, spread*.5f));
                float s = Random.Range(0.15f, 0.45f);
                go.transform.localScale = Vector3.one * s;
                gos[i] = go;
            }
            float t = 0f, dur = 1.2f;
            while(t < dur)
            {
                t += Time.deltaTime;
                float a = 1f - t/dur;
                for(int i=0;i<gos.Length;i++)
                {
                    if(gos[i]==null) continue;
                    gos[i].transform.position += Vector3.up * Time.deltaTime * Random.Range(1.5f,3f);
                    var c2 = gos[i].GetComponent<MeshRenderer>().material.color;
                    c2.a = a; gos[i].GetComponent<MeshRenderer>().material.color = c2;
                }
                yield return null;
            }
            foreach(var go in gos) if(go) Destroy(go);
        }

        // Efeito de raio: coluna brilhante vertical
        void SpawnFxBolt(Vector3 pos)
        {
            StartCoroutine(BoltRoutine(pos));
        }

        System.Collections.IEnumerator BoltRoutine(Vector3 pos)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f,0.95f,0.3f);
            mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", new Color(2f,1.8f,0.5f));
            var bolt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(bolt.GetComponent<Collider>());
            bolt.GetComponent<MeshRenderer>().material = mat;
            bolt.transform.position = pos + Vector3.up * 15f;
            bolt.transform.localScale = new Vector3(0.3f, 15f, 0.3f);
            // Luz do raio
            var lgo = new GameObject("boltLight");
            lgo.transform.position = pos + Vector3.up * 3f;
            var lt = lgo.AddComponent<Light>();
            lt.type=LightType.Point; lt.color=new Color(1f,0.95f,0.3f); lt.range=20f; lt.intensity=8f;
            float t=0f;
            while(t<0.4f){ t+=Time.deltaTime; float a=1f-t/0.4f; lt.intensity=8f*a; yield return null; }
            Destroy(bolt); Destroy(lgo);
        }

        CG.Creature Near(int gx,int gy){ CG.Creature b=null; float bd=1e9f; foreach(var c in sim.Sim.Pop.Creatures){float dx=c.X-(gx+.5f),dy=c.Y-(gy+.5f),d=dx*dx+dy*dy;if(d<bd){bd=d;b=c;}} return bd<36f?b:null; }
        CG.Creature FindC(int id){ if(sim.Sim==null)return null; var l=sim.Sim.Pop.Creatures; for(int i=0;i<l.Count;i++)if(l[i].Id==id)return l[i]; return null; }
        void Msg(string t, Color c){ _msg=t; _msgTimer=4f; _msgColor=$"#{ColorUtility.ToHtmlStringRGB(c)}"; }

        // ─────────────────────────────────────────────────────────────────────
        void BuildStyles()
        {
            if (_stylesBuilt) return;
            _texW = new Texture2D(1,1); _texW.SetPixel(0,0,Color.white); _texW.Apply();

            _sLabel   = Sty(9,  C_TEXT);
            _sDim     = Sty(8,  C_DIM);
            _sBold    = Sty(9,  C_TEXT,  bold:true);
            _sHeader  = Sty(10, C_ACCENT, bold:true);
            _sSecTitle= Sty(8,  C_DIM,   bold:true);
            _sWrap    = Sty(8,  C_DIM,   wrap:true);

            _sBtn     = MakeBtn(new Color(0.15f,0.18f,0.28f,1f), C_TEXT,    9);
            _sBtnOn   = MakeBtn(C_ACCENT,                         Color.black,9);
            _sBtnSmall= MakeBtn(new Color(0.13f,0.16f,0.24f,1f), C_DIM,     8);
            _sTabOff  = MakeBtn(new Color(0.10f,0.12f,0.18f,1f), C_DIM,     8);
            _sTabOn   = MakeBtn(C_SEC,                            C_ACCENT,  8, bold:true);

            _stylesBuilt = true;
        }

        GUIStyle Sty(int size, Color col, bool bold=false, bool wrap=false)
        {
            var s = new GUIStyle(GUI.skin.label){ fontSize=size, fontStyle=bold?FontStyle.Bold:FontStyle.Normal, wordWrap=wrap, padding=new RectOffset(2,2,1,1) };
            s.normal.textColor = col; return s;
        }
        GUIStyle MakeBtn(Color bg, Color txt, int size, bool bold=false)
        {
            var s = new GUIStyle(GUI.skin.button){ fontSize=size, fontStyle=bold?FontStyle.Bold:FontStyle.Normal, padding=new RectOffset(4,4,3,3) };
            s.normal.textColor  = txt; s.hover.textColor  = Color.white; s.active.textColor = Color.white;
            var t = new Texture2D(1,1); t.SetPixel(0,0,bg); t.Apply();
            s.normal.background = s.focused.background = t;
            var th = new Texture2D(1,1); th.SetPixel(0,0,new Color(bg.r+0.06f,bg.g+0.06f,bg.b+0.10f,1f)); th.Apply();
            s.hover.background  = th;
            var ta = new Texture2D(1,1); ta.SetPixel(0,0,new Color(bg.r-0.04f,bg.g-0.04f,bg.b-0.04f,1f)); ta.Apply();
            s.active.background = ta;
            return s;
        }

        // ─────────────────────────────────────────────────────────────────────
        void OnGUI()
        {
            if (sim?.Sim == null) return;
            BuildStyles();
            if (IsMouseOverPanel && Event.current.type == EventType.ScrollWheel) Event.current.Use();

            var env    = sim.Sim.Env;
            var infl   = sim.Sim.Influence;
            var belief = sim.Sim.Pop.Belief;
            ulong tick = sim.Sim.Tick;
            string fase = dayNight!=null ? dayNight.PhaseName : "—";
            float season = Mathf.Sin(2f*Mathf.PI*(tick%CG.Environment.Year)/CG.Environment.Year);

            int py = PAD, ph = Screen.height - PAD*2;

            // ── Fundo do painel ───────────────────────────────────────────────
            DrawRect(new Rect(PanelX, py, PW, ph), C_BG);

            // ── FPS (canto sup-esq do painel) ─────────────────────────────────
            GUI.Label(new Rect(PanelX - 52, py + 2, 48, 14), $"{_fps:0} fps", _sDim);

            // ── Cabeçalho fixo ────────────────────────────────────────────────
            GUILayout.BeginArea(new Rect(PanelX+PAD, py+4, PW-PAD*2, HDR_H));
            GUILayout.BeginHorizontal();
            GUILayout.Label("GÊNESE", _sHeader);
            GUILayout.Label($"t{tick}  {(season>=0?"☀":"❄")} {fase}", _sDim);
            GUILayout.EndHorizontal();
            // Barra de Atenção inline
            float att = infl.Attention, attMax = infl.MaxAttention(belief.Fervor);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Atenção {att:0}/{attMax:0}", _sDim, GUILayout.Width(100));
            var ar = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8), GUILayout.ExpandWidth(true));
            if (Event.current.type==EventType.Repaint) DrawProgressBar(ar, att/attMax, att/attMax>0.5f?C_GREEN:att/attMax>0.25f?C_GOLD:C_RED);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // ── Barra de abas ─────────────────────────────────────────────────
            int tabY = py + HDR_H;
            GUILayout.BeginArea(new Rect(PanelX, tabY, PW, TAB_H));
            GUILayout.BeginHorizontal();
            string[] tabNames = { "Status", "Civs", "Mundo", "Config" };
            for (int i = 0; i < 4; i++)
                if (GUILayout.Button(tabNames[i], _tab==i ? _sTabOn : _sTabOff, GUILayout.ExpandWidth(true), GUILayout.Height(TAB_H)))
                    _tab = i;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // ── Conteúdo da aba (tamanho fixo) ───────────────────────────────
            int contentY = tabY + TAB_H;
            var contentRect = new Rect(PanelX+1, contentY, PW-2, ContentH);
            DrawRect(contentRect, C_SEC);
            GUILayout.BeginArea(new Rect(contentRect.x+PAD, contentRect.y+PAD, contentRect.width-PAD*2, contentRect.height-PAD*2));
            switch (_tab)
            {
                case 0: DrawTabStatus(tick, season); break;
                case 1: DrawTabCivs();  break;
                case 2: DrawTabMundo(env, belief); break;
                case 3: DrawTabConfig(fase); break;
            }
            GUILayout.EndArea();

            // ── Área inferior fixa ────────────────────────────────────────────
            int btmY = contentY + ContentH + 2;
            DrawRect(new Rect(PanelX, btmY, PW, BTM_H), C_BG);
            GUILayout.BeginArea(new Rect(PanelX+PAD, btmY+6, PW-PAD*2, BTM_H-8));
            DrawBottom(infl);
            GUILayout.EndArea();

            // ── Overlay ───────────────────────────────────────────────────────
            if (world != null) world.ActiveOverlay = (int)_overlay;

            // ── Inspeção (painel esquerdo, topo) ─────────────────────────────
            DrawInspect(env, belief);

            // ── Crônica (painel esquerdo, rodapé) ────────────────────────────
            DrawChronicle();
        }

        // ── ABA 0: STATUS ─────────────────────────────────────────────────────
        void DrawTabStatus(ulong tick, float season)
        {
            // Destino
            Color dc = sim.Sim.Destiny switch
            { CG.DestinyType.Prosperidade=>C_GREEN, CG.DestinyType.Transcendencia=>C_GOLD, CG.DestinyType.Extincao=>C_RED, CG.DestinyType.Estagnacao=>C_RED, CG.DestinyType.Fusao=>C_PURPLE, CG.DestinyType.Divergencia=>C_ACCENT, _=>C_TEXT };
            Row("Destino", sim.Sim.Destiny.ToString(), dc);
            Row("Crônica", $"{sim.Sim.Chronicle.Count} entradas");
            Sep();

            // Dinâmica global
            L("DINÂMICA  (últimos 50t)", _sSecTitle);
            int totPop=0,totB=0,totD=0;
            foreach(var civ in sim.Sim.Civs){totPop+=civ.Pop.Count;totB+=civ.Pop.BirthsRecent;totD+=civ.Pop.DeathsRecent;}
            int delta=totB-totD;
            Color tc = delta>0?C_GREEN:delta<0?C_RED:C_DIM;
            string tr = delta>2?"▲ crescendo":delta<-2?"▼ declínio":"◆ estável";
            GUILayout.BeginHorizontal();
            CL($"↑{totB}", C_GREEN); CL($"  ↓{totD}", C_RED); CL($"  {tr}", tc);
            GUILayout.EndHorizontal();
            Row("Pop total", $"{totPop}");
            Row("Linhagens", $"{CG.Speciation.ContarLinhagens(sim.Sim.Pop.Creatures)} activas");

            // Pressão reprodutiva global (soma de todas as civs)
            float totalPress = 0f;
            foreach(var civ in sim.Sim.Civs) totalPress += civ.Pop.GlobalPopPressure;
            float pressNorm = totalPress / Mathf.Max(1, 0.09f * sim.Sim.Civs.Count);
            Color pressCol = pressNorm < 0.4f ? C_GREEN : pressNorm < 0.75f ? C_GOLD : C_RED;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pressão reprod.", _sSecTitle, GUILayout.Width(90));
            MiniBarInline(pressNorm, pressCol, 90);
            GUILayout.EndHorizontal();
            Sep();

            // Civs resumo com identidade (nome + traço)
            L("CIVILIZAÇÕES", _sSecTitle);
            for(int ci=0;ci<sim.Sim.Civs.Count;ci++)
            {
                var civ=sim.Sim.Civs[ci];
                Color cc = CivColor(ci);
                string civName  = CG.CivIdentity.NameOf(civ);
                string trait    = CG.CivIdentity.TraitOf(civ);
                string trIcon   = CG.CivIdentity.TraitIcon(trait);
                int b=civ.Pop.BirthsRecent,d=civ.Pop.DeathsRecent;
                Color tcc=b>d?C_GREEN:b<d?C_RED:C_DIM;
                string tt=b>d?"▲":b<d?"▼":"◆";
                bool selected = _highlightCiv == ci;
                // Linha: ● Nome (clicável para destacar) + traço
                GUILayout.BeginHorizontal();
                CL("● ", cc);
                var ns=new GUIStyle(_sBold); ns.normal.textColor=cc;
                if(selected) ns.fontStyle=FontStyle.BoldAndItalic;
                if(GUILayout.Button(civName, ns, GUILayout.Width(82)))
                    _highlightCiv = selected ? -1 : ci; // toggle
                CL($" {trIcon}{trait}", C_DIM);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  {civ.Pop.Count}p  {civ.Pop.Social.GroupCount}g  {civ.Pop.Social.FigureCount}fig", _sLabel);
                CL($"  {tt}", tcc);
                GUILayout.EndHorizontal();
                foreach(var kv in civ.Relations)
                {
                    var r=kv.Value; string icon=r.Stance switch{CG.CivStance.Aliada=>"✦",CG.CivStance.Comercial=>"⇌",CG.CivStance.Guerra=>"⚔",_=>"·"};
                    Color sc=r.Stance==CG.CivStance.Guerra?C_RED:r.Stance==CG.CivStance.Aliada?C_GREEN:C_DIM;
                    CL($"   {icon} Civ {kv.Key}  T{r.Trust:0.1} R{r.Resentment:0.1}", sc);
                }
            }
            Sep();

            // Eventos activos
            var evs=sim.Sim.Events.Active;
            if(evs.Count>0)
            {
                L("EVENTOS ATIVOS", _sSecTitle);
                foreach(var ev in evs)
                { Color ec=ev.Type switch{CG.EventType.Seca=>C_GOLD,CG.EventType.Fome=>C_RED,CG.EventType.GuerraDeclarada=>C_RED,_=>C_ACCENT}; CL($"▶ {ev.Type}  civ {ev.CivId}",ec); }
            }

            // Últimos eventos resolvidos
            var log=sim.Sim.Events.Log; int st=System.Math.Max(0,log.Count-3);
            for(int i=log.Count-1;i>=st;i--)
            {
                string res=ProcessText(log[i].Resolution??"");
                GUILayout.Label("✓ "+res, _sWrap);
            }
        }

        // ── ABA 1: CIVS ───────────────────────────────────────────────────────
        void DrawTabCivs()
        {
            if(sim.Sim.Civs.Count==0) return;
            _civTab = Mathf.Clamp(_civTab, 0, sim.Sim.Civs.Count-1);
            var civ = sim.Sim.Civs[_civTab];
            Color cc = CivColor(_civTab);
            string civName2 = CG.CivIdentity.NameOf(civ);
            string trait2   = CG.CivIdentity.TraitOf(civ);

            // Seletor de civ com nome
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("‹", _sBtnSmall, GUILayout.Width(22))) _civTab=(_civTab-1+sim.Sim.Civs.Count)%sim.Sim.Civs.Count;
            var hs=new GUIStyle(_sBold); hs.normal.textColor=cc;
            GUILayout.Label($"{civName2}  —  {civ.Pop.Count}p", hs);
            if(GUILayout.Button("›", _sBtnSmall, GUILayout.Width(22))) _civTab=(_civTab+1)%sim.Sim.Civs.Count;
            GUILayout.EndHorizontal();
            Sep();

            _scrollCivs = GUILayout.BeginScrollView(_scrollCivs, GUIStyle.none, GUI.skin.verticalScrollbar);

            // Grupos
            var groups = new Dictionary<int, List<CG.Creature>>();
            int solo=0;
            foreach(var c in civ.Pop.Creatures){if(!c.Alive)continue;if(c.GroupId<0){solo++;continue;}if(!groups.ContainsKey(c.GroupId))groups[c.GroupId]=new List<CG.Creature>();groups[c.GroupId].Add(c);}
            L($"GRUPOS  ({civ.Pop.Social.GroupCount}g · {civ.Pop.Social.FigureCount} fig)", _sSecTitle);
            foreach(var kv in groups)
            {
                var mbrs=kv.Value; float avgE=0f; CG.Creature ldr=null;
                foreach(var m in mbrs){avgE+=m.Energy;if(m.IsFigure&&(ldr==null||m.Prestige>ldr.Prestige))ldr=m;}
                avgE/=mbrs.Count;
                Color ec=avgE>0.6f?C_GREEN:avgE>0.35f?C_GOLD:C_RED;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  G{kv.Key}", _sDim, GUILayout.Width(28));
                GUILayout.Label($"{mbrs.Count}m", _sLabel, GUILayout.Width(24));
                CL($"⚡{avgE:0.00}", ec);
                if(ldr!=null) CL($"  ★#{ldr.Id}",C_GOLD);
                GUILayout.EndHorizontal();
                var sb=new System.Text.StringBuilder("    ");
                int sh=0; foreach(var m in mbrs){if(sh++>=5){sb.Append("…");break;}sb.Append($"#{m.Id} ");}
                GUILayout.Label(sb.ToString(), _sDim);
            }
            if(solo>0) CL($"  Solitários: {solo}", C_DIM);
            Sep();

            // Simbólico
            var cl=civ.Pop.Language; var cc2=civ.Pop.Culture; var cb=civ.Pop.Belief;
            L("SIMBÓLICO", _sSecTitle);
            Row("Língua",   $"{cl.Stage}  lex {cl.Lexicon.Count}  drift {cl.DriftCount}");
            Row("Religião", $"{cb.Stage}  fervor {cb.Fervor:0.00}");
            Row("Imagem",   cb.Image.ToString());
            Row("Símbolos", $"{cc2.SymbolCount}  coesão {cc2.CulturalCohesion:0.00}");
            var dom=cc2.Dominant();
            if(dom.HasValue) CL($"  Dom: [{dom.Value.Type}] f{dom.Value.Force:0.00} p{dom.Value.Prevalence:0.00}", C_GOLD);
            Sep();

            // Linhagens
            var linC=new Dictionary<int,int>();
            foreach(var c in civ.Pop.Creatures) if(c.Alive){linC.TryGetValue(c.Genome.LinhagemId,out int lc);linC[c.Genome.LinhagemId]=lc+1;}
            L($"LINHAGENS  ({linC.Count})", _sSecTitle);
            int sh2=0;
            foreach(var kv in linC){if(sh2++>4){CL("  …",C_DIM);break;} CL($"  Lin {kv.Key}: {kv.Value} criaturas", kv.Key==0?C_TEXT:C_ACCENT);}
            Sep();

            // Relações
            if(civ.Relations.Count>0)
            {
                L("RELAÇÕES", _sSecTitle);
                foreach(var kv in civ.Relations)
                {
                    var r=kv.Value; string ic=r.Stance switch{CG.CivStance.Aliada=>"✦ aliada",CG.CivStance.Comercial=>"⇌ comercial",CG.CivStance.Guerra=>"⚔ guerra",_=>"· cautelosa"};
                    Color sc=r.Stance==CG.CivStance.Guerra?C_RED:r.Stance==CG.CivStance.Aliada?C_GREEN:C_DIM;
                    CL($"  Civ {kv.Key}  {ic}  T{r.Trust:0.1} R{r.Resentment:0.1}", sc);
                }
            }

            GUILayout.EndScrollView();
        }

        // ── ABA 2: MUNDO ──────────────────────────────────────────────────────
        void DrawTabMundo(CG.Environment env, CG.Belief belief)
        {
            L("AMBIENTE", _sSecTitle);
            // Estatísticas rápidas
            float foodSum=0,tempSum=0; int drought=0,land=0,dense=0;
            int[] cp=new int[env.W*env.H];
            foreach(var civ in sim.Sim.Civs) foreach(var c in civ.Pop.Creatures) if(c.Alive){int cx2=Mathf.Clamp((int)c.X,0,env.W-1),cy2=Mathf.Clamp((int)c.Y,0,env.H-1);cp[env.Idx(cx2,cy2)]++;}
            for(int i=0;i<env.Bioma.Length;i++){if(env.IsBarrier(i%env.W,i/env.W))continue;land++;foodSum+=env.Comida[i];tempSum+=env.Temp[i];if(env.BalancoAgua[i]<CG.Environment.DroughtThreshold)drought++;if(cp[i]>=5)dense++;}
            float avgF=land>0?foodSum/land:0f, avgT=land>0?tempSum/land:0f;

            MiniBar("Comida",     avgF, C_GREEN);
            MiniBar("Temperatura",avgT, C_RED);
            Row("Seca",    $"{(land>0?100f*drought/land:0):0}% das células");
            Row("Densas",  $"{dense} células (≥5 criaturas)");
            if(dense>0) CL("  ⚠ superlotação", C_RED);
            Sep();

            L("OVERLAY", _sSecTitle);
            GUILayout.BeginHorizontal();
            string[] ovNames={"Bioma","Comida","Água","Temp","Pop","Grupos"};
            for(int oi=0;oi<6;oi++)
            {
                bool on=(int)_overlay==oi;
                if(GUILayout.Button(ovNames[oi], on?_sBtnOn:_sBtnSmall, GUILayout.Width(45))) _overlay=(OverlayMode)oi;
                if(oi==2) GUILayout.EndHorizontal(); // quebra em 2 linhas
                if(oi==2) GUILayout.BeginHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        // ── ABA 3: CONFIG ─────────────────────────────────────────────────────
        void DrawTabConfig(string fase)
        {
            L("MUNDO", _sSecTitle);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("⟳ Novo mundo", _sBtn)) sim.NewWorld();
            if(saveManager!=null)
            {
                if(GUILayout.Button("💾", _sBtn, GUILayout.Width(30))){saveManager.Save(1);Msg("Salvo (slot 1)",C_GREEN);}
                if(saveManager.HasSave(1)&&GUILayout.Button("📂", _sBtn, GUILayout.Width(30))){saveManager.Load(1);Msg("Carregado",C_ACCENT);}
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            CycleRow("Civs",    $"{sim.NumCivs}",            d=>sim.CycleNumCivs(d));
            CycleRow("Biomas",  sim.BiomeVarietyName,        d=>sim.CycleBiomeVariety(d));
            CycleRow("Clima N", sim.ClimateName,             d=>sim.CycleClimate(d));
            CycleRow("Clima S", sim.ClimateName2,            d=>sim.CycleClimate2(d));
            CycleRow("Bioma N", sim.FantasyThemeName,        d=>sim.CycleFantasyTheme(d));
            CycleRow("Bioma S", sim.FantasyThemeName2,       d=>sim.CycleFantasyTheme2(d));
            CycleRow("Cult. N", sim.CultureName,             d=>sim.CycleCulture(d));
            CycleRow("Cult. S", sim.CultureName2,            d=>sim.CycleCulture2(d));
        }

        // ── Área inferior: nudges + poderes divinos + tempo ──────────────────
        void DrawBottom(CG.InfluenceSystem infl)
        {
            L("INFLUÊNCIA", _sSecTitle);
            GUILayout.BeginHorizontal();
            NudgeBtn("✶ Sinal",   "sinal",      CG.NudgeType.Sinal,      infl);
            NudgeBtn("✦ Faísca",  "faisca",     CG.NudgeType.Faisca,     infl);
            NudgeBtn("☼ Inspir.", "inspiracao", CG.NudgeType.Inspiracao, infl);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            NudgeBtn("⚑ Proteção","protecao",   CG.NudgeType.Protecao,   infl);
            NudgeBtn("⊕ Pressão", "pressao",    CG.NudgeType.Pressao,    infl);
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            L("PODERES DIVINOS", _sSecTitle);
            GUILayout.BeginHorizontal();
            DivinBtn("⛰ Montanha", "montanha", 45f, infl);
            DivinBtn("🌱 Plantar",  "plantar",  20f, infl);
            DivinBtn("🌧 Chuva",    "chuva",    25f, infl);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DivinBtn("⚡ Raio",    "raio",     40f, infl);
            DivinBtn("🔥 Queimar", "queimar",  30f, infl);
            DivinBtn("💥 Demolir", "demolir",  35f, infl);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            Sep();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(sim.playing?"⏸":"▶", _sBtn, GUILayout.Width(36))) sim.playing=!sim.playing;
            if(GUILayout.Button("‹‹", _sBtnSmall, GUILayout.Width(26))) sim.stepsPerSecond=Mathf.Max(1,sim.stepsPerSecond-4);
            GUILayout.Label($"{sim.stepsPerSecond:0}t/s", _sDim, GUILayout.Width(36));
            if(GUILayout.Button("››", _sBtnSmall, GUILayout.Width(26))) sim.stepsPerSecond=Mathf.Min(60,sim.stepsPerSecond+4);
            if(dayNight!=null&&GUILayout.Button(dayNight.PhaseName+"→", _sBtnSmall)) dayNight.Toggle();
            GUILayout.EndHorizontal();
            if(!string.IsNullOrEmpty(_msg)) GUILayout.Label($"<color={_msgColor}>▸ {_msg}</color>", _sWrap);
        }

        // ── Inspeção (painel esquerdo) ────────────────────────────────────────
        void DrawInspect(CG.Environment env, CG.Belief belief)
        {
            var cr = _creatureId>=0 ? FindC(_creatureId) : null;
            if(cr==null && _cellX<0) return;
            const int IW=255, IH=200;
            DrawRect(new Rect(PAD, PAD, IW, IH), C_BG);
            GUILayout.BeginArea(new Rect(PAD+6, PAD+6, IW-12, IH-12));
            if(cr!=null)
            {
                GUILayout.BeginHorizontal(); CL($"#{cr.Id}", C_ACCENT); if(cr.IsFigure) CL("  ★ FIGURA",C_GOLD); GUILayout.EndHorizontal();
                Row("Papel",   $"{cr.Role()}  grp {(cr.GroupId>=0?cr.GroupId.ToString():"—")}");
                Row("Vida",    $"age {cr.Age}  gen {cr.Genome.Geracao}  lin {cr.Genome.LinhagemId}");
                MiniBar("Energia", cr.Energy, C_GREEN);
                Row("Prestígio",$"{cr.Prestige:0.00}  dom {cr.DominanceWins}");
                L("Genoma", _sSecTitle);
                _scrollGenome = GUILayout.BeginScrollView(_scrollGenome, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(85));
                for(int gi=0;gi<CG.GeneRegistry.Count;gi++)
                {
                    var d=CG.GeneRegistry.Def(gi); float v=cr.Genome.Get(d.Id);
                    Color gc=d.Bloco==CG.Bloco.Comportamental?C_ACCENT:d.Bloco==CG.Bloco.Regulatorio?C_PURPLE:C_TEXT;
                    GUILayout.BeginHorizontal();
                    var ls=new GUIStyle(_sDim); ls.normal.textColor=gc;
                    GUILayout.Label(Short(d.Id), ls, GUILayout.Width(80));
                    MiniBarInline(v, gc, 85);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                int i=env.Idx(_cellX,_cellY); bool seca=env.BalancoAgua[i]<CG.Environment.DroughtThreshold;
                GUILayout.BeginHorizontal(); CL($"({_cellX},{_cellY})",C_ACCENT); if(seca)CL("  ⚠ seca",C_RED); GUILayout.EndHorizontal();
                Row("Bioma", ((CG.Biome)env.Bioma[i]).ToString());
                MiniBar("Comida",    env.Comida[i],  C_GREEN);
                MiniBar("Água",      env.Agua[i],    C_ACCENT);
                MiniBar("Temp",      env.Temp[i],    C_RED);
                MiniBar("Umidade",   env.Umidade[i], C_ACCENT);
                MiniBar("Altitude",  env.Altitude[i],C_DIM);
                float focus=sim.Sim.Influence.FocusAt(_cellX,_cellY);
                if(focus>0.1f) Row("Foco",$"{focus:0.0} →×{sim.Sim.Influence.FocusMultiplier(_cellX,_cellY):0.00}");
                CG.PopStats.Compute(sim.Sim.Pop.Creatures, out float[] means, out float[] stds);
                if(CG.PopStats.HomogeneityAlert(stds)) CL("⚠ Diversidade crítica!",C_RED);
            }
            GUILayout.EndArea();
        }

        // ── Crônica ───────────────────────────────────────────────────────────
        void DrawChronicle()
        {
            var chron=sim.Sim.Chronicle; if(chron.Count==0) return;
            const int CW=255, CH=115;
            DrawRect(new Rect(PAD, Screen.height-CH-PAD, CW, CH), C_BG);
            GUILayout.BeginArea(new Rect(PAD+6, Screen.height-CH-PAD+6, CW-12, CH-12));
            L("Crônica", _sSecTitle);
            _scrollCron=GUILayout.BeginScrollView(_scrollCron, GUIStyle.none, GUI.skin.verticalScrollbar);
            int st=System.Math.Max(0,chron.Count-6);
            for(int i=chron.Count-1;i>=st;i--) GUILayout.Label(ProcessText(chron.Entries[i].Text), _sWrap);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ── Helpers de desenho ────────────────────────────────────────────────
        void DrawRect(Rect r, Color c)
        {
            if(Event.current.type!=EventType.Repaint) return;
            var old=GUI.color; GUI.color=c; GUI.DrawTexture(r,_texW); GUI.color=old;
        }
        void DrawProgressBar(Rect r, float t, Color col)
        {
            GUI.color=new Color(0.12f,0.13f,0.18f,1f); GUI.DrawTexture(r,_texW);
            GUI.color=new Color(col.r,col.g,col.b,0.85f);
            GUI.DrawTexture(new Rect(r.x,r.y,r.width*Mathf.Clamp01(t),r.height),_texW);
            GUI.color=Color.white;
        }
        void MiniBar(string lbl, float v, Color col)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(lbl, _sDim, GUILayout.Width(72));
            MiniBarInline(v, col, 90);
            GUILayout.EndHorizontal();
        }
        void MiniBarInline(float v, Color col, int w)
        {
            float t=Mathf.Clamp01(v);
            var r=GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8), GUILayout.Width(w));
            if(Event.current.type==EventType.Repaint) DrawProgressBar(r, t, col);
            GUILayout.Label($" {v:0.00}", _sDim, GUILayout.Width(30));
        }
        void Row(string lbl, string val, Color? valCol=null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(lbl, _sSecTitle, GUILayout.Width(70));
            var vs=valCol.HasValue?new GUIStyle(_sLabel){normal={textColor=valCol.Value}}:_sLabel;
            GUILayout.Label(val, vs);
            GUILayout.EndHorizontal();
        }
        void Row(string lbl, string val, Color valCol) => Row(lbl, val, (Color?)valCol);
        void Sep() { GUILayout.Space(3); var r=GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1), GUILayout.ExpandWidth(true)); if(Event.current.type==EventType.Repaint)DrawRect(r, new Color(0.2f,0.22f,0.30f,1f)); GUILayout.Space(3); }
        void L(string t, GUIStyle s) => GUILayout.Label(t, s);
        void CL(string t, Color c) { var s=new GUIStyle(_sLabel); s.normal.textColor=c; GUILayout.Label(t, s); }
        void CycleRow(string lbl, string val, System.Action<int> fn)
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("‹", _sBtnSmall, GUILayout.Width(20))) fn(-1);
            GUILayout.Label($"<color=#9aa>  {lbl}:</color> {val}", _sLabel);
            if(GUILayout.Button("›", _sBtnSmall, GUILayout.Width(20))) fn(+1);
            GUILayout.EndHorizontal();
        }
        void NudgeBtn(string lbl, string mode, CG.NudgeType nt, CG.InfluenceSystem infl)
        {
            bool on=_mode==mode; float cost=CG.InfluenceSystem.Cost[(int)nt];
            bool canUse=infl.CanApply(nt); var old=GUI.enabled; GUI.enabled=canUse||on;
            if(GUILayout.Button($"{lbl}({cost:0})", on?_sBtnOn:_sBtn)) _mode=on?null:mode;
            GUI.enabled=old;
        }

        void DivinBtn(string lbl, string mode, float cost, CG.InfluenceSystem infl)
        {
            bool on=_mode==mode;
            bool canUse=infl.CanApplyCustom(cost); var old=GUI.enabled; GUI.enabled=canUse||on;
            var st = on ? _sBtnOn : _sBtnSmall;
            if(GUILayout.Button($"{lbl}({cost:0})", st)) _mode=on?null:mode;
            GUI.enabled=old;
        }

        static string Short(string id){ int d=id.IndexOf('.'); return d>=0?id.Substring(d+1):id; }

        // Substitui "civ N" / "Civ N" pelo nome real da civilização no texto
        string ProcessText(string text)
        {
            if (sim?.Sim == null || string.IsNullOrEmpty(text)) return text;
            foreach (var civ in sim.Sim.Civs)
            {
                string name = CG.CivIdentity.NameOf(civ);
                text = text.Replace($"civ {civ.Id}", name)
                           .Replace($"Civ {civ.Id}", name)
                           .Replace($"civilização {civ.Id}", name)
                           .Replace($"Civilização {civ.Id}", name);
            }
            return text;
        }
    }
}
