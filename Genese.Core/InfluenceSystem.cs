using System;
using System.IO;

namespace Genese.Core
{
    public enum NudgeType : byte
    {
        Sinal      = 0, // brota recursos na área (pressão ambiental leve)
        Faisca     = 1, // muta criatura próxima
        Inspiracao = 2, // +energia em criaturas próximas
        Protecao   = 3, // escudo temporário: criatura perde 50% menos energia por N ticks
        Pressao    = 4  // altera temperatura/umidade numa área (Alavanca 1 — M12 §4.1)
    }

    /// <summary>
    /// Sistema formal de Atenção (M12). A Atenção é recurso escasso que:
    ///   • Regenera por tempo + fervor religioso (M10) — função do estado, não constante.
    ///   • É gasto por nudges com custo proporcional à escala do efeito.
    ///   • O mapa de foco (Alavanca 3) acumula onde o jogador observa → aumenta
    ///     sutilmente a densidade de eventos naquelas células (efeito acumulativo pequeno).
    /// Toda intervenção é injetada como causa no mundo; a resposta vem das regras da
    /// simulação + lente cultural (M09/M10), nunca como comando direto (GDD §5).
    /// </summary>
    public sealed class InfluenceSystem
    {
        public float Attention { get; private set; } = 80f;

        // Custo por NudgeType (ordem: Sinal, Faísca, Inspiração, Proteção, Pressão)
        public static readonly float[] Cost = { 18f, 20f, 18f, 25f, 30f };

        private float[] _focus;  // mapa de foco por célula (size W×H)
        private int     _W, _H;
        private int     _totalNudges;
        private int     _protectedCreatureId = -1;
        private int     _protectionTicksLeft;

        public int   TotalNudges             => _totalNudges;
        public int   ProtectedCreature       => _protectedCreatureId;
        public bool  HasProtection           => _protectionTicksLeft > 0;

        public float MaxAttention(float fervor) => 100f + fervor * 30f;

        public void Init(int w, int h)
        {
            _W = w; _H = h;
            _focus = new float[w * h];
        }

        /// <summary>Passo de simulação: regenera Atenção e decai o foco/proteção.</summary>
        public void Step(float fervor)
        {
            float max   = MaxAttention(fervor);
            float regen = 0.015f + fervor * 0.008f; // M12 §4: f(tempo + fervor)
            Attention = Math.Min(max, Attention + regen);

            if (_focus != null)
                for (int i = 0; i < _focus.Length; i++)
                    _focus[i] *= 0.985f; // foco decai lentamente

            if (_protectionTicksLeft > 0) _protectionTicksLeft--;
        }

        public bool CanApply(NudgeType type) => Attention >= Cost[(int)type];
        public bool CanApplyCustom(float amount) => Attention >= amount;

        /// <summary>Desconta Atenção e aplica o efeito bruto do nudge na simulação.</summary>
        public bool Spend(NudgeType type)
        {
            if (!CanApply(type)) return false;
            Attention -= Cost[(int)type];
            _totalNudges++;
            return true;
        }

        /// <summary>Gasta uma quantidade customizada de Atenção (poderes divinos).</summary>
        public bool SpendCustom(float amount)
        {
            if (Attention < amount) return false;
            Attention -= amount;
            _totalNudges++;
            return true;
        }

        // ---- Alavanca 3: Direcionamento de Atenção (M12 §4.3) ----

        /// <summary>
        /// Regista observação numa célula — acumula foco. Chamado pelo jogador ao
        /// manter o cursor ou clicar na área. Efeito derivado do tempo de foco.
        /// </summary>
        public void AddFocus(int x, int y, float amount = 0.5f)
        {
            if (_focus == null || x < 0 || y < 0 || x >= _W || y >= _H) return;
            _focus[y * _W + x] = Math.Min(12f, _focus[y * _W + x] + amount);
        }

        /// <summary>
        /// Multiplicador de densidade de eventos na célula (M12 §4.3).
        /// Valor pequeno e derivado do tempo de foco acumulado — não de sorteio.
        /// </summary>
        public float FocusMultiplier(int x, int y)
        {
            if (_focus == null || x < 0 || y < 0 || x >= _W || y >= _H) return 1f;
            return 1f + _focus[y * _W + x] * 0.03f;
        }

        public float FocusAt(int x, int y)
        {
            if (_focus == null || x < 0 || y < 0 || x >= _W || y >= _H) return 0f;
            return _focus[y * _W + x];
        }

        // ---- Nudge Proteção ----
        public void ApplyProtection(int creatureId, int durationTicks = 80)
        {
            _protectedCreatureId = creatureId;
            _protectionTicksLeft = durationTicks;
        }

        /// <summary>Multiplicador de perda de energia para criaturas protegidas.</summary>
        public float EnergyLossMult(int creatureId)
            => (_protectionTicksLeft > 0 && creatureId == _protectedCreatureId) ? 0.4f : 1f;

        // ---- snapshot ----
        public void Write(BinaryWriter w)
        {
            w.Write(Attention); w.Write(_totalNudges);
            w.Write(_W); w.Write(_H);
            w.Write(_protectedCreatureId); w.Write(_protectionTicksLeft);
            if (_focus != null) foreach (var f in _focus) w.Write(f);
        }
        public void ReadInto(BinaryReader r)
        {
            Attention = r.ReadSingle(); _totalNudges = r.ReadInt32();
            _W = r.ReadInt32(); _H = r.ReadInt32();
            _protectedCreatureId = r.ReadInt32(); _protectionTicksLeft = r.ReadInt32();
            _focus = new float[_W * _H];
            for (int i = 0; i < _focus.Length; i++) _focus[i] = r.ReadSingle();
        }
    }
}
