using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// Testes-âncora do núcleo (E01 §4). Os de determinismo são bloqueadores
    /// (GDD §1.5): se falharem, há acaso arbitrário a eliminar.
    /// </summary>
    public class CoreTests
    {
        // --- Determinismo de replay: mesma semente + mesmos ticks ⇒ snapshot idêntico ---
        [Fact]
        public void Replay_SameSeed_SameTicks_IdenticalSnapshot()
        {
            var a = new Simulation(12345UL);
            var b = new Simulation(12345UL);
            for (int i = 0; i < 10_000; i++) { a.Step(); b.Step(); }
            Assert.Equal(a.Snapshot(), b.Snapshot());
        }

        [Fact]
        public void Replay_DifferentSeed_DifferentSnapshot()
        {
            var a = new Simulation(1UL);
            var b = new Simulation(2UL);
            for (int i = 0; i < 1_000; i++) { a.Step(); b.Step(); }
            Assert.NotEqual(a.Snapshot(), b.Snapshot());
        }

        // --- Snapshot round-trip: Restore(Snapshot()) reproduz o estado exato ---
        [Fact]
        public void Snapshot_RoundTrip_RestoresExactState()
        {
            var sim = new Simulation(777UL, 32, 24);
            for (int i = 0; i < 777; i++) sim.Step();
            byte[] snap = sim.Snapshot();

            var other = new Simulation(999UL);   // semente diferente de propósito
            other.Restore(snap);

            Assert.Equal(777UL, other.Tick);
            Assert.Equal(snap, other.Snapshot());

            // e continua determinístico após restaurar
            for (int i = 0; i < 500; i++) { sim.Step(); other.Step(); }
            Assert.Equal(sim.Snapshot(), other.Snapshot());
        }

        // --- Sub-streams independentes: adicionar consumo num stream não muda outro ---
        [Fact]
        public void Forks_AreIndependent_OfOtherConsumption()
        {
            var root1 = new Rng(42UL);
            var mut1 = root1.Fork(Streams.Mutation);
            var seqA = Enumerable.Range(0, 6).Select(_ => mut1.NextULong()).ToArray();

            var root2 = new Rng(42UL);
            var dec2 = root2.Fork(Streams.Decision);
            for (int i = 0; i < 50; i++) dec2.NextULong();   // consumo "novo" em outro stream
            var mut2 = root2.Fork(Streams.Mutation);
            var seqB = Enumerable.Range(0, 6).Select(_ => mut2.NextULong()).ToArray();

            Assert.Equal(seqA, seqB);
        }

        [Fact]
        public void Forks_DifferentStreams_DifferentSequences()
        {
            var root = new Rng(7UL);
            var s1 = root.Fork(Streams.Mutation).NextULong();
            var s2 = root.Fork(Streams.Decision).NextULong();
            Assert.NotEqual(s1, s2);
        }

        // --- Limites do RNG ---
        [Fact]
        public void NextDouble_InUnitInterval()
        {
            var rng = new Rng(2024UL);
            for (int i = 0; i < 100_000; i++)
            {
                double d = rng.NextDouble();
                Assert.InRange(d, 0.0, 0.9999999999);
            }
        }

        [Fact]
        public void RangeInt_RespectsBounds()
        {
            var rng = new Rng(2025UL);
            for (int i = 0; i < 100_000; i++)
            {
                int v = rng.Range(3, 9);
                Assert.InRange(v, 3, 8);
            }
        }
    }
}
