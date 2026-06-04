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
            if (_attention < 100f) _attention += Time.deltaTime * 8f;

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
                _msg = "Sinal — comida brota; as criaturas tendem a vir";
            }
            else if (_mode == "faisca")
            {
                Fx.Faisca(worldPoint);
                var c = Nearest(x, y);
                if (c != null) { c.Genome = CG.Reproduction.Reproduce(c.Genome, c.Genome, _nudgeRng, 1f); creatures.MarkDirty(c.Id); _msg = $"Faísca — criatura #{c.Id} sofreu mutação"; }
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
            const int W = 280;
            var env = sim.Sim.Env;
            ulong tick = sim.Sim.Tick;
            float season = Mathf.Sin(2f * Mathf.PI * (tick % CG.Environment.Year) / CG.Environment.Year);

            GUILayout.BeginArea(new Rect(Screen.width - W - 10, 10, W, 346), GUI.skin.box);
            GUILayout.Label("<b>GÊNESE — Núcleo na Unity</b>");
            string fase = dayNight != null ? dayNight.PhaseName : "—";
            GUILayout.Label($"tick {tick}  ·  pop {sim.Sim.Pop.Count}  ·  {fase}");
            GUILayout.Label($"grupos {sim.Sim.Pop.Social.GroupCount}  ·  figuras {sim.Sim.Pop.Social.FigureCount}");
            GUILayout.Label($"estação {(season >= 0 ? "quente" : "fria")} ({season:0.00})  ·  Atenção {Mathf.RoundToInt(_attention)}");

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
            GUILayout.Label($"Clima: {sim.ClimateName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleClimate(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleCulture(-1);
            GUILayout.Label($"Cultura: {sim.CultureName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleCulture(1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹")) sim.CycleFantasyTheme(-1);
            GUILayout.Label($"Bioma: {sim.FantasyThemeName}", GUILayout.Width(W - 96));
            if (GUILayout.Button("›")) sim.CycleFantasyTheme(1);
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_msg)) GUILayout.Label("▸ " + _msg);
            GUILayout.EndArea();

            // ---- inspeção ----
            GUILayout.BeginArea(new Rect(Screen.width - W - 10, 366, W, Screen.height - 376), GUI.skin.box);
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
            }
            else GUILayout.Label("Clique numa célula (recursos) ou criatura (genoma).");
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
