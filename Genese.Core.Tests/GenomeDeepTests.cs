using System;
using System.IO;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de Genome, GeneRegistry e Reproduction.</summary>
    public class GenomeDeepTests
    {
        // ── GeneRegistry ────────────────────────────────────────────────────────
        [Fact] public void GeneRegistry_AllGenesHaveValidMinMax()
        {
            for (int i = 0; i < GeneRegistry.Count; i++)
            {
                var d = GeneRegistry.Def(i);
                Assert.True(d.Min < d.Max, $"Gene {d.Id}: min({d.Min}) >= max({d.Max})");
                Assert.True(d.TaxaMutBase >= 0f && d.TaxaMutBase <= 1f,
                    $"Gene {d.Id}: taxaMut={d.TaxaMutBase} fora de [0,1]");
            }
        }

        [Fact] public void GeneRegistry_IndexOf_AllGenes()
        {
            for (int i = 0; i < GeneRegistry.Count; i++)
            {
                var id = GeneRegistry.Def(i).Id;
                Assert.Equal(i, GeneRegistry.IndexOf(id));
            }
        }

        // ── Genome ──────────────────────────────────────────────────────────────
        [Fact] public void Genome_Founder_ValuesInRange()
        {
            var g = Genome.Founder(new Rng(1UL));
            for (int i = 0; i < GeneRegistry.Count; i++)
            {
                var d = GeneRegistry.Def(i);
                Assert.InRange(g.Values[i], d.Min, d.Max);
            }
        }

        [Fact] public void Genome_Clone_IsDeepCopy()
        {
            var original = Genome.Founder(new Rng(2UL));
            var clone    = original.Clone();
            clone.Values[0] = original.Values[0] + 0.5f;
            Assert.NotEqual(original.Values[0], clone.Values[0]);
        }

        [Fact] public void Genome_Distance_Zero_ForIdentical()
        {
            var g = Genome.Founder(new Rng(3UL));
            Assert.Equal(0f, Genome.Distance(g, g));
        }

        [Fact] public void Genome_Distance_Symmetric()
        {
            var a = Genome.Founder(new Rng(4UL));
            var b = Genome.Founder(new Rng(5UL));
            float dAB = Genome.Distance(a, b);
            float dBA = Genome.Distance(b, a);
            Assert.Equal(dAB, dBA, precision: 5);
        }

        [Fact] public void Genome_Distance_NonNegative()
        {
            for (int i = 0; i < 20; i++)
            {
                var a = Genome.Founder(new Rng((ulong)i));
                var b = Genome.Founder(new Rng((ulong)(i + 100)));
                Assert.True(Genome.Distance(a, b) >= 0f);
            }
        }

        [Fact] public void Genome_Serialization_RoundTrip()
        {
            var original = Genome.Founder(new Rng(6UL));
            original.LinhagemId = 5; original.Geracao = 12;

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            original.Write(w);

            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var restored = Genome.Read(r);

            Assert.Equal(original.Values, restored.Values);
            Assert.Equal(original.LinhagemId, restored.LinhagemId);
            Assert.Equal(original.Geracao,    restored.Geracao);
        }

        // ── Reproduction ────────────────────────────────────────────────────────
        [Fact] public void Reproduce_Dominant_Mode_MaxOfParents()
        {
            var mae = Genome.Founder(new Rng(7UL));
            var pai = Genome.Founder(new Rng(8UL));
            var rng = new Rng(9UL).Fork(Streams.Mutation);
            var child = Reproduction.Reproduce(mae, pai, rng, 0f, mutationScale: 0f);

            int idx = GeneRegistry.IndexOf("corpo.textura"); // Dominante
            float expected = Math.Max(mae.Values[idx], pai.Values[idx]);
            Assert.Equal(expected, child.Values[idx]);
        }

        [Fact] public void Reproduce_Media_Mode_AverageOfParents()
        {
            var mae = Genome.Founder(new Rng(10UL));
            var pai = Genome.Founder(new Rng(11UL));
            var rng = new Rng(12UL).Fork(Streams.Mutation);
            var child = Reproduction.Reproduce(mae, pai, rng, 0f, mutationScale: 0f);

            int idx = GeneRegistry.IndexOf("corpo.tamanho"); // Media
            float expected = (mae.Values[idx] + pai.Values[idx]) * 0.5f;
            Assert.Equal(expected, child.Values[idx], precision: 5);
        }

        [Fact] public void Reproduce_Recessivo_Mode_ProductOfParents()
        {
            var mae = Genome.Founder(new Rng(13UL));
            var pai = Genome.Founder(new Rng(14UL));
            var rng = new Rng(15UL).Fork(Streams.Mutation);
            var child = Reproduction.Reproduce(mae, pai, rng, 0f, mutationScale: 0f);

            int idx = GeneRegistry.IndexOf("resistencia.doenca"); // Recessivo
            float expected = mae.Values[idx] * pai.Values[idx];
            Assert.Equal(expected, child.Values[idx], precision: 5);
        }

        [Fact] public void Reproduce_ZeroMutationScale_NoMutation()
        {
            var mae = Genome.Founder(new Rng(16UL));
            var pai = mae.Clone();
            var rng = new Rng(17UL).Fork(Streams.Mutation);
            var child = Reproduction.Reproduce(mae, pai, rng, 1f, mutationScale: 0f);
            Assert.Equal(mae.Values, child.Values);
        }

        [Fact] public void Reproduce_Geracao_IncrementsByOne()
        {
            var mae = Genome.Founder(new Rng(18UL)); mae.Geracao = 5;
            var rng = new Rng(19UL).Fork(Streams.Mutation);
            var child = Reproduction.Reproduce(mae, mae, rng, 0f);
            Assert.Equal(6, child.Geracao);
        }

        [Fact] public void Reproduce_ChildValues_AlwaysInRange()
        {
            var rng = new Rng(20UL).Fork(Streams.Mutation);
            var g   = Genome.Founder(new Rng(21UL));
            for (int i = 0; i < 200; i++)
            {
                g = Reproduction.Reproduce(g, g, rng, 1.5f);
                for (int gi = 0; gi < GeneRegistry.Count; gi++)
                {
                    var d = GeneRegistry.Def(gi);
                    Assert.InRange(g.Values[gi], d.Min, d.Max);
                }
            }
        }
    }
}
