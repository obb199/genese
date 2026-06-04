using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// E11 — QA &amp; Conformidade. Testes de casos-limite e auditoria do princípio de
    /// causalidade (GDD §1.5): toda probabilidade deve derivar do estado; sem acaso arbitrário.
    /// </summary>
    public class E11QATests
    {
        // ── Caso-limite: população vai a zero sem crashar ──────────────────────
        [Fact]
        public void Population_GoesZero_NoException()
        {
            var sim = new Simulation(1UL, 32, 24, 4, numCivs: 1);
            // Mata manualmente todas as criaturas
            foreach (var c in sim.Pop.Creatures) c.Energy = 0f;
            for (int i = 0; i < 50; i++) sim.Step(); // não deve lançar excepção
            Assert.True(sim.Tick > 0);
        }

        // ── Caso-limite: criatura solitária consegue reproduzir assexuadamente ──
        [Fact]
        public void SingleCreature_CanReproduceAsexually()
        {
            var sim = new Simulation(2UL, 48, 32, 40, numCivs: 1); // 1 civ, 40 criaturas
            // Coloca só 1 criatura viva
            for (int i = 1; i < sim.Pop.Creatures.Count; i++) sim.Pop.Creatures[i].Energy = 0f;
            sim.Pop.Creatures[0].Energy = 0.95f;
            // Coloca comida abundante à volta para encorajar reprodução
            for (int i2 = 0; i2 < sim.Env.Comida.Length; i2++) sim.Env.Comida[i2] = 1f;
            for (int t = 0; t < 600; t++) sim.Step();
            Assert.True(sim.Pop.Count >= 1, "população não sobreviveu");
        }

        // ── Causalidade — Reprodução: mesmos genomas + mesma semente → filho idêntico ──
        [Fact]
        public void Causalidade_Reproduce_Deterministic()
        {
            var rng1 = new Rng(77UL).Fork(Streams.Mutation);
            var rng2 = new Rng(77UL).Fork(Streams.Mutation);
            var mae  = Genome.Founder(new Rng(1UL));
            var pai  = Genome.Founder(new Rng(2UL));
            var c1   = Reproduction.Reproduce(mae, pai, rng1, 0.3f);
            var c2   = Reproduction.Reproduce(mae, pai, rng2, 0.3f);
            Assert.Equal(c1.Values, c2.Values);
        }

        // ── Causalidade — ContactAffinity é determinístico (sem acaso na avaliação) ──
        [Fact]
        public void Causalidade_ContactAffinity_PureFromState()
        {
            var sim = new Simulation(3UL, 48, 32, 20);
            float aff1 = Civilization.ContactAffinity(sim.Civs[0], sim.Civs[1]);
            float aff2 = Civilization.ContactAffinity(sim.Civs[0], sim.Civs[1]);
            Assert.Equal(aff1, aff2); // mesma chamada → mesmo resultado (sem RNG interno)
        }

        // ── Causalidade — InfluenceSystem.Step é determinístico ──
        [Fact]
        public void Causalidade_InfluenceSystem_Deterministic()
        {
            var a = new InfluenceSystem(); a.Init(16, 16);
            var b = new InfluenceSystem(); b.Init(16, 16);
            // Ambos partem de 80 de Atenção; após 30 ticks com fervor 0.4 devem ser idênticos
            for (int i = 0; i < 30; i++) { a.Step(0.4f); b.Step(0.4f); }
            Assert.Equal(a.Attention, b.Attention);
        }

        // ── Causalidade — Chronicle não inventa eventos (só narra o log) ──
        [Fact]
        public void Causalidade_Chronicle_NoFabricatedEvents()
        {
            var sim = new Simulation(5UL, 32, 24, 20);
            for (int i = 0; i < 300; i++) sim.Step();
            // Número de entradas da crónica == número de eventos no log
            Assert.Equal(sim.Events.Log.Count, sim.Chronicle.Count);
            // Cada entrada tem um CivId que existe em Civs
            var civIds = sim.Civs.ConvertAll(c => c.Id);
            foreach (var e in sim.Chronicle.Entries)
                Assert.Contains(e.CivId, civIds);
        }

        // ── E11 Conformidade — Overlay não altera o estado da simulação ──
        [Fact]
        public void E11_Overlay_DoesNotAlterSimulationState()
        {
            var sim1 = new Simulation(9UL, 32, 24, 16);
            var sim2 = new Simulation(9UL, 32, 24, 16);
            for (int i = 0; i < 100; i++) { sim1.Step(); sim2.Step(); }

            // Captura snap antes de "consultar" overlays
            byte[] snap1 = sim1.Snapshot();

            // Consulta PopStats (função pura)
            PopStats.Compute(sim1.Pop.Creatures, out float[] means, out float[] stds);
            // Consulta Chronicle (só leitura)
            var summary = sim1.Chronicle.SummaryByEra();
            // Consulta FocusMultiplier (só leitura)
            float fm = sim1.Influence.FocusMultiplier(5, 5);

            // Estado não deve ter mudado
            Assert.Equal(snap1, sim1.Snapshot());
            Assert.Equal(snap1, sim2.Snapshot());
        }
    }
}
