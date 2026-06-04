using System.IO;
using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de InfluenceSystem, PopStats, Chronicle e destinos.</summary>
    public class InfluenceDeepTests
    {
        // ── InfluenceSystem ───────────────────────────────────────────────────
        [Fact] public void InfluenceSystem_CannotSpend_WhenInsufficient()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            // Drain all attention
            while (inf.CanApply(NudgeType.Sinal)) inf.Spend(NudgeType.Sinal);
            Assert.False(inf.CanApply(NudgeType.Sinal));
        }

        [Fact] public void InfluenceSystem_Spend_ReducesAttention()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            float before = inf.Attention;
            inf.Spend(NudgeType.Sinal);
            Assert.True(inf.Attention < before);
        }

        [Fact] public void InfluenceSystem_NudgeCost_AllPositive()
        {
            foreach (var cost in InfluenceSystem.Cost)
                Assert.True(cost > 0f);
        }

        [Fact] public void InfluenceSystem_Protection_AffectsEnergyMult()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            Assert.Equal(1f, inf.EnergyLossMult(0), precision: 3); // no protection
            inf.ApplyProtection(0, 50);
            Assert.True(inf.EnergyLossMult(0) < 1f, "Proteção deveria reduzir perda de energia");
            Assert.Equal(1f, inf.EnergyLossMult(1), precision: 3); // different creature
        }

        [Fact] public void InfluenceSystem_Protection_Expires()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            inf.ApplyProtection(0, 3);
            for (int i = 0; i < 5; i++) inf.Step(0f);
            Assert.Equal(1f, inf.EnergyLossMult(0), precision: 3); // expired
        }

        [Fact] public void InfluenceSystem_Focus_OutOfBounds_NoException()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            inf.AddFocus(-1, -1); // should not throw
            inf.AddFocus(100, 100);
            Assert.Equal(1f, inf.FocusMultiplier(-1, -1));
        }

        [Fact] public void InfluenceSystem_Focus_MaxCaps()
        {
            var inf = new InfluenceSystem(); inf.Init(10, 10);
            for (int i = 0; i < 1000; i++) inf.AddFocus(5, 5, 1f);
            float fm = inf.FocusMultiplier(5, 5);
            Assert.True(fm < 10f, "Multiplicador não tem teto"); // capped at 12*0.03 + 1 = 1.36
        }

        [Fact] public void InfluenceSystem_Serialization_RoundTrip()
        {
            var inf = new InfluenceSystem(); inf.Init(16, 12);
            inf.Spend(NudgeType.Faisca);
            for (int i = 0; i < 10; i++) inf.AddFocus(5, 5);
            inf.ApplyProtection(3, 20);
            for (int i = 0; i < 5; i++) inf.Step(0.5f);

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); inf.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var inf2 = new InfluenceSystem(); inf2.ReadInto(r);

            Assert.Equal(inf.Attention,       inf2.Attention,     precision: 3);
            Assert.Equal(inf.TotalNudges,      inf2.TotalNudges);
            Assert.Equal(inf.FocusAt(5,5),    inf2.FocusAt(5,5), precision: 3);
            Assert.Equal(inf.ProtectedCreature, inf2.ProtectedCreature);
        }

        // ── PopStats ──────────────────────────────────────────────────────────
        [Fact] public void PopStats_EmptyPopulation_NoException()
        {
            PopStats.Compute(new System.Collections.Generic.List<Creature>(),
                out float[] means, out float[] stds);
            Assert.Equal(GeneRegistry.Count, means.Length);
            foreach (var m in means) Assert.Equal(0f, m);
        }

        [Fact] public void PopStats_SingleCreature_MeanEqualsGenome()
        {
            var g = Genome.Founder(new Rng(1UL));
            var c = new Creature(0, g, 0f, 0f); c.Alive = true;
            PopStats.Compute(new System.Collections.Generic.List<Creature>{c},
                out float[] means, out float[] stds);
            for (int i=0;i<GeneRegistry.Count;i++)
                Assert.Equal(g.Values[i], means[i], precision: 5);
            foreach (var s in stds) Assert.Equal(0f, s, precision: 5);
        }

        [Fact] public void PopStats_HomogeneityAlert_HomogenousPop()
        {
            var g = Genome.Founder(new Rng(2UL));
            var creatures = new System.Collections.Generic.List<Creature>();
            for (int i=0;i<10;i++)
            {
                var clone = g.Clone(); // identical genomes
                var c = new Creature(i, clone, i, 0f); c.Alive = true;
                creatures.Add(c);
            }
            PopStats.Compute(creatures, out _, out float[] stds);
            Assert.True(PopStats.HomogeneityAlert(stds), "Pop homogénea deveria disparar alerta");
        }

        [Fact] public void PopStats_MacroStats_InRange()
        {
            var sim = new Simulation(3UL, 32, 24, 16, numCivs:1);
            for (int i=0;i<100;i++) sim.Step();
            PopStats.Compute(sim.Pop.Creatures, out float[] means, out _);
            Assert.InRange(PopStats.Militarismo(means),  0f, 1f);
            Assert.InRange(PopStats.CoesaoSocial(means), 0f, 1f);
            Assert.InRange(PopStats.Inovacao(means),     0f, 1f);
        }

        // ── Destinos finais ───────────────────────────────────────────────────
        [Fact] public void Destiny_Transcendencia_WhenConditionsMet()
        {
            var sim = new Simulation(4UL, 48, 32, 20, numCivs:1);
            // Force transcendence conditions
            sim.Pop.Belief.Step(10, 5, 5, new Rng(1UL)); // raise fervor/org
            for (int i=0;i<200;i++) { sim.Pop.Belief.Step(10,5,5,new Rng((ulong)i)); }
            // Check EvaluateDestiny directly
            if (sim.Pop.Belief.Stage == BeliefStage.Transcendente && sim.Pop.Belief.Organization >= 0.6f)
            {
                var d = EventSystem.EvaluateDestiny(sim.Civs, sim.Env);
                Assert.Equal(DestinyType.Transcendencia, d);
            }
        }

        [Fact] public void Destiny_Divergencia_WhenManyLineages()
        {
            var sim = new Simulation(5UL, 48, 32, 20, numCivs:1);
            // Assign 4 different lineage IDs
            int lin = 0;
            foreach (var c in sim.Pop.Creatures)
            {
                if (c.Alive) { c.Genome.LinhagemId = lin++ % 4; }
            }
            var d = EventSystem.EvaluateDestiny(sim.Civs, sim.Env);
            Assert.Equal(DestinyType.Divergencia, d);
        }

        // ── Integration: Influence affects Simulation Destiny ─────────────────
        [Fact] public void InfluenceInSimulation_AttentionBoundedByFervor()
        {
            var sim = new Simulation(6UL, 32, 24, 16, numCivs:1);
            // Force high fervor
            for (int i=0;i<300;i++) { sim.Pop.Belief.Step(10,5,3,new Rng((ulong)i)); }
            float maxWithFervor = sim.Influence.MaxAttention(sim.Pop.Belief.Fervor);
            Assert.True(maxWithFervor > 100f, $"Fervor deveria aumentar teto: {maxWithFervor}");
            for (int i=0;i<500;i++) sim.Step();
            Assert.True(sim.Influence.Attention <= maxWithFervor + 1f,
                $"Atenção {sim.Influence.Attention} excedeu teto {maxWithFervor}");
        }
    }
}
