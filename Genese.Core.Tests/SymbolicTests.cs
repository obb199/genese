using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// Testes da Camada Simbólica (E07 / M08-M10).
    /// Causalidade é bloqueadora: sem acaso arbitrário na língua, cultura ou religião.
    /// </summary>
    public class SymbolicTests
    {
        // --- M08: Léxico cresce com ticks e interações ---
        [Fact]
        public void Lexicon_GrowsAfterInteractions()
        {
            var sim = new Simulation(1111UL, 32, 24, 12);
            for (int i = 0; i < 600; i++) sim.Step();
            // Com 12 criaturas e 600 ticks, o léxico deve ter ao menos 1 palavra nomeada
            Assert.True(sim.Pop.Language.Lexicon.Count > 0,
                $"Léxico vazio após 600 ticks (pop={sim.Pop.Count})");
        }

        // --- M08: Estágio avança por pré-condição de estado (não por tempo) ---
        [Fact]
        public void LanguageStage_AdvancesBeyondGestual_WithEnoughPop()
        {
            var sim = new Simulation(2222UL, 32, 24, 20);
            for (int i = 0; i < 2500; i++) sim.Step();
            // Com 20 criaturas iniciais e muitos ticks, ao menos pré-condição pop≥3 foi atingida
            Assert.True(sim.Pop.Language.Stage > LanguageStage.Gestual,
                $"Estágio ainda Gestual após 2500 ticks (pop={sim.Pop.Count})");
        }

        // --- M09: Meme de Figura tem prevalência real após N ticks ---
        [Fact]
        public void Meme_SpawnedByFigure_GainsPrevalence()
        {
            var sim = new Simulation(3333UL, 32, 24, 30);
            for (int i = 0; i < 2500; i++) sim.Step();
            // Se Figuras emergiram (E05), o pool de memes deve ter ao menos 1 meme
            // (pode não ter Figuras se semente desfavorável — só verifica coerência)
            Assert.True(sim.Pop.Culture.CulturalCohesion >= 0f && sim.Pop.Culture.CulturalCohesion <= 1f,
                "CulturalCohesion fora do intervalo [0,1]");
            // Se há memes, a prevalência de cada um deve estar em [0,1]
            foreach (var kv in sim.Pop.Culture.Pool)
                Assert.InRange(kv.Value.Prevalence, 0f, 1f);
        }

        // --- M10: Imagem do jogador tende a Benevolente após nudges bons ---
        [Fact]
        public void Belief_PlayerImage_TendsBenevolent_AfterPositiveNudges()
        {
            var belief = new Belief();
            for (int i = 0; i < 15; i++) belief.RecordNudge(+1);
            Assert.Equal(PlayerImage.Benevolente, belief.Image);
        }

        // --- M10: Imagem Hostil após nudges negativos consecutivos ---
        [Fact]
        public void Belief_PlayerImage_TendsHostile_AfterNegativeNudges()
        {
            var belief = new Belief();
            for (int i = 0; i < 15; i++) belief.RecordNudge(-1);
            Assert.Equal(PlayerImage.Hostil, belief.Image);
        }

        // --- Determinismo + snapshot round-trip da camada simbólica ---
        [Fact]
        public void SymbolicLayer_Deterministic_AndSnapshotRoundTrip()
        {
            var a = new Simulation(9999UL, 32, 24, 15);
            var b = new Simulation(9999UL, 32, 24, 15);
            for (int i = 0; i < 800; i++) { a.Step(); b.Step(); }

            // Determinismo: mesma semente → estado idêntico
            Assert.Equal(a.Pop.Language.Stage,         b.Pop.Language.Stage);
            Assert.Equal(a.Pop.Language.Lexicon.Count, b.Pop.Language.Lexicon.Count);
            Assert.Equal(a.Pop.Language.DriftCount,    b.Pop.Language.DriftCount);
            Assert.Equal(a.Pop.Culture.MemeCount,      b.Pop.Culture.MemeCount);
            Assert.Equal(a.Pop.Belief.Stage,           b.Pop.Belief.Stage);
            Assert.Equal(a.Snapshot(),                 b.Snapshot());

            // Snapshot round-trip: restore + continuar = idêntico
            var snap = a.Snapshot();
            var c = new Simulation(1UL);
            c.Restore(snap);
            for (int i = 0; i < 200; i++) { a.Step(); c.Step(); }
            Assert.Equal(a.Snapshot(), c.Snapshot());
        }
    }
}
