using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// HUD do mundo do núcleo: status + controles (tempo, novo mundo, clima, cultura,
    /// anoitecer) + NUDGES (Sinal/Faísca/Inspiração, gastando Atenção) + inspeção:
    /// clique numa CÉLULA (temperatura/umidade/recursos) ou CRIATURA (genoma rolável).
    /// </summary>
    public class CoreHud : MonoBehaviour
    {
        public CoreSim sim;
        public CoreWorldView world;
        public CoreCreatureView creatures;
        public CoreDayNight dayNight;
        public Camera cam;

        int _cellX = -1, _cellY = -1, _creatureId = -1;
        Vector3 _down; bool _validDown;
        Vector2 _scroll;
        string _mode;                 // null | "sinal" | "faisca" | "inspiracao"
        float _attention = 70f;
        readonly CG.Rng _nudgeRng = new CG.Rng(0xCAFE);
        string _msg = "";

        void Update()
        {
            if (cam == null || sim == null) return;
            // Atenção recupera no tempo; teto estendido pelo fervor religioso (M12)
            float attCap = sim.Sim != null
                ? 100f + sim.Sim.Pop.Belief.AttentionBonus * 30f
                : 100f;
            if (_attention < attCap) _attention += Time.deltaTime * 8f;

            if (Input.GetMouseButtonDown(0)) { _down = Input.mousePosition; _validDown = Input.mousePosition.x < Screen.width - 290; }
            if (Input.GetMouseButtonUp(0) && _validDown && Vector3.Distance(_down, Input.mousePosition) < 10f)
                Pick();
        }

        void Pick()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f)) return;
            var ct = hit.collider.GetComponentInParent<CoreCreatureTag>();
            if (ct != null) { _creatureId = ct.Id; return; }                 // criatura: inspeciona
            if (hit.collider.GetComponent<CoreTerrainTag>() == null) return;

            if (_mode != null) { ApplyNudge(hit.point); return; }            // chão + nudge ativo → aplica
            if (world.WorldToCell(hit.point, out int x, out int y)) { _cellX = x; _cellY = y; _creatureId = -1; } // senão, inspeciona célula
        }

        void ApplyNudge(Vector3 worldPoint)
        {
            if (_attention < 18f) { _msg = "Atenção insuficiente"; return; }
            if (!world.WorldToCell(worldPoint, out int x, out int y)) return;
            _attention -= 18f;
            var env = sim.Sim.Env;

            if (_mode == "sinal")
            {
                Fx.Signal(worldPoint);
                for (int dy = -2; dy <= 2; dy++)
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= env.W || ny >= env.H) continue;
                        int i = env.Idx(nx, ny);
                        env.Comida[i] = Mathf.Min(1f, env.Comida[i] + 0.5f);  // brota comida (pressão ambiental)
                    }
                sim.Sim.Pop.Belief.RecordNudge(+1); // percebido como benigno (M10 §4.3)
                _msg = "Sinal — comida brota; as criaturas tendem a vir";
            }
            else if (_mode == "faisca")
            {
                Fx.Faisca(worldPoint);
                var c = Nearest(x, y);
                if (c != null) { c.Genome = CG.Reproduction.Reproduce(c.Genome, c.Genome, _nudgeRng, 1f); creatures.MarkDirty(c.Id); _msg = $"Faísca — criatura #{c.Id} sofreu mutação"; }
                sim.Sim.Pop.Belief.RecordNudge(0);  // resultado ambíguo (M10)
            }
            else if (_mode == "inspiracao")
            {
                Fx.Inspiracao(worldPoint);
                int n = 0;
                foreach (var c in sim.Sim.Pop.Creatures)
                {
                    float dx = c.X - (x + 0.5f), dy = c.Y - (y + 0.5f);
                    if (dx * dx + dy * dy < 9f) { c.Energy = Mathf.Min(1f, c.Energy + 0.5f); n++; }
                }
                sim.Sim.Pop.Belief.RecordNudge(+1); // percebido como benigno (M10 §4.3)
                _msg = $"Inspiração — {n} criatura(s) revigorada(s)";
            }
        }

        CG.Creature Nearest(int gx, int gy)
        {
            CG.Creature best = null; float bd = 1e9f;
            foreach (var c in sim.Sim.Pop.Creatures)
            {
                float dx = c.X - (gx + 0.5f), dy = c.Y - (gy + 0.5f), d = dx * dx + dy * dy;
                if (d < bd) { bd = d; best = c; }
            }
            return bd < 36f ? best : null;
        }

        CG.Creature FindCreature(int id)
        {
            if (sim.Sim == null) return null;
            var list = sim.Sim.Pop.Creatures;
            for (int i = 0; i < list.Count; i++) if (list[i].Id == id) return list[i];
            return null;
        }

        void OnGUI()
        {
            if (sim == null || sim.Sim == null) return;
            DrawCivPanel();
            const int W = 290;
            var env = sim.Sim.Env;
            ulong tick = sim.Sim.Tick;
            float season = Mathf.Sin(2f * Mathf.PI * (tick % CG.Environment.Year) / CG.Environment.Year);
            var lang    = sim.Sim.Pop.Language;
            var culture = sim.Sim.Pop.Culture;
            var belief  = sim.Sim.Pop.Belief;
            float attentionMax = 100f + belief.AttentionBonus * 30f;

            GUILayout.BeginArea(new Rect(Screen.width - W - 10, 10, W, 570), GUI.skin.box);
            GUILayout.Label("<b>GÊNESE — Núcleo na Unity</b>");
            string fase = dayNight != null ? dayNight.PhaseName : "—";
            GUILayout.Label($"tick {tick}  ·  pop {sim.Sim.Pop.Count}  ·  {fase}");
            // Multi-civ (E08): mostra pop de cada civ
            for (int ci = 0; ci < sim.Sim.Civs.Count; ci++)
            {
                var civ = sim.Sim.Civs[ci];
                string marker = ci == 0 ? "● " : "○ ";
                GUILayout.Label($"{marker}Civ {ci}: pop {civ.Pop.Count}  grp {civ.Pop.Social.GroupCount}  fig {civ.Pop.Social.FigureCount}");
            }
            GUILayout.Label($"estação {(season >= 0 ? "quente" : "fria")}  ·  Atenção {Mathf.RoundToInt(_attention)}/{Mathf.RoundToInt(attentionMax)}");

            // Camada simbólica
            GUILayout.Label($"língua: <b>{lang.Stage}</b>  léxico:{lang.Lexicon.Count}  deriva:{lang.DriftCount}");
            GUILayout.Label($"cultura: memes:{culture.MemeCount}  coesão:{culture.CulturalCohesion:0.00}");
            GUILayout.Label($"religião: <b>{belief.Stage}</b>  fervor:{belief.Fervor:0.00}  imagem:{belief.Image}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(sim.playing ? "⏸" : "▶")) sim.playing = !sim.playing;
            if (GUILayout.Button("‹vel")) sim.stepsPerSecond = Mathf.Max(1, sim.stepsPerSecond - 4);
            if (GUILayout.Button("vel›")) sim.stepsPerSecond = Mathf.Min(60, sim.stepsPerSecond + 4);
            if (dayNight != null && GUILayout.Button($"☀ {dayNight.PhaseName} →")) dayNight.Toggle();
            GUILayout.EndHorizontal();

            GUILayout.Label("<b>Influência</b> (escolha e clique no chão):");
            GUILayout.BeginHorizontal();
            Toggle("✶ Sinal", "sinal"); Toggle("✦ Faísca", "faisca"); Toggle("☼ Inspiração", "inspiracao");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("⟳ Novo mundo")) sim.NewWorld();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleClimate(-1);
            GUILayout.Label($"Clima N: {sim.ClimateName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleClimate(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleClimate2(-1);
            GUILayout.Label($"Clima S: {sim.ClimateName2}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleClimate2(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleCulture(-1);
            GUILayout.Label($"Cultura: {sim.CultureName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleCulture(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleFantasyTheme(-1);
            GUILayout.Label($"Bioma N: {sim.FantasyThemeName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleFantasyTheme(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleFantasyTheme2(-1);
            GUILayout.Label($"Bioma S: {sim.FantasyThemeName2}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleFantasyTheme2(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleCulture2(-1);
            GUILayout.Label($"Cultura S: {sim.CultureName2}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleCulture2(1);
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_msg)) GUILayout.Label("▸ " + _msg);
            GUILayout.EndArea();

            // ---- inspeção ----
            GUILayout.BeginArea(new Rect(Screen.width - W - 10, 590, W, Screen.height - 600), GUI.skin.box);
            var cr = _creatureId >= 0 ? FindCreature(_creatureId) : null;
            if (cr != null)
            {
                GUILayout.Label($"<b>Criatura #{cr.Id}</b>{(cr.IsFigure ? "  ★ FIGURA" : "")}");
                GUILayout.Label($"geração {cr.Genome.Geracao} · linhagem {cr.Genome.LinhagemId}");
                GUILayout.Label($"idade {cr.Age} · energia {Bar(cr.Energy)}");
                GUILayout.Label($"papel {cr.Role()} · grupo {(cr.GroupId >= 0 ? cr.GroupId.ToString() : "—")}");
                GUILayout.Label($"prestígio {cr.Prestige:0.00} · dominância {cr.DominanceWins}");
                GUILayout.Label("<b>Genoma (role para ver tudo)</b>");
                _scroll = GUILayout.BeginScrollView(_scroll);
                for (int gi = 0; gi < CG.GeneRegistry.Count; gi++)
                {
                    var d = CG.GeneRegistry.Def(gi);
                    GUILayout.Label($"{Short(d.Id),-13} {Bar(cr.Genome.Get(d.Id))}");
                }
                GUILayout.EndScrollView();
            }
            else if (_cellX >= 0)
            {
                int i = env.Idx(_cellX, _cellY);
                bool seca = env.BalancoAgua[i] < CG.Environment.DroughtThreshold;
                GUILayout.Label($"<b>Célula ({_cellX}, {_cellY})</b>");
                GUILayout.Label($"bioma: {(CG.Biome)env.Bioma[i]}{(seca ? "  ⚠ seca" : "")}");
                GUILayout.Label($"altitude   {Bar(env.Altitude[i])}");
                GUILayout.Label($"temperatura{Bar(env.Temp[i])}");
                GUILayout.Label($"umidade    {Bar(env.Umidade[i])}");
                GUILayout.Label($"comida     {Bar(env.Comida[i])}");
                GUILayout.Label($"água       {Bar(env.Agua[i])}");
                GUILayout.Label($"material   {Bar(env.Material[i])}");
                GUILayout.Space(4);
                GUILayout.Label($"<b>Cultura da civilização</b>");
                GUILayout.Label($"fervor   {Bar(belief.Fervor)}");
                GUILayout.Label($"coesão   {Bar(culture.CulturalCohesion)}");
                GUILayout.Label($"organiz. {Bar(belief.Organization)}");
                if (culture.Dominant() is CG.Culture.Meme dom)
                    GUILayout.Label($"meme dom.: [{dom.Type}] força {dom.Force:0.00}  prev {dom.Prevalence:0.00}");
            }
            else GUILayout.Label("Clique numa célula (recursos) ou criatura (genoma).");
            GUILayout.EndArea();
        }

        void DrawCivPanel()
        {
            if (sim == null || sim.Sim == null) return;
            const int PW = 250;
            GUILayout.BeginArea(new Rect(10, 10, PW, 300), GUI.skin.box);
            GUILayout.Label("<b>Civilizações (E08)</b>");
            for (int ci = 0; ci < sim.Sim.Civs.Count; ci++)
            {
                var civ = sim.Sim.Civs[ci];
                GUILayout.Label($"Civ {ci}: pop {civ.Pop.Count} · {civ.Pop.Language.Stage} · {civ.Pop.Belief.Stage}");
                foreach (var kv in civ.Relations)
                {
                    var r = kv.Value;
                    string icon = r.Stance switch
                    {
                        CG.CivStance.Aliada    => "✦",
                        CG.CivStance.Comercial => "⇌",
                        CG.CivStance.Guerra    => "⚔",
                        CG.CivStance.Vassalagem=> "▼",
                        _ => "·"
                    };
                    GUILayout.Label($"  {icon} ↔ Civ {kv.Key}: T{r.Trust:0.0} R{r.Resentment:0.0}");
                }
            }
            GUILayout.Space(4);
            GUILayout.Label($"<b>Eventos</b> (crônica: {sim.Sim.Events.TotalEvents})");
            var evs = sim.Sim.Events.Active;
            if (evs.Count == 0) GUILayout.Label("  (sem eventos activos)");
            else foreach (var ev in evs)
                GUILayout.Label($"  [{ev.Type}] civ {ev.CivId}");
            // Últimos 3 eventos resolvidos
            var log = sim.Sim.Events.Log;
            int start = System.Math.Max(0, log.Count - 3);
            for (int i = start; i < log.Count; i++)
            {
                string res = log[i].Resolution ?? "";
                if (res.Length > 38) res = res.Substring(0, 38) + "…";
                GUILayout.Label($"  ✓ {res}");
            }
            GUILayout.EndArea();
        }

        void Toggle(string label, string mode)
        {
            bool on = _mode == mode;
            if (GUILayout.Toggle(on, label, GUI.skin.button)) _mode = mode; else if (on) _mode = null;
        }

        static string Short(string id) { int d = id.IndexOf('.'); return d >= 0 ? id.Substring(d + 1) : id; }
        static string Bar(float v)
        {
            int n = Mathf.RoundToInt(Mathf.Clamp01(v) * 12);
            return "[" + new string('#', n).PadRight(12, '·') + $"] {v:0.00}";
        }
    }
}
