using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Linha do tempo horizontal no rodapé: mostra ícones coloridos dos eventos
    /// marcantes na ordem em que ocorreram. Hover revela o texto do evento.
    /// Sem texto permanente — leitura opcional.
    /// </summary>
    public class CoreTimeline : MonoBehaviour
    {
        public CoreSim sim;

        const int H       = 36;   // altura da barra
        const int ICON    = 24;   // tamanho de cada ícone
        const int PAD     = 6;
        const int MAX_VIS = 32;   // máximo de ícones visíveis ao mesmo tempo
        // Offset acima da crônica: CH(115) + PAD(8) + gap(4) = 127
        const int BOTTOM_OFFSET = 131;

        struct Entry
        {
            public CG.EventType type;
            public int          civId;
            public ulong        tick;
            public string       text;
        }

        readonly List<Entry> _entries = new List<Entry>();
        int      _syncedLog = 0;
        int      _worldVer  = -1;

        // Estilos
        GUIStyle _sIcon, _sTooltip, _sLabel;
        Texture2D _texW;
        bool _stylesBuilt;

        static readonly Color C_BG   = new Color(0.05f, 0.06f, 0.09f, 0.92f);
        static readonly Color C_SEP  = new Color(0.20f, 0.22f, 0.30f, 1f);

        void BuildStyles()
        {
            if (_stylesBuilt) return;
            _texW = new Texture2D(1,1); _texW.SetPixel(0,0,Color.white); _texW.Apply();

            _sIcon = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 18, alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(0,0,0,0)
            };
            _sIcon.normal.textColor = Color.white;

            _sTooltip = new GUIStyle(GUI.skin.box)
            {
                fontSize = 10, alignment = TextAnchor.UpperLeft,
                wordWrap = true, padding = new RectOffset(6,6,4,4)
            };
            _sTooltip.normal.textColor       = new Color(0.88f,0.90f,0.95f,1f);
            _sTooltip.normal.background      = MakeTex(new Color(0.08f,0.09f,0.14f,0.97f));

            _sLabel = new GUIStyle(GUI.skin.label)
            { fontSize=9, alignment=TextAnchor.MiddleCenter };
            _sLabel.normal.textColor = new Color(0.50f,0.53f,0.60f,1f);

            _stylesBuilt = true;
        }

        Texture2D MakeTex(Color c)
        { var t=new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

        void Update()
        {
            if (sim?.Sim == null) return;

            // Reset quando o mundo muda
            if (sim.WorldVersion != _worldVer)
            { _entries.Clear(); _syncedLog = 0; _worldVer = sim.WorldVersion; }

            // Captura novos eventos do log com deduplicação (mesmo tipo+civ dentro de 500t)
            var log = sim.Sim.Events.Log;
            for (int i = _syncedLog; i < log.Count; i++)
            {
                var ev = log[i];
                bool dup = false;
                for (int j = _entries.Count - 1; j >= Mathf.Max(0, _entries.Count - 8); j--)
                {
                    var e2 = _entries[j];
                    if (e2.type == ev.Type && e2.civId == ev.CivId && ev.Tick - e2.tick < 500)
                        { dup = true; break; }
                }
                if (!dup)
                    // Tenta substituir "civ N" pelo nome (será processado no tooltip também)
                _entries.Add(new Entry { type=ev.Type, civId=ev.CivId, tick=ev.Tick, text=ev.Resolution??ev.Type.ToString() });

            }
            _syncedLog = log.Count;
        }

        void OnGUI()
        {
            if (sim?.Sim == null || _entries.Count == 0) return;
            BuildStyles();

            int sw = Screen.width, sh = Screen.height;
            // Deixa espaço para painel direito e painel esquerdo (crônica)
            int leftMargin  = 265;  // largura da crônica + gap
            int rightMargin = CoreHud.PanelX > 0 ? sw - CoreHud.PanelX + 8 : 285;
            int usableW = sw - leftMargin - rightMargin;
            int barX = leftMargin;
            // Posiciona acima da crônica (BOTTOM_OFFSET = CH + PAD + gap)
            int barY = sh - BOTTOM_OFFSET - H;

            // Fundo da barra
            DrawRect(new Rect(barX, barY, usableW, H), C_BG);
            DrawRect(new Rect(barX, barY, usableW, 1), C_SEP);

            int total = _entries.Count;
            int start = Mathf.Max(0, total - MAX_VIS);
            int count = total - start;
            float slotW = Mathf.Min(ICON + 4, (float)usableW / Mathf.Max(1, count));
            float x0 = barX + 2f;

            int hovered = -1;

            for (int i = 0; i < count; i++)
            {
                var e = _entries[start + i];
                float ix = x0 + i * slotW;
                float iy = barY;

                // Cor e emoji do evento
                Color col = EventColor(e.type);
                string icon = EventIcon(e.type);

                // Fundo colorido do ícone
                float age = (sim.Sim.Tick - e.tick) / 500f; // quão antigo
                float alpha = Mathf.Clamp01(1f - age * 0.35f); // mais velho = mais apagado
                var bg = col; bg.a = alpha * 0.55f;
                DrawRect(new Rect(ix, iy + 2, slotW - 2, H - 4), bg);

                // Ícone (emoji/símbolo)
                _sIcon.normal.textColor = new Color(1f,1f,1f, alpha);
                GUI.Label(new Rect(ix, iy + (H - ICON) * 0.5f, slotW - 2, ICON), icon, _sIcon);

                // Detecta hover
                var r = new Rect(ix, iy, slotW, H);
                if (r.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                    hovered = i;
            }

            // Tooltip do evento hovered
            if (hovered >= 0)
            {
                var e = _entries[start + hovered];
                string tip  = $"t{e.tick}  Civ {e.civId}\n{e.text}";
                float tw    = 200f;
                float tx    = x0 + hovered * slotW;
                tx = Mathf.Clamp(tx, leftMargin, sw - rightMargin - tw - PAD);
                GUI.Box(new Rect(tx, barY - 52, tw, 48), tip, _sTooltip);
            }
        }

        static Color EventColor(CG.EventType t) => t switch
        {
            CG.EventType.Seca            => new Color(0.85f,0.55f,0.10f),
            CG.EventType.Fome            => new Color(0.90f,0.18f,0.10f),
            CG.EventType.ColapsoPop      => new Color(0.40f,0.05f,0.05f),
            CG.EventType.GuerraDeclarada => new Color(0.95f,0.12f,0.08f),
            CG.EventType.Transcendencia  => new Color(0.78f,0.55f,1.00f),
            CG.EventType.Expansao        => new Color(0.20f,0.85f,0.42f),
            CG.EventType.Fusao           => new Color(0.35f,0.68f,1.00f),
            _                            => new Color(0.55f,0.58f,0.65f)
        };

        static string EventIcon(CG.EventType t) => t switch
        {
            CG.EventType.Seca            => "☀",
            CG.EventType.Fome            => "⚠",
            CG.EventType.ColapsoPop      => "☠",
            CG.EventType.GuerraDeclarada => "⚔",
            CG.EventType.Transcendencia  => "✦",
            CG.EventType.Expansao        => "▲",
            CG.EventType.Fusao           => "⇌",
            _                            => "●"
        };

        void DrawRect(Rect r, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;
            var old = GUI.color; GUI.color = c;
            GUI.DrawTexture(r, _texW);
            GUI.color = old;
        }
    }
}
