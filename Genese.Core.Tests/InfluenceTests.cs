using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// Testes de E09 — Influência (M12), Observação/Crônica (M13), PopStats (M02)
    /// e destinos finais (M14).
    /// </summary>
    public class InfluenceTests
    {
        // --- M02: PopStats calcula média/stddev corretos ---
        [Fact]
        public void PopStats_ComputesMeanAndStdDev()
        {
            var sim = new Simulation(1UL, 32, 24, 20);
            for (int i = 0; i < 200; i++) sim.Step();

            PopStats.Compute(sim.Pop.Creatures, out float[] means, out float[] stdDevs);

            Assert.Equal(GeneRegistry.Count, means.Length);
            // Médias devem estar em [0,1]
            foreach (float m in means) Assert.InRange(m, 0f, 1f);
            // Desvios não-negativos
            foreach (float s in stdDevs) Assert.True(s >= 0f, $"stddev negativo: {s}");
        }

        // --- M12: Atenção regenera por tick ---
        [Fact]
        public void Influence_AttentionRegeneratesOverTicks()
        {
            var inf = new InfluenceSystem();
            inf.Init(32, 24);
            float start = inf.Attention;
            // Gastar parte da atenção
            inf.Spend(NudgeType.Sinal);
            float afterSpend = inf.Attention;
            Assert.True(afterSpend < start, "Atenção não diminuiu após nudge");
            // Regenerar por 50 ticks
            for (int i = 0; i < 50; i++) inf.Step(0f);
            Assert.True(inf.Attention > afterSpend, "Atenção não regenerou");
        }

        // --- M12: Fervor amplifica teto de Atenção ---
        [Fact]
        public void Influence_FervorIncreasesMaxAttention()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            float maxLow  = inf.MaxAttention(0.0f);
            float maxHigh = inf.MaxAttention(1.0f);
            Assert.True(maxHigh > maxLow, $"Fervor não aumentou teto: low={maxLow} high={maxHigh}");
        }

        // --- M12: Foco acumula e afeta multiplicador de eventos ---
        [Fact]
        public void Influence_FocusAccumulates_AndAffectsMultiplier()
        {
            var inf = new InfluenceSystem(); inf.Init(32, 24);
            Assert.Equal(1f, inf.FocusMultiplier(5, 5), precision: 3);
            // Acumula foco
            for (int i = 0; i < 10; i++) inf.AddFocus(5, 5, 1f);
            Assert.True(inf.FocusMultiplier(5, 5) > 1f, "Multiplicador não aumentou com foco");
            Assert.Equal(1f, inf.FocusMultiplier(10, 10), precision: 3); // célula sem foco = 1
        }

        // --- M13: Crônica só narra eventos realmente registados (causalidade bloqueadora) ---
        [Fact]
        public void Chronicle_OnlyNarratesRealEvents()
        {
            var sim = new Simulation(5UL, 32, 24, 20);
            // Forçar seca
            foreach (var c in sim.Civs[0].Pop.Creatures)
            {
                if (!c.Alive) continue;
                int x = System.Math.Clamp((int)c.X, 0, sim.Env.W-1);
                int y = System.Math.Clamp((int)c.Y, 0, sim.Env.H-1);
                sim.Env.BalancoAgua[sim.Env.Idx(x,y)] = -0.8f;
            }
            for (int i = 0; i < 35; i++) sim.Step();

            // Crônica deve ter exatamente os mesmos eventos que o log
            int logCount = sim.Events.Log.Count;
            Assert.Equal(logCount, sim.Chronicle.Count);

            // Nenhuma entrada tem texto vazio
            foreach (var e in sim.Chronicle.Entries)
                Assert.False(string.IsNullOrWhiteSpace(e.Text), "Entrada da Crônica sem texto");
        }

        // --- M14: Destino avaliado corretamente ---
        [Fact]
        public void Destiny_EvaluatesFromState()
        {
            var sim = new Simulation(7UL, 32, 24, 20);
            for (int i = 0; i < 200; i++) sim.Step();
            // Destino deve ser Continuidade ou outro válido (nunca inválido)
            var validDestinies = new[]
            {
                DestinyType.Continuidade, DestinyType.Transcendencia, DestinyType.Extincao,
                DestinyType.Fusao, DestinyType.Prosperidade, DestinyType.Estagnacao, DestinyType.Divergencia
            };
            Assert.Contains(sim.Destiny, validDestinies);
        }

        // --- Snapshot round-trip com E09 (determinismo completo) ---
        [Fact]
        public void E09_SnapshotRoundTrip_Deterministic()
        {
            var a = new Simulation(8UL, 32, 24, 16);
            var b = new Simulation(8UL, 32, 24, 16);
            for (int i = 0; i < 400; i++) { a.Step(); b.Step(); }

            Assert.Equal(a.Snapshot(), b.Snapshot());
            Assert.Equal(a.Chronicle.Count, b.Chronicle.Count);

            var snap = a.Snapshot();
            var c    = new Simulation(42UL);
            c.Restore(snap);
            for (int i = 0; i < 100; i++) { a.Step(); c.Step(); }
            Assert.Equal(a.Snapshot(), c.Snapshot());
        }
    }
}
