using System;
using System.Collections.Generic;

namespace Genese.Core
{
    /// <summary>
    /// Estatísticas coletivas de traço (M02 §5): média e desvio-padrão de cada gene
    /// sobre a população viva, calculados como FUNÇÃO PURA do estado (não armazenados
    /// — recalculados on-demand). A média é o 'caráter' actual da espécie; o desvio-padrão
    /// é a diversidade interna (matéria-prima para especiação futura, M03).
    /// </summary>
    public static class PopStats
    {
        /// <summary>
        /// Calcula média e desvio-padrão de cada gene sobre as criaturas vivas.
        /// </summary>
        public static void Compute(List<Creature> creatures, out float[] means, out float[] stdDevs)
        {
            int ng = GeneRegistry.Count;
            means   = new float[ng];
            stdDevs = new float[ng];
            int alive = 0;

            for (int i = 0; i < creatures.Count; i++)
            {
                var c = creatures[i]; if (!c.Alive) continue; alive++;
                for (int g = 0; g < ng; g++) means[g] += c.Genome.Values[g];
            }
            if (alive == 0) return;
            for (int g = 0; g < ng; g++) means[g] /= alive;

            for (int i = 0; i < creatures.Count; i++)
            {
                var c = creatures[i]; if (!c.Alive) continue;
                for (int g = 0; g < ng; g++) { float d = c.Genome.Values[g] - means[g]; stdDevs[g] += d * d; }
            }
            for (int g = 0; g < ng; g++) stdDevs[g] = (float)Math.Sqrt(stdDevs[g] / alive);
        }

        /// <summary>
        /// Alerta de homogeneidade (M02 §9 / M13): se o desvio-padrão de qualquer traço
        /// comportamental colapsa abaixo do limiar, a população é frágil a mudanças.
        /// </summary>
        public static bool HomogeneityAlert(float[] stdDevs, float threshold = 0.025f)
        {
            for (int g = 0; g < GeneRegistry.Count; g++)
                if (GeneRegistry.Def(g).Bloco == Bloco.Comportamental && stdDevs[g] < threshold)
                    return true;
            return false;
        }

        /// <summary>
        /// Macro-estatística de 'militarismo' (M02 §5): combina agressividade + territorialidade.
        /// </summary>
        public static float Militarismo(float[] means)
        {
            float agg  = means[GeneRegistry.IndexOf("comp.agressividade")];
            float terr = means[GeneRegistry.IndexOf("comp.territorialidade")];
            return (agg * 0.6f + terr * 0.4f);
        }

        /// <summary>Macro-estatística de 'coesão social': sociabilidade + cooperação.</summary>
        public static float CoesaoSocial(float[] means)
        {
            float soc  = means[GeneRegistry.IndexOf("comp.sociabilidade")];
            float coop = means[GeneRegistry.IndexOf("comp.cooperacao")];
            return (soc * 0.5f + coop * 0.5f);
        }

        /// <summary>Macro-estatística de 'inovação': curiosidade + exploração + aprendizagem.</summary>
        public static float Inovacao(float[] means)
        {
            float cur  = means[GeneRegistry.IndexOf("comp.curiosidade")];
            float expl = means[GeneRegistry.IndexOf("comp.exploracao")];
            float apr  = means[GeneRegistry.IndexOf("comp.aprendizagem")];
            return (cur * 0.35f + expl * 0.35f + apr * 0.30f);
        }
    }
}
