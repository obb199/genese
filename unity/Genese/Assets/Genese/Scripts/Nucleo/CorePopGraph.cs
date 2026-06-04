using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Gráfico de população por civ ao longo do tempo + indicador de saúde geral da run.
    /// Desenhado como painel flutuante no canto superior esquerdo.
    /// "Saúde" = combinação de pop total, destino e diversidade de linhagens.
    /// </summary>
    public class CorePopGraph : MonoBehaviour
    {
        public CoreSim sim;

        const int SAMPLES    = 120;
        const int GW         = 230;
        const int GH         = 72;
        const int PANEL_W    = 248;
        const int PAD        = 7;
        const int SAMPLE_INT = 10;
        // Altura dinâmica calculada no OnGUI
        int PanelH(int civs) => 22 + GH + 6 + civs * 17 + 26;

        readonly List<int[]> _history = new List<int[]>(); // [tick_sample][civ_pop]
        int   _lastSampleTick = -1;
        int   _worldVer       = -1;
        int   _civCount       = 0;

        GUIStyle _sLabel, _sTitle, _sTrait;
        Texture2D _texW;
        bool _built;

        // Mesma paleta que CoreHud.CivColors
        static Color[] CivCols => CoreHud.CivColors;

        void BuildStyles()
        {
            if (_built) return;
            _texW = new Texture2D(1,1); _texW.SetPixel(0,0,Color.white); _texW.Apply();
            _sLabel = new GUIStyle(GUI.skin.label){fontSize=9, padding=new RectOffset(2,2,1,1)};
            _sLabel.normal.textColor = new Color(0.55f,0.58f,0.65f,1f);
            _sTitle = new GUIStyle(GUI.skin.label){fontSize=10, fontStyle=FontStyle.Bold, padding=new RectOffset(2,2,2,2)};
            _sTitle.normal.textColor = new Color(0.88f,0.90f,0.96f,1f);
            _sTrait = new GUIStyle(GUI.skin.label){fontSize=9, fontStyle=FontStyle.Italic, padding=new RectOffset(2,2,0,0)};
            _sTrait.normal.textColor = new Color(0.62f,0.65f,0.72f,1f);
            _built = true;
        }

        void Update()
        {
            if (sim?.Sim == null) return;
            ulong tick = sim.Sim.Tick;

            if (sim.WorldVersion != _worldVer)
            {
                _history.Clear(); _lastSampleTick = -1;
                _worldVer = sim.WorldVersion;
                _civCount = sim.Sim.Civs.Count;
            }

            // Registra amostra a cada SAMPLE_INT ticks
            if ((int)tick - _lastSampleTick >= SAMPLE_INT)
            {
                var sample = new int[sim.Sim.Civs.Count];
                for (int i = 0; i < sim.Sim.Civs.Count; i++)
                    sample[i] = sim.Sim.Civs[i].Pop.Count;
                _history.Add(sample);
                if (_history.Count > SAMPLES) _history.RemoveAt(0);
                _lastSampleTick = (int)tick;
            }
        }

        void OnGUI()
        {
            if (sim?.Sim == null || _history.Count < 2) return;
            BuildStyles();

            int x0 = PAD;
            int civs = sim.Sim.Civs.Count;
            int panelH = PanelH(civs);
            int y0 = PAD + 148;

            DrawRect(new Rect(x0, y0, PANEL_W, panelH), new Color(0.05f,0.06f,0.10f,0.92f));

            // Título
            GUI.Label(new Rect(x0+PAD, y0+4, PANEL_W-PAD, 16), "População", _sTitle);

            // ── Gráfico de linhas ────────────────────────────────────────────
            int gx = x0 + PAD, gy = y0 + 22;
            DrawRect(new Rect(gx, gy, GW, GH), new Color(0.08f,0.09f,0.13f,1f));

            // Linha de grade (máximo)
            int maxPop = 1;
            foreach (var s in _history) foreach (var v in s) if (v > maxPop) maxPop = v;
            DrawRect(new Rect(gx, gy + GH/2, GW, 1), new Color(0.18f,0.20f,0.28f,1f));

            int civCount = _civCount;
            for (int ci = 0; ci < civCount; ci++)
            {
                Color col = CivCols[ci % CivCols.Length];
                col.a = 0.9f;
                for (int s = 1; s < _history.Count; s++)
                {
                    if (ci >= _history[s].Length || ci >= _history[s-1].Length) continue;
                    float x1 = gx + (s-1) / (float)(SAMPLES-1) * GW;
                    float x2 = gx + s     / (float)(SAMPLES-1) * GW;
                    float y1 = gy + GH - _history[s-1][ci] / (float)maxPop * GH;
                    float y2 = gy + GH - _history[s  ][ci] / (float)maxPop * GH;
                    DrawLine(new Vector2(x1,y1), new Vector2(x2,y2), col, 2f);
                }
                // Valor atual (ponto final)
                if (ci < _history[_history.Count-1].Length)
                {
                    int val = _history[_history.Count-1][ci];
                    float px = gx + GW - 2;
                    float py = gy + GH - val/(float)maxPop*GH;
                    DrawRect(new Rect(px-3,py-3,6,6), col);
                }
            }

            // ── Legenda de civs (nome + traço) ───────────────────────────────
            int ly = gy + GH + 6;
            for (int ci = 0; ci < civCount; ci++)
            {
                Color col = CivCols[ci % CivCols.Length];
                var civ = sim.Sim.Civs[ci];
                string name  = CG.CivIdentity.NameOf(civ);
                string trait = CG.CivIdentity.TraitOf(civ);
                string icon  = CG.CivIdentity.TraitIcon(trait);
                int pop      = civ.Pop.Count;

                // Cor de fundo da civ
                DrawRect(new Rect(gx, ly + ci*18, 8, 12), col);
                _sLabel.normal.textColor = col;
                GUI.Label(new Rect(gx+12, ly + ci*17, 90, 14), name, _sLabel);
                _sTrait.normal.textColor = new Color(0.58f,0.62f,0.70f,1f);
                GUI.Label(new Rect(gx+104, ly + ci*17, 80, 14), $"{icon} {trait}", _sTrait);
                _sLabel.normal.textColor = new Color(0.55f,0.58f,0.65f,1f);
                GUI.Label(new Rect(gx+186, ly + ci*17, 40, 14), $"{pop}p", _sLabel);
            }

            // ── Saúde geral da run ────────────────────────────────────────────
            int hy = ly + civCount * 17 + 4;
            float health = ComputeRunHealth();
            Color hcol = health > 0.6f ? new Color(0.25f,0.85f,0.45f)
                       : health > 0.3f ? new Color(1f,0.75f,0.15f)
                                       : new Color(0.95f,0.18f,0.18f);
            DrawRect(new Rect(gx, hy, GW, 8), new Color(0.12f,0.13f,0.18f,1f));
            DrawRect(new Rect(gx, hy, (int)(GW * health), 8), hcol);
            _sLabel.normal.textColor = new Color(0.55f,0.58f,0.65f,1f);
            GUI.Label(new Rect(gx, hy+9, GW, 12),
                health > 0.6f ? "Run: florescendo" : health > 0.3f ? "Run: estável" : "Run: crítica",
                _sLabel);
        }

        float ComputeRunHealth()
        {
            if (sim?.Sim == null) return 0.5f;
            var civs = sim.Sim.Civs;
            if (civs.Count == 0) return 0f;

            float score = 0f; int alive = 0;
            foreach (var civ in civs)
            {
                if (civ.Pop.Count == 0) continue; alive++;
                score += Mathf.Clamp01(civ.Pop.Count / 30f);
                score += civ.Pop.Belief.Organization * 0.3f;
                score += civ.Pop.Culture.CulturalCohesion * 0.2f;
            }
            if (alive == 0) return 0f;
            score /= alive * 1.5f;

            // Penalidade por destinos negativos
            var d = sim.Sim.Destiny;
            if (d == CG.DestinyType.Extincao)  score *= 0.1f;
            if (d == CG.DestinyType.Estagnacao) score *= 0.5f;
            if (d == CG.DestinyType.Prosperidade || d == CG.DestinyType.Transcendencia) score = Mathf.Max(score, 0.85f);

            return Mathf.Clamp01(score);
        }

        // ── Helpers de desenho ────────────────────────────────────────────────
        void DrawRect(Rect r, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, _texW); GUI.color = old;
        }

        void DrawLine(Vector2 a, Vector2 b, Color col, float w)
        {
            if (Event.current.type != EventType.Repaint) return;
            Vector2 d = b - a; float len = d.magnitude;
            if (len < 0.5f) return;
            var mat = new Rect(a.x, a.y - w*0.5f, len, w);
            GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, a);
            var old = GUI.color; GUI.color = col; GUI.DrawTexture(mat, _texW); GUI.color = old;
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
