using System;
using System.IO;

namespace Genese.Core
{
    public enum BeliefStage : byte { Animismo, Politeismo, Monoteismo, Transcendente }
    public enum PlayerImage : byte { Neutro, Benevolente, Hostil, Trickster, Adormecido }

    /// <summary>
    /// Sistema religioso emergente (M10). Subcategoria da camada cultural voltada ao
    /// inexplicável: catástrofes, astros, e a presença do jogador. A imagem do jogador
    /// é DERIVADA do histórico de intervenções lido pela lente cultural (nunca sorteada).
    /// Cinco dimensões (fervor, dogmatismo, organização, imagem, estágio) evoluem por
    /// pré-condições de estado; fervor realimenta a Atenção disponível ao jogador (M12).
    /// </summary>
    public sealed class Belief
    {
        public BeliefStage Stage  = BeliefStage.Animismo;
        public PlayerImage Image  = PlayerImage.Adormecido;
        public float Fervor       = 0.10f;   // 0-1: intensidade religiosa coletiva
        public float Dogmatism    = 0.20f;   // 0-1: resistência à mudança de crença
        public float Organization = 0.00f;   // 0-1: estrutura institucional (clero/sacerdócio)

        // Histórico circular dos últimos 20 resultados percebidos de nudge do jogador:
        // +1 = benigno (comida brotou, energia subiu), -1 = maligno, 0 = indiferente
        private readonly sbyte[] _history = new sbyte[20];
        private int _cursor;
        private int _totalNudges;

        /// <summary>
        /// Registra o resultado percebido de uma intervenção do jogador (M10 §4.3).
        /// Chame após cada nudge: outcome +1 (benigno), -1 (maligno), 0 (indiferente).
        /// Atualiza a imagem do jogador de forma causal (sem sorteio).
        /// </summary>
        public void RecordNudge(int outcome)
        {
            _history[_cursor % 20] = (sbyte)Math.Max(-1, Math.Min(1, outcome));
            _cursor = (_cursor + 1) % 20;
            _totalNudges++;
            DeriveImage();
        }

        void DeriveImage()
        {
            int n = Math.Min(_totalNudges, 20), pos = 0, neg = 0;
            for (int i = 0; i < n; i++) { if (_history[i] > 0) pos++; else if (_history[i] < 0) neg++; }
            float posR = n > 0 ? (float)pos / n : 0f, negR = n > 0 ? (float)neg / n : 0f;
            Image = (posR >= 0.6f) ? PlayerImage.Benevolente :
                    (negR >= 0.6f) ? PlayerImage.Hostil :
                    (pos > 0 && neg > 0 && Math.Abs(posR - negR) < 0.25f) ? PlayerImage.Trickster :
                    (_totalNudges < 3) ? PlayerImage.Adormecido :
                    PlayerImage.Neutro;
        }

        /// <summary>
        /// Passo da camada religiosa: evolui fervor, organização, dogmatismo e avança
        /// estágio por pré-condições causais (M10 §4.2).
        /// </summary>
        public void Step(int popCount, int groupCount, int figureCount, Rng rng)
        {
            // Fervor: sustentado por grupos coesos e figuras (comunidade + líderes)
            float targetFervor = Clamp01(0.08f
                + 0.28f * Clamp01(figureCount * 0.25f)
                + 0.18f * Clamp01(groupCount  * 0.20f));
            Fervor = Clamp01(Fervor + (targetFervor - Fervor) * 0.018f);

            // Organização: emerge do controle de figuras e do estágio (M05/M07)
            float targetOrg = Clamp01(figureCount * 0.14f + (float)Stage * 0.12f);
            Organization = Clamp01(Organization + (targetOrg - Organization) * 0.012f);

            // Dogmatismo: cresce com fervor, limitado pelo índice de aprendizagem médio
            Dogmatism = Clamp01(Dogmatism * 0.9985f + Fervor * 0.0018f);

            AdvanceStage(popCount, figureCount);
        }

        void AdvanceStage(int pop, int figures)
        {
            Stage = Stage switch
            {
                BeliefStage.Animismo   when pop     >= 4  && Fervor       >= 0.20f                              => BeliefStage.Politeismo,
                BeliefStage.Politeismo when figures >= 1  && Organization >= 0.22f                              => BeliefStage.Monoteismo,
                BeliefStage.Monoteismo when figures >= 2  && Organization >= 0.45f && Dogmatism >= 0.50f        => BeliefStage.Transcendente,
                _ => Stage
            };
        }

        /// <summary>
        /// Bônus de Atenção (M12): fervor alto amplifica as intervenções do jogador.
        /// Valor em [0, 0.3] — somado à Atenção base no HUD.
        /// </summary>
        public float AttentionBonus => Fervor * 0.30f;

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        // ----- snapshot -----
        public void Write(BinaryWriter w)
        {
            w.Write((byte)Stage); w.Write((byte)Image);
            w.Write(Fervor); w.Write(Dogmatism); w.Write(Organization);
            w.Write(_cursor); w.Write(_totalNudges);
            for (int i = 0; i < 20; i++) w.Write(_history[i]);
        }
        public void ReadInto(BinaryReader r)
        {
            Stage        = (BeliefStage)r.ReadByte(); Image = (PlayerImage)r.ReadByte();
            Fervor       = r.ReadSingle(); Dogmatism = r.ReadSingle(); Organization = r.ReadSingle();
            _cursor      = r.ReadInt32();  _totalNudges = r.ReadInt32();
            for (int i = 0; i < 20; i++) _history[i] = r.ReadSByte();
        }
    }
}
