using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes de E04/M04. Determinismo é bloqueador (GDD §1.5).</summary>
    public class PopulationTests
    {
        static void FillBiome(Environment env, Biome b, float food, float umid)
        {
            for (int i = 0; i < env.Bioma.Length; i++)
            {
                env.Bioma[i] = (byte)b; env.Comida[i] = food; env.Agua[i] = 0.8f;
                env.Altitude[i] = 0.5f; env.BaseTemp[i] = 0.5f; env.BaseUmid[i] = umid; env.BalancoAgua[i] = 0.2f;
            }
        }

        // --- Determinismo: mesma semente ⇒ população idêntica após N ticks ---
        [Fact]
        public void Population_IsDeterministic()
        {
            var a = new Simulation(2024, 48, 32, 40);
            var b = new Simulation(2024, 48, 32, 40);
            for (int i = 0; i < 1500; i++) { a.Step(); b.Step(); }
            Assert.Equal(a.Pop.Count, b.Pop.Count);
            Assert.Equal(a.Snapshot(), b.Snapshot());
        }

        // --- Snapshot round-trip com população ---
        [Fact]
        public void Snapshot_RoundTrip_WithPopulation()
        {
            var sim = new Simulation(11, 40, 28, 30);
            for (int i = 0; i < 600; i++) sim.Step();
            var snap = sim.Snapshot();
            var other = new Simulation(999, 8, 8, 1);
            other.Restore(snap);
            Assert.Equal(sim.Tick, other.Tick);
            Assert.Equal(sim.Pop.Count, other.Pop.Count);
            Assert.Equal(snap, other.Snapshot());
        }

        // --- Abundância ⇒ população cresce (reprodução emergente) ---
        [Fact]
        public void Abundance_GrowsPopulation()
        {
            var sim = new Simulation(5, 40, 28, 20);
            FillBiome(sim.Env, Biome.Floresta, 1f, 0.7f);   // comida farta e úmido (sem seca)
            int inicial = sim.Pop.Count;
            for (int i = 0; i < 600; i++) sim.Step();
            Assert.True(sim.Pop.Count > inicial, $"esperava crescimento; {inicial}→{sim.Pop.Count}");
        }

        // --- Escassez ⇒ população declina (fome). Testa a Population isolada, sem
        //     regeneração do ambiente (comida forçada a 0 a cada tick) ---
        [Fact]
        public void Scarcity_DeclinesPopulation()
        {
            var env = new Environment(40, 28);
            env.Generate(new Rng(5).Fork(Streams.Environment));
            FillBiome(env, Biome.Pradaria, 0f, 0.5f);   // terra firme, mas sem comida
            var pop = new Population();
            pop.Seed(env, new Rng(5).Fork(Streams.Spawn), 60);
            var dec = new Rng(5).Fork(Streams.Decision);
            var mut = new Rng(5).Fork(Streams.Mutation);

            int inicial = pop.Count;
            for (ulong t = 0; t < 400; t++)
            {
                for (int j = 0; j < env.Comida.Length; j++) env.Comida[j] = 0f; // sem comida (não chamamos Env.Step)
                pop.Step(env, t, dec, mut);
            }
            Assert.True(pop.Count < inicial, $"esperava declínio; {inicial}→{pop.Count}");
            Assert.True(pop.Count == 0, $"sem comida, a população deveria morrer; restou {pop.Count}");
        }

        // --- Agentes permanecem em terra (não entram em oceano/montanha) ---
        [Fact]
        public void Creatures_StayOnLand()
        {
            var sim = new Simulation(77, 48, 32, 40);
            for (int i = 0; i < 800; i++) sim.Step();
            foreach (var c in sim.Pop.Creatures)
            {
                int x = (int)c.X, y = (int)c.Y;
                Assert.False(sim.Env.IsBarrier(x, y), "criatura num barreira (água/montanha)");
            }
        }
    }
}
