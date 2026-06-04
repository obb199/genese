using System;
using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes exaustivos do RNG — fundação de todo o determinismo.</summary>
    public class RngDeepTests
    {
        [Fact] public void NextDouble_NeverReturns1()
        {
            var r = new Rng(42UL);
            for (int i = 0; i < 1_000_000; i++) Assert.True(r.NextDouble() < 1.0);
        }

        [Fact] public void NextDouble_NeverReturnsNegative()
        {
            var r = new Rng(99UL);
            for (int i = 0; i < 100_000; i++) Assert.True(r.NextDouble() >= 0.0);
        }

        [Fact] public void RangeDouble_RespectsBounds()
        {
            var r = new Rng(7UL);
            for (int i = 0; i < 100_000; i++)
            {
                double v = r.Range(-5.0, 5.0);
                Assert.True(v >= -5.0 && v < 5.0, $"fora do intervalo: {v}");
            }
        }

        [Fact] public void Chance_FrequencyMatchesProbability()
        {
            var r = new Rng(123UL);
            int hits = 0, trials = 100_000;
            for (int i = 0; i < trials; i++) if (r.Chance(0.3)) hits++;
            double ratio = (double)hits / trials;
            Assert.InRange(ratio, 0.28, 0.32);
        }

        [Fact] public void Chance_Zero_NeverFires()
        {
            var r = new Rng(5UL);
            for (int i = 0; i < 10_000; i++) Assert.False(r.Chance(0.0));
        }

        [Fact] public void Chance_One_AlwaysFires()
        {
            var r = new Rng(5UL);
            for (int i = 0; i < 10_000; i++) Assert.True(r.Chance(1.0));
        }

        [Fact] public void WriteState_ReadState_ContinuesSameSequence()
        {
            var r = new Rng(77UL);
            for (int i = 0; i < 50; i++) r.NextULong();

            using var ms = new System.IO.MemoryStream();
            using var w = new System.IO.BinaryWriter(ms);
            r.WriteState(w);

            var seqA = Enumerable.Range(0, 20).Select(_ => r.NextULong()).ToArray();
            ms.Position = 0;
            using var rd = new System.IO.BinaryReader(ms);
            var r2 = Rng.ReadState(rd);
            var seqB = Enumerable.Range(0, 20).Select(_ => r2.NextULong()).ToArray();
            Assert.Equal(seqA, seqB);
        }

        [Fact] public void Fork_SameSeed_SameSubstream()
        {
            var r1 = new Rng(11UL); var f1 = r1.Fork(42UL);
            var r2 = new Rng(11UL); var f2 = r2.Fork(42UL);
            Assert.Equal(
                Enumerable.Range(0, 10).Select(_ => f1.NextULong()).ToArray(),
                Enumerable.Range(0, 10).Select(_ => f2.NextULong()).ToArray());
        }

        [Fact] public void RangeInt_Uniform_Distribution()
        {
            var r = new Rng(55UL);
            int[] counts = new int[10];
            int n = 500_000;
            for (int i = 0; i < n; i++) counts[r.Range(0, 10)]++;
            // Each bucket should be ~50000; allow ±5%
            foreach (var c in counts) Assert.InRange(c, n / 10 - n / 20, n / 10 + n / 20);
        }

        [Fact] public void Fork_IsIndependentOfRootConsumption()
        {
            // Consuming from root before forking should NOT affect fork output
            var r1 = new Rng(33UL);
            var f1 = r1.Fork(1UL);
            var seq1 = Enumerable.Range(0, 10).Select(_ => f1.NextULong()).ToArray();

            var r2 = new Rng(33UL);
            for (int i = 0; i < 1000; i++) r2.NextULong(); // consume root heavily
            var f2 = r2.Fork(1UL);
            var seq2 = Enumerable.Range(0, 10).Select(_ => f2.NextULong()).ToArray();

            Assert.Equal(seq1, seq2);
        }
    }
}
