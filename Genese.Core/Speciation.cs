using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Nível de detalhe de simulação (E06/LOD). Permite escalar a simulação
    /// para mobile conforme o tamanho e o foco do jogador (M03 §2.3).
    /// </summary>
    public enum SimLOD
    {
        /// <summary>Todos os sistemas por tick (M02/M04/M05 completos).</summary>
        Pleno,
        /// <summary>Social/especiação esparsos; adequado para regiões fora de foco.</summary>
        Agregado,
        /// <summary>Só sobrevivência/morte; adequado para regiões muito distantes.</summary>
        Dormente
    }

    /// <summary>
    /// Motor evolutivo (M03): distância genômica por par, isolamento reprodutivo e
    /// especiação emergente. Três regras causais — nada é sorteado sem origem no estado:
    ///   1. Taxa de mutação = taxaMutBase × fatorRegulatório × pressãoAmbiental × fatorPop.
    ///   2. Compatibilidade reprodutiva = f(Genome.Distance) → bloqueia cruzamento acima
    ///      de LimiarFertil (espécies distintas; permanente).
    ///   3. Zona cinzenta [LimiarViavel, LimiarFertil]: híbridos raros com nova linhagem.
    /// </summary>
    public sealed class Speciation
    {
        // ---- limiares de incompatibilidade (M03 §4.4) ----------------------------
        /// <summary>Distância genômica média a partir da qual híbridos tornam-se raros.</summary>
        public const float LimiarViavel = 0.28f;
        /// <summary>Distância acima da qual a reprodução é impossível (espécies distintas).</summary>
        public const float LimiarFertil = 0.48f;

        /// <summary>Número de eventos de especiação (cisões de linhagem) registrados.</summary>
        public int EspeciacaoCount;

        // ---- fator de tamanho populacional (M03 §4.1) ----------------------------
        /// <summary>
        /// Populações pequenas mutam e fixam mais rápido (deriva amplificada).
        /// Escala 0.7 (grande) → 2.2 (mínima): aplica como multiplicador de mutationScale.
        /// </summary>
        public static float FatorTamanhoPop(int n) =>
            n <= 3  ? 2.2f :
            n <= 8  ? 1.7f :
            n <= 20 ? 1.3f :
            n <= 60 ? 1.0f : 0.7f;

        // ---- compatibilidade reprodutiva (M03 §4.4) ------------------------------
        /// <summary>
        /// Fator de compatibilidade [0..1] baseado na distância genômica entre dois
        /// indivíduos. 1 = mesma espécie; 0 = isolamento completo.
        /// </summary>
        public static float Compatibilidade(Genome a, Genome b)
        {
            float d = Genome.Distance(a, b);
            if (d >= LimiarFertil) return 0f;
            if (d <= LimiarViavel) return 1f;
            return 1f - (d - LimiarViavel) / (LimiarFertil - LimiarViavel);
        }

        /// <summary>
        /// Retorna verdadeiro se os dois organismos podem reproduzir (d &lt; LimiarFertil).
        /// </summary>
        public static bool PodeReproduzir(Creature a, Creature b)
            => Genome.Distance(a.Genome, b.Genome) < LimiarFertil;

        /// <summary>
        /// Determina o LinhagemId do filho. Se os pais estiverem na zona cinzenta
        /// (LimiarViavel ≤ d &lt; LimiarFertil), cria nova linhagem híbrida.
        /// </summary>
        public static int LinhaDaDescendencia(Genome mae, Genome pai, ref int nextLinId)
        {
            if (mae.LinhagemId == pai.LinhagemId) return mae.LinhagemId;
            float d = Genome.Distance(mae, pai);
            if (d < LimiarViavel)  return mae.LinhagemId; // ainda compatível: herda da mãe
            if (d >= LimiarFertil) return mae.LinhagemId; // bloqueado antes de chegar aqui
            return nextLinId++;                           // zona cinzenta → nova linhagem híbrida
        }

        /// <summary>Conta linhagens vivas distintas (proxy de espécies na população).</summary>
        public static int ContarLinhagens(List<Creature> cs)
        {
            var s = new HashSet<int>();
            for (int i = 0; i < cs.Count; i++) if (cs[i].Alive) s.Add(cs[i].Genome.LinhagemId);
            return s.Count;
        }

        // ---- snapshot -----------------------------------------------------------
        public void Write(BinaryWriter w)    => w.Write(EspeciacaoCount);
        public void ReadInto(BinaryReader r) => EspeciacaoCount = r.ReadInt32();
    }
}
