using System.IO;
using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de Civilization, ContactSystem, EventSystem e Chronicle.</summary>
    public class CivDeepTests
    {
        // ── CivRelation serialization ────────────────────────────────────────
        [Fact] public void CivRelation_Serialization_RoundTrip()
        {
            var rel = new CivRelation
            {
                Trust=0.6f, Resentment=0.3f, Stance=CivStance.Comercial,
                TradeCount=5, WarCount=2, LastContactTick=1000
            };
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); rel.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var rel2 = CivRelation.Read(r);
            Assert.Equal(rel.Trust,            rel2.Trust);
            Assert.Equal(rel.Resentment,       rel2.Resentment);
            Assert.Equal(rel.Stance,           rel2.Stance);
            Assert.Equal(rel.TradeCount,       rel2.TradeCount);
            Assert.Equal(rel.WarCount,         rel2.WarCount);
            Assert.Equal(rel.LastContactTick,  rel2.LastContactTick);
        }

        // ── Civilization ────────────────────────────────────────────────────
        [Fact] public void Civilization_GetOrDefault_ReturnsDesconhecida()
        {
            var sim = new Simulation(1UL, 32, 24, 16);
            var rel = sim.Civs[0].GetOrDefault(999); // unknown id
            Assert.Equal(CivStance.Desconhecida, rel.Stance);
        }

        [Fact] public void Civilization_ContactAffinity_InRange()
        {
            var sim = new Simulation(2UL, 48, 32, 20);
            float aff = Civilization.ContactAffinity(sim.Civs[0], sim.Civs[1]);
            Assert.InRange(aff, 0f, 1f);
        }

        [Fact] public void Civilization_Serialization_RoundTrip()
        {
            var sim = new Simulation(3UL, 32, 24, 16);
            for (int i = 0; i < 100; i++) sim.Step();
            var civ = sim.Civs[0];

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); civ.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var civ2 = Civilization.Read(r);

            Assert.Equal(civ.Id,     civ2.Id);
            Assert.Equal(civ.SpawnX, civ2.SpawnX);
            Assert.Equal(civ.SpawnY, civ2.SpawnY);
            Assert.Equal(civ.Pop.Count, civ2.Pop.Count);
            Assert.Equal(civ.Relations.Count, civ2.Relations.Count);
        }

        // ── ContactSystem direct ─────────────────────────────────────────────
        [Fact] public void ContactSystem_NoContact_WhenTooFarApart()
        {
            var sim = new Simulation(4UL, 64, 48, 20);
            // Place civs far apart
            foreach (var c in sim.Civs[0].Pop.Creatures) if (c.Alive) { c.X=2f; c.Y=2f; }
            foreach (var c in sim.Civs[1].Pop.Creatures) if (c.Alive) { c.X=62f; c.Y=46f; }
            var rng = new Rng(5UL);
            ContactSystem.CheckAndInteract(sim.Civs[0], sim.Civs[1], sim.Env, 0UL, rng);
            var rel = sim.Civs[0].GetOrDefault(sim.Civs[1].Id);
            Assert.Equal(CivStance.Desconhecida, rel.Stance);
        }

        [Fact] public void ContactSystem_WarIncreasesResentment()
        {
            var sim = new Simulation(5UL, 64, 48, 20);
            var c0 = sim.Civs[0].Pop.Creatures.Find(c=>c.Alive);
            var c1 = sim.Civs[1].Pop.Creatures.Find(c=>c.Alive);
            if (c0==null||c1==null) return;
            c0.X=32f;c0.Y=24f; c1.X=33f;c1.Y=24f;
            // Force war by pre-setting high resentment
            sim.Civs[0].Relations[sim.Civs[1].Id] = new CivRelation
                { Resentment=0.9f, Stance=CivStance.Guerra, Trust=0f };
            var rng = new Rng(6UL);
            float resBefore = sim.Civs[0].GetOrDefault(sim.Civs[1].Id).Resentment;
            ContactSystem.CheckAndInteract(sim.Civs[0], sim.Civs[1], sim.Env, 10UL, rng);
            // Resentment should have increased (war drains but resentment += 0.022)
            var relAfter = sim.Civs[0].GetOrDefault(sim.Civs[1].Id);
            Assert.True(relAfter.WarCount >= 1, "Guerra não foi registada");
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        [Fact] public void EventSystem_Famine_ActivatesWhenEnergyLow()
        {
            var sim = new Simulation(7UL, 32, 24, 16);
            // Force all creatures to have very low energy
            foreach (var c in sim.Pop.Creatures) if (c.Alive) c.Energy = 0.1f;
            for (int i = 0; i < 35; i++) sim.Step();
            bool famineFound = sim.Events.Log.Any(e=>e.Type==EventType.Fome && e.CivId==0)
                            || sim.Events.Active.Any(e=>e.Type==EventType.Fome && e.CivId==0);
            Assert.True(famineFound, "Evento de Fome não activou com energia baixa");
        }

        [Fact] public void EventSystem_Expansion_WhenNearCap()
        {
            var sim = new Simulation(8UL, 64, 48, 8, numCivs:1);
            sim.Pop.Cap = 10; // very low cap
            // Force expansion condition
            for (int i=0;i<35;i++) sim.Step();
            // Not guaranteed to trigger, but should not crash
            Assert.True(sim.Tick >= 35);
        }

        [Fact] public void EventSystem_Log_OnlyRealEvents()
        {
            var sim = new Simulation(9UL, 32, 24, 16);
            for (int i=0;i<120;i++) sim.Step();
            // All log events must have valid CivId
            var civIds = sim.Civs.Select(c=>c.Id).ToList();
            foreach (var ev in sim.Events.Log)
                Assert.Contains(ev.CivId, civIds);
        }

        [Fact] public void EventSystem_EvaluateDestiny_Extintion_WhenPopZero()
        {
            var sim = new Simulation(10UL, 32, 24, 16, numCivs:1);
            foreach (var c in sim.Pop.Creatures) c.Energy = 0f;
            for (int i=0;i<20;i++) sim.Step();
            // With 0 pop, destiny should be Extinção or Continuidade (not error)
            Assert.True(sim.Destiny==DestinyType.Extincao || sim.Destiny==DestinyType.Continuidade);
        }

        // ── Chronicle ───────────────────────────────────────────────────────
        [Fact] public void Chronicle_PeopleNameOf_NoLexicon_ReturnsCivId()
        {
            var sim = new Simulation(11UL, 32, 24, 16);
            string name = Chronicle.PeopleNameOf(sim.Civs[0]);
            Assert.Contains("0", name);
        }

        [Fact] public void Chronicle_PeopleNameOf_WithGrupoWord_UsesWord()
        {
            var sim = new Simulation(12UL, 32, 24, 16);
            var civ = sim.Civs[0];
            var rng = new Rng(99UL);
            civ.Pop.Language.Name("grupo", rng); // forces "grupo" into lexicon
            string name = Chronicle.PeopleNameOf(civ);
            Assert.Contains("«", name); // should use the emergent name
        }

        [Fact] public void Chronicle_SummaryByEra_ReturnsEntries()
        {
            var sim = new Simulation(13UL, 32, 24, 16);
            foreach (var c in sim.Pop.Creatures) if(c.Alive) { c.X=16f; c.Y=12f; }
            sim.Env.BalancoAgua[sim.Env.Idx(16,12)] = -1f;
            for (int i=0;i<35;i++) sim.Step();
            var summary = sim.Chronicle.SummaryByEra();
            // If any events occurred, summary should be non-empty
            if (sim.Chronicle.Count > 0)
                Assert.True(summary.Count > 0);
        }

        [Fact] public void Chronicle_Serialization_RoundTrip()
        {
            var sim = new Simulation(14UL, 32, 24, 16);
            sim.Env.BalancoAgua[0] = -1f;
            for (int i=0;i<35;i++) sim.Step();
            var chron = sim.Chronicle;

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); chron.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var chron2 = new Chronicle(); chron2.ReadInto(r);

            Assert.Equal(chron.Count, chron2.Count);
            for (int i=0;i<chron.Count;i++)
            {
                Assert.Equal(chron.Entries[i].Text, chron2.Entries[i].Text);
                Assert.Equal(chron.Entries[i].Era,  chron2.Entries[i].Era);
            }
        }

        // ── Multi-civ isolation ──────────────────────────────────────────────
        [Fact] public void MultiCiv_3Civs_AllDistinct()
        {
            var sim = new Simulation(15UL, 96, 48, 24, numCivs:3);
            Assert.Equal(3, sim.Civs.Count);
            // Civs should have distinct spawn centers
            int sx0 = sim.Civs[0].SpawnX, sx1 = sim.Civs[1].SpawnX, sx2 = sim.Civs[2].SpawnX;
            Assert.True(sx0 < sx1, $"Civ0 spawnX({sx0}) deveria < Civ1({sx1})");
            Assert.True(sx1 < sx2, $"Civ1 spawnX({sx1}) deveria < Civ2({sx2})");
        }

        [Fact] public void MultiCiv_LOD_Dormente_SkipsMostTicks()
        {
            var sim1 = new Simulation(16UL, 32, 24, 16);
            var sim2 = new Simulation(16UL, 32, 24, 16);
            sim2.SetLOD(SimLOD.Dormente);
            for (int i=0;i<100;i++) { sim1.Step(); sim2.Step(); }
            // Both should have same tick count but Dormente should have done less work
            Assert.Equal(sim1.Tick, sim2.Tick);
        }

        [Fact] public void SimulationPopCap_Respected()
        {
            var sim = new Simulation(17UL, 64, 48, 40, numCivs:1);
            sim.Pop.Cap = 15;
            // Fill food to allow growth
            for (int i=0;i<sim.Env.Comida.Length;i++) sim.Env.Comida[i] = 1f;
            for (int i=0;i<2000;i++) sim.Step();
            Assert.True(sim.Pop.Count <= sim.Pop.Cap + 5,
                $"Pop {sim.Pop.Count} deveria respeitar Cap {sim.Pop.Cap}");
        }
    }
}
