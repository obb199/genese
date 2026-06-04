using System;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes de E02/M01 §9. Determinismo é bloqueador (GDD §1.5).</summary>
    public class GenomeTests
    {
        static Genome Uniform(float v)
        {
            var g = new Genome();
            for (int i = 0; i < g.Values.Length; i++) g.Values[i] = v;
            return g;
        }

        // --- Herança sem mutação: filho é exatamente a combinação dos pais ---
        [Fact]
        public void NoMutation_ChildIsExactCombination()
        {
            var mae = new Genome(); var pai = new Genome();
            mae.Set("corpo.tamanho", 0.2f); pai.Set("corpo.tamanho", 0.6f);   // Media → 0.4
            mae.Set("corpo.textura", 0.3f); pai.Set("corpo.textura", 0.8f);   // Dominante → 0.8
            mae.Set("resistencia.doenca", 0.9f); pai.Set("resistencia.doenca", 0.9f); // Recessivo → 0.81
            mae.Set("comp.curiosidade", 0.4f); pai.Set("comp.curiosidade", 0.6f); // Poligênico base → 0.5

            var rng = new Rng(1).Fork(Streams.Mutation);
            var f = Reproduction.Reproduce(mae, pai, rng, envPressure: 0f, mutationScale: 0f);

            Assert.Equal(0.4f, f.Get("corpo.tamanho"), 5);
            Assert.Equal(0.8f, f.Get("corpo.textura"), 5);
            Assert.Equal(0.81f, f.Get("resistencia.doenca"), 5);
            Assert.Equal(0.5f, f.Get("comp.curiosidade"), 5);
            Assert.Equal(1, f.Geracao);
        }

        // --- Recessivo só se expressa se ambos os pais portarem ---
        [Fact]
        public void Recessive_ExpressesOnlyWhenBothCarry()
        {
            var rng = new Rng(1).Fork(Streams.Mutation);
            var ambos = new Genome(); ambos.Set("resistencia.doenca", 0.9f);
            var so1 = new Genome(); so1.Set("resistencia.doenca", 0.9f);
            var baixo = new Genome(); baixo.Set("resistencia.doenca", 0.1f);

            float comAmbos = Reproduction.Reproduce(ambos, so1, rng, 0f, 0f).Get("resistencia.doenca"); // 0.81
            float comUm = Reproduction.Reproduce(so1, baixo, rng, 0f, 0f).Get("resistencia.doenca");    // 0.09
            Assert.True(comAmbos > 0.7f);
            Assert.True(comUm < 0.2f);
        }

        // --- Determinismo: mesmos pais + mesma semente ⇒ filho idêntico ---
        [Fact]
        public void Reproduce_IsDeterministic()
        {
            var mae = Uniform(0.5f); var pai = Uniform(0.5f);
            var f1 = Reproduction.Reproduce(mae, pai, new Rng(99).Fork(Streams.Mutation), 0.3f);
            var f2 = Reproduction.Reproduce(mae, pai, new Rng(99).Fork(Streams.Mutation), 0.3f);
            Assert.Equal(f1.Values, f2.Values);
        }

        // --- Clamp: nenhum gene sai da faixa após muitas gerações ---
        [Fact]
        public void Genes_StayInRange_OverManyGenerations()
        {
            var rng = new Rng(7).Fork(Streams.Mutation);
            var g = Genome.Founder(new Rng(7));
            for (int gen = 0; gen < 2000; gen++)
                g = Reproduction.Reproduce(g, g, rng, envPressure: 1f); // pressão alta, assexuada
            var defs = GeneRegistry.Defs;
            for (int i = 0; i < defs.Length; i++)
                Assert.InRange(g.Values[i], defs[i].Min, defs[i].Max);
        }

        // --- Frequência de mutação ≈ taxa × fator × pressão (gene não-poligênico) ---
        [Fact]
        public void MutationFrequency_MatchesRate()
        {
            // fator = 0.5 + 1.5*v ; com v=1/3 ⇒ fator=1. Pressão 0 ⇒ envK=1.
            var mae = new Genome(); var pai = new Genome();
            mae.Set("reg.fatorMutacao", 1f / 3f); pai.Set("reg.fatorMutacao", 1f / 3f);
            mae.Set("corpo.cor", 0.5f); pai.Set("corpo.cor", 0.5f); // Media → base 0.5
            int idx = GeneRegistry.IndexOf("corpo.cor");
            float taxa = GeneRegistry.Def(idx).TaxaMutBase; // p esperado ≈ taxa

            var rng = new Rng(2024).Fork(Streams.Mutation);
            int changed = 0, N = 40000;
            for (int i = 0; i < N; i++)
            {
                var f = Reproduction.Reproduce(mae, pai, rng, envPressure: 0f);
                if (Math.Abs(f.Values[idx] - 0.5f) > 1e-6f) changed++;
            }
            double freq = changed / (double)N;
            Assert.InRange(freq, taxa - 0.006, taxa + 0.006); // ~0.022
        }

        // --- Distância genética: 0 consigo; cresce com gerações; linhagens divergem ---
        [Fact]
        public void GeneticDistance_GrowsWithIsolation()
        {
            var founder = Genome.Founder(new Rng(123));
            Assert.Equal(0f, Genome.Distance(founder, founder), 6);

            var rngA = new Rng(10).Fork(Streams.Mutation);
            var rngB = new Rng(20).Fork(Streams.Mutation);
            var a = founder.Clone(); var b = founder.Clone();
            for (int i = 0; i < 800; i++) { a = Reproduction.Reproduce(a, a, rngA, 0.5f); b = Reproduction.Reproduce(b, b, rngB, 0.5f); }

            Assert.True(Genome.Distance(a, founder) > 0.02f);   // acumulou divergência
            Assert.True(Genome.Distance(a, b) > 0.02f);          // linhagens isoladas divergiram
            Assert.True(a.Geracao == 800);
        }
    }
}
