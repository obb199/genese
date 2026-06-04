using System.Collections.Generic;
using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Notificações toast de eventos em tempo real: aparecem brevemente no canto
    /// da tela com cor e ícone do evento, e somem sem interação. O jogador vê o
    /// drama acontecendo sem precisar abrir o painel da Crônica.
    /// </summary>
    public class CoreToast : MonoBehaviour
    {
        public CoreSim sim;

        const float DURATION   = 5.0f;   // segundos visíveis
        const float FADE_TIME  = 1.2f;   // segundos de fade-out
        const int   MAX_TOASTS = 4;      // máx. simultâneos
        const int   W          = 280;    // largura do toast
        const int   H          = 48;     // altura
        const int   PAD        = 10;

        struct Toast
        {
            public string  text;
            public string  icon;
            public Color   col;
            public float   timeLeft;
            public string  civName;
            public int     civId;
            public CG.EventType evType;
        }

        readonly List<Toast>                         _toasts      = new List<Toast>();
        readonly Dictionary<long, float>             _lastShown   = new Dictionary<long, float>(); // cooldown por (civId,type)
        int  _syncedLog = 0;
        int  _worldVer  = -1;

        // Chave de cooldown para evitar duplicatas
        static long CooldownKey(int civId, CG.EventType t) => ((long)civId << 32) | (uint)t;
        const float DEDUP_COOLDOWN = 8f; // segundos entre toasts do mesmo tipo/civ

        GUIStyle _sText, _sIcon, _sCivName, _sClose;
        Texture2D _texW;
        bool _built;

        void BuildStyles()
        {
            if (_built) return;
            _texW = new Texture2D(1,1); _texW.SetPixel(0,0,Color.white); _texW.Apply();

            _sText = new GUIStyle(GUI.skin.label)
            { fontSize=11, wordWrap=true, padding=new RectOffset(4,4,2,2) };
            _sText.normal.textColor = new Color(0.90f,0.92f,0.96f,1f);

            _sCivName = new GUIStyle(GUI.skin.label)
            { fontSize=10, fontStyle=FontStyle.Bold, padding=new RectOffset(4,4,1,1) };
            _sCivName.normal.textColor = new Color(1f,0.85f,0.35f,1f);

            _sIcon = new GUIStyle(GUI.skin.label)
            { fontSize=20, alignment=TextAnchor.MiddleCenter };
            _sIcon.normal.textColor = Color.white;

            _sClose = new GUIStyle(GUI.skin.button)
            { fontSize=10, padding=new RectOffset(0,0,0,0), alignment=TextAnchor.MiddleCenter };
            _sClose.normal.textColor = new Color(0.6f,0.6f,0.65f,1f);

            _built = true;
        }

        void Update()
        {
            if (sim?.Sim == null) return;

            if (sim.WorldVersion != _worldVer)
            { _toasts.Clear(); _syncedLog = 0; _worldVer = sim.WorldVersion; return; }

            // Decai cooldowns
            float dt2 = Time.deltaTime;
            var expiredKeys = new List<long>();
            foreach (var kv in _lastShown)
            { if (Time.time - kv.Value > DEDUP_COOLDOWN) expiredKeys.Add(kv.Key); }
            foreach (var k in expiredKeys) _lastShown.Remove(k);

            // Captura novos eventos com deduplicação
            var log = sim.Sim.Events.Log;
            for (int i = _syncedLog; i < log.Count; i++)
            {
                var ev = log[i];
                long ck = CooldownKey(ev.CivId, ev.Type);
                if (_lastShown.ContainsKey(ck)) continue; // mesmo evento recente → ignora
                _lastShown[ck] = Time.time;

                var civ = sim.Sim.Civs.Find(c => c.Id == ev.CivId);
                string name = civ != null ? CG.CivIdentity.NameOf(civ) : $"Civ {ev.CivId}";
                string res  = ev.Resolution ?? "";
                int arrow   = res.IndexOf('→');
                string desc = arrow >= 0 ? res.Substring(arrow + 1).Trim() : res;
                if (desc.Length > 55) desc = desc.Substring(0, 55) + "…";

                if (_toasts.Count >= MAX_TOASTS) _toasts.RemoveAt(0);
                _toasts.Add(new Toast
                {
                    text=desc, icon=EventIcon(ev.Type), col=EventColor(ev.Type),
                    timeLeft=DURATION, civName=name, civId=ev.CivId, evType=ev.Type
                });
            }
            _syncedLog = log.Count;

            // Decai
            float dt = Time.deltaTime;
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                var t = _toasts[i]; t.timeLeft -= dt; _toasts[i] = t;
                if (t.timeLeft <= 0) _toasts.RemoveAt(i);
            }
        }

        void OnGUI()
        {
            if (_toasts.Count == 0) return;
            BuildStyles();

            int sw = Screen.width, sh = Screen.height;
            int rightPad = 380; // deixa espaço para o painel HUD
            float x = sw - W - rightPad - PAD;

            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                var t = _toasts[i];
                float alpha = t.timeLeft < FADE_TIME ? t.timeLeft / FADE_TIME : 1f;
                int y = sh / 2 - (H + PAD) * (_toasts.Count - 1 - i);

                // Fundo colorido
                Color bg  = t.col; bg.a = alpha * 0.85f;
                DrawRect(new Rect(x, y, W, H), new Color(0.05f,0.06f,0.10f, alpha*0.88f));
                DrawRect(new Rect(x, y, 5, H), bg);  // faixa lateral colorida

                // Ícone
                _sIcon.normal.textColor = new Color(t.col.r, t.col.g, t.col.b, alpha);
                GUI.Label(new Rect(x + 10, y, 34, H), t.icon, _sIcon);

                // Nome da civ
                _sCivName.normal.textColor = new Color(1f,0.85f,0.35f,alpha);
                GUI.Label(new Rect(x + 46, y + 4, W - 68, 16), t.civName, _sCivName);

                // Texto do evento
                _sText.normal.textColor = new Color(0.88f,0.90f,0.96f, alpha * 0.92f);
                GUI.Label(new Rect(x + 46, y + 20, W - 68, H - 22), t.text, _sText);

                // Botão ×
                var oldColor = GUI.color; GUI.color = new Color(1f,1f,1f,alpha*0.7f);
                if (GUI.Button(new Rect(x + W - 20, y + 2, 18, 18), "×", _sClose))
                    _toasts.RemoveAt(i);
                GUI.color = oldColor;
            }
        }

        void DrawRect(Rect r, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, _texW); GUI.color = old;
        }

        static Color EventColor(CG.EventType t) => t switch
        {
            CG.EventType.Seca            => new Color(0.85f,0.55f,0.10f),
            CG.EventType.Fome            => new Color(0.90f,0.18f,0.10f),
            CG.EventType.ColapsoPop      => new Color(0.60f,0.05f,0.05f),
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
    }
}
