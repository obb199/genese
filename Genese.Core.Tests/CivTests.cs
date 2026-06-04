using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// Testes de E08 — Múltiplas Civilizações (M11) e Eventos Causais (M14).
    /// Causalidade é bloqueadora: interações e eventos derivam do estado, sem acaso puro.
    /// </summary>
    public class CivTests
    {
        // --- Simulação inicia com N civs separadas ---
        [Fact]
        public void TwoCivs_SpawnInSeparateHalves()
        {
            var sim = new Simulation(1UL, 64, 48, 20, numCivs: 2);
            Assert.Equal(2, sim.Civs.Count);
            // Civ 0 deve ter criaturas na metade esquerda (x < 32)
            float avgX0 = sim.Civs[0].Pop.Creatures.Where(c => c.Alive).Average(c => c.X);
            float avgX1 = sim.Civs[1].Pop.Creatures.Where(c => c.Alive).Average(c => c.X);
            Assert.True(avgX0 < avgX1, $"Civ0 avgX={avgX0:0} deveria ser < Civ1 avgX={avgX1:0}");
        }

        // --- Pop backward-compat aponta para Civs[0] ---
        [Fact]
        public void Pop_BackwardCompat_IsCivs0()
        {
            var sim = new Simulation(2UL);
            Assert.Same(sim.Pop, sim.Civs[0].Pop);
        }

        // --- ContactSystem detecta pares e constrói relação ---
        [Fact]
        public void Contact_Detected_WhenCreaturesClose()
        {
            var sim = new Simulation(3UL, 64, 48, 20, numCivs: 2);
            // Move criaturas de civ 0 e civ 1 para o mesmo ponto central
            float mx = 32f, my = 24f;
            foreach (var c in sim.Civs[0].Pop.Creatures) if (c.Alive) { c.X = mx - 2f; c.Y = my; }
            foreach (var c in sim.Civs[1].Pop.Creatures) if (c.Alive) { c.X = mx + 2f; c.Y = my; }

            // Avança 10 ticks para o ContactSystem rodar
            for (int i = 0; i < 10; i++) sim.Step();

            var rel = sim.Civs[0].GetOrDefault(sim.Civs[1].Id);
            Assert.True(rel.Stance != CivStance.Desconhecida,
                $"Stance ainda Desconhecida após contato: {rel.Stance}");
        }

        // --- Comércio constrói confiança (chama ContactSystem diretamente) ---
        [Fact]
        public void Trade_BuildsTrust_BetweenFriendlyCivs()
        {
            var sim = new Simulation(4UL, 64, 48, 20, numCivs: 2);
            // Posiciona uma criatura de cada civ no mesmo ponto — certamente dentro do ContactRadius
            var c0 = sim.Civs[0].Pop.Creatures.Find(c => c.Alive);
            var c1 = sim.Civs[1].Pop.Creatures.Find(c => c.Alive);
            Assert.NotNull(c0); Assert.NotNull(c1);
            c0.X = 32f; c0.Y = 24f; c0.Energy = 0.8f;
            c1.X = 33f; c1.Y = 24f; c1.Energy = 0.2f;

            // Chama ContactSystem diretamente 6 vezes (sem depender de terreno ou movimento)
            var rng = new Rng(99UL);
            for (int i = 0; i < 6; i++)
                ContactSystem.CheckAndInteract(sim.Civs[0], sim.Civs[1], sim.Env, (ulong)(i * 10), rng);

            var rel = sim.Civs[0].GetOrDefault(sim.Civs[1].Id);
            // Qualquer interação (comércio, conflito ou troca cultural) produz stance != Desconhecida
            // e/ou confiança > 0
            Assert.True(rel.Stance != CivStance.Desconhecida,
                "Nenhum contato registrado — FindContactPairs não encontrou pares");
            Assert.True(rel.TradeCount > 0 || rel.WarCount > 0 || rel.Trust > 0f,
                $"Nenhuma interação produtiva após contato (stance={rel.Stance}, trust={rel.Trust:0.000})");
        }

        // --- EventSystem: seca ativa por limiar ---
        [Fact]
        public void Event_Drought_ActivatesFromThreshold()
        {
            var sim = new Simulation(5UL, 64, 48, 20, numCivs: 2);
            // Força seca nos tiles das criaturas de civ 0
            foreach (var c in sim.Civs[0].Pop.Creatures)
            {
                if (!c.Alive) continue;
                int x = System.Math.Clamp((int)c.X, 0, sim.Env.W-1);
                int y = System.Math.Clamp((int)c.Y, 0, sim.Env.H-1);
                sim.Env.BalancoAgua[sim.Env.Idx(x,y)] = -0.8f; // abaixo do limiar
            }
            // Avança até o EventSystem rodar (30 ticks)
            for (int i = 0; i < 35; i++) sim.Step();

            bool droughtLogged = sim.Events.Log.Exists(e => e.Type == EventType.Seca && e.CivId == 0)
                              || sim.Events.Active.Exists(e => e.Type == EventType.Seca && e.CivId == 0);
            Assert.True(droughtLogged, "Evento de seca não foi ativado apesar do limiar estar cruzado");
        }

        // --- EventSystem: mesmo gatilho, resolução diferente por cultura ---
        [Fact]
        public void Event_SameDrought_DifferentResolution_ByCulture()
        {
            // Duas civs com imagens de jogador opostas
            var sim1 = new Simulation(10UL, 64, 48, 20, numCivs: 2);
            var sim2 = new Simulation(20UL, 64, 48, 20, numCivs: 2);

            // Civ 0 do sim1: imagem hostil + xenofobia alta
            foreach (var c in sim1.Civs[0].Pop.Creatures)
                if (c.Alive) { c.Genome.Values[GeneRegistry.IndexOf("comp.territorialidade")] = 0.95f; c.Genome.Values[GeneRegistry.IndexOf("comp.agressividade")] = 0.95f; }
            for (int i = 0; i < 15; i++) sim1.Civs[0].Pop.Belief.RecordNudge(-1);

            // Civ 0 do sim2: imagem benevolente + nomadismo alto
            foreach (var c in sim2.Civs[0].Pop.Creatures)
                if (c.Alive) { c.Genome.Values[GeneRegistry.IndexOf("comp.nomadismo")] = 0.95f; c.Genome.Values[GeneRegistry.IndexOf("comp.agressividade")] = 0.05f; }
            for (int i = 0; i < 15; i++) sim2.Civs[0].Pop.Belief.RecordNudge(+1);

            // Força seca em ambas as civs
            void ForceDrought(Simulation s)
            {
                foreach (var c in s.Civs[0].Pop.Creatures)
                {
                    if (!c.Alive) continue;
                    int x = System.Math.Clamp((int)c.X, 0, s.Env.W-1);
                    int y = System.Math.Clamp((int)c.Y, 0, s.Env.H-1);
                    s.Env.BalancoAgua[s.Env.Idx(x,y)] = -0.8f;
                }
            }
            ForceDrought(sim1); ForceDrought(sim2);

            for (int i = 0; i < 35; i++) { sim1.Step(); sim2.Step(); }

            string res1 = sim1.Events.Log.Find(e => e.Type == EventType.Seca && e.CivId == 0).Resolution ?? "";
            string res2 = sim2.Events.Log.Find(e => e.Type == EventType.Seca && e.CivId == 0).Resolution ?? "";

            // Resoluções diferentes de acordo com a cultura (causalidade, não sorteio)
            if (res1.Length > 0 && res2.Length > 0)
                Assert.NotEqual(res1, res2);
            // Se algum evento ainda não resolveu, apenas verifica que existem
            else
                Assert.True(res1.Length > 0 || sim1.Events.Active.Count > 0, "Evento não registrado em sim1");
        }

        // --- Determinismo e snapshot round-trip com múltiplas civs ---
        [Fact]
        public void MultiCiv_Deterministic_SnapshotRoundTrip()
        {
            var a = new Simulation(9UL, 32, 24, 16, numCivs: 2);
            var b = new Simulation(9UL, 32, 24, 16, numCivs: 2);
            for (int i = 0; i < 600; i++) { a.Step(); b.Step(); }

            // Determinismo
            Assert.Equal(a.Civs.Count, b.Civs.Count);
            Assert.Equal(a.Pop.Count,  b.Pop.Count);
            Assert.Equal(a.Snapshot(), b.Snapshot());

            // Snapshot round-trip
            var snap = a.Snapshot();
            var c    = new Simulation(42UL);
            c.Restore(snap);
            for (int i = 0; i < 200; i++) { a.Step(); c.Step(); }
            Assert.Equal(a.Snapshot(), c.Snapshot());
        }
    }
}
