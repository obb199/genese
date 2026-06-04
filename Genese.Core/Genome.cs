using System;

namespace Genese.Core
{
    /// <summary>
    /// Genoma de uma criatura (M01 §3.2). Compacto: só o vetor de valores
    /// (alinhado ao GeneRegistry) + metadados de linhagem. Genótipo herdável;
    /// o fenótipo (aparência/comportamento real) é derivado por M02/GDD §8.3.
    /// </summary>
    public sealed class Genome
    {
        public readonly float[] Values;   // indexado por GeneRegistry
        public int Geracao;
        public int LinhagemId;

        public Genome()
        {
            Values = new float[GeneRegistry.Count];
        }

        public float Get(string id) => Values[GeneRegistry.IndexOf(id)];
        public void Set(string id, float v) => Values[GeneRegistry.IndexOf(id)] = v;

        public Genome Clone()
        {
            var g = new Genome { Geracao = Geracao, LinhagemId = LinhagemId };
            Array.Copy(Values, g.Values, Values.Length);
            return g;
        }

        /// <summary>Genoma fundador (geração 0) com valores derivados do RNG semeado.</summary>
        public static Genome Founder(Rng rng, int linhagemId = 0)
        {
            var g = new Genome { Geracao = 0, LinhagemId = linhagemId };
            for (int i = 0; i < g.Values.Length; i++) g.Values[i] = (float)rng.NextDouble();
            return g;
        }

        /// <summary>
        /// Distância genética média (handoff para M03/E06: linhagens isoladas
        /// acumulam distância; ao cruzar um limiar, vira espécie nova).
        /// </summary>
        public static float Distance(Genome a, Genome b)
        {
            double sum = 0;
            for (int i = 0; i < a.Values.Length; i++) sum += Math.Abs(a.Values[i] - b.Values[i]);
            return (float)(sum / a.Values.Length);
        }

        // serialização (E01 snapshot extensível para populações em E04)
        public void Write(System.IO.BinaryWriter w)
        {
            w.Write(Geracao); w.Write(LinhagemId);
            for (int i = 0; i < Values.Length; i++) w.Write(Values[i]);
        }
        public static Genome Read(System.IO.BinaryReader r)
        {
            var g = new Genome { Geracao = r.ReadInt32(), LinhagemId = r.ReadInt32() };
            for (int i = 0; i < g.Values.Length; i++) g.Values[i] = r.ReadSingle();
            return g;
        }
    }
}
