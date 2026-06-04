using System;

namespace Genese.Core
{
    /// <summary>
    /// Reprodução e herança (M01 §4). O filho é uma função DETERMINÍSTICA dos pais
    /// + um delta de mutação causal (M03): magnitude/probabilidade vêm de
    /// taxaMutBase × fatorMutação × pressão ambiental — nada sorteado "no vácuo"
    /// (GDD §1.5). O delta vem sempre do sub-stream de mutação (Streams.Mutation).
    /// </summary>
    public static class Reproduction
    {
        public const float MutStep = 0.15f;   // magnitude máxima de um passo de mutação

        /// <summary>
        /// Combina dois genomas. Para assexuada/partenogênese, passe mae == pai
        /// (cópia com mutação). <paramref name="mutationScale"/>=0 desliga a mutação
        /// (herança pura, determinística e exata — M01 §9.1).
        /// </summary>
        public static Genome Reproduce(Genome mae, Genome pai, Rng mutRng, float envPressure = 0f, float mutationScale = 1f)
        {
            var defs = GeneRegistry.Defs;
            int fatorIdx = GeneRegistry.IndexOf("reg.fatorMutacao");
            float fator = 0.5f + 1.5f * ((mae.Values[fatorIdx] + pai.Values[fatorIdx]) * 0.5f); // [0.5, 2]
            float envK = 1f + Math.Max(0f, envPressure);

            var child = new Genome();
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                float baseV = Combine(d.Modo, mae.Values[i], pai.Values[i]);
                float p = d.TaxaMutBase * fator * envK * mutationScale; // probabilidade causal

                float delta;
                if (d.Modo == Heranca.Poligenico)
                {
                    // soma de sub-mutações independentes ⇒ traço complexo muda de forma suave
                    float acc = 0f;
                    for (int k = 0; k < 4; k++)
                        if (p > 0f && mutRng.NextDouble() < p) acc += (float)(mutRng.NextDouble() * 2.0 - 1.0) * MutStep;
                    delta = acc * 0.25f;
                }
                else
                {
                    delta = (p > 0f && mutRng.NextDouble() < p) ? (float)(mutRng.NextDouble() * 2.0 - 1.0) * MutStep : 0f;
                }

                child.Values[i] = Clamp(baseV + delta, d.Min, d.Max);
            }

            child.Geracao = Math.Max(mae.Geracao, pai.Geracao) + 1;
            child.LinhagemId = mae.LinhagemId; // M03 decide cisões de linhagem depois
            return child;
        }

        /// <summary>Combinação gene a gene segundo o modo de herança (M01 §4.1).</summary>
        public static float Combine(Heranca modo, float a, float b)
        {
            switch (modo)
            {
                case Heranca.Dominante: return Math.Max(a, b);              // expressão tudo-ou-nada
                case Heranca.Recessivo: return a * b;                      // só alto se AMBOS portarem
                case Heranca.Poligenico: return (a + b) * 0.5f;            // base suave (mutação fina à parte)
                default: return (a + b) * 0.5f;                           // Media
            }
        }

        private static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    }
}
