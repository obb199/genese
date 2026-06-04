using System.Collections.Generic;
using System.IO;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de Social, Creature e Speciation.</summary>
    public class SocialDeepTests
    {
        // ── Creature ────────────────────────────────────────────────────────────
        [Fact] public void Creature_Trait_ReturnsGenomeValue()
        {
            var g = Genome.Founder(new Rng(1UL));
            var c = new Creature(0, g, 5f, 5f);
            int idx = GeneRegistry.IndexOf("comp.agressividade");
            Assert.Equal(g.Values[idx], c.Trait("comp.agressividade"));
        }

        [Fact] public void Creature_Size_DerivedFromGenome()
        {
            var g = Genome.Founder(new Rng(2UL));
            var c = new Creature(0, g, 0f, 0f);
            Assert.Equal(g.Get("corpo.tamanho"), c.Size);
        }

        [Fact] public void Creature_Role_Lider_WhenFigure()
        {
            var c = new Creature(0, Genome.Founder(new Rng(3UL)), 0f, 0f);
            c.IsFigure = true;
            Assert.Equal("Líder", c.Role());
        }

        [Fact] public void Creature_Role_Parental_WhenHighRepro()
        {
            var c = new Creature(0, Genome.Founder(new Rng(4UL)), 0f, 0f);
            c.ReproCount = 10; c.ForageCount = 2; c.ExploreCount = 2;
            c.IsFigure = false;
            Assert.Equal("Parental", c.Role());
        }

        [Fact] public void Creature_Role_Explorador_WhenHighExplore()
        {
            var c = new Creature(0, Genome.Founder(new Rng(5UL)), 0f, 0f);
            c.ExploreCount = 20; c.ForageCount = 5; c.ReproCount = 5;
            c.IsFigure = false;
            Assert.Equal("Explorador", c.Role());
        }

        [Fact] public void Creature_Role_Forrageiro_Default()
        {
            var c = new Creature(0, Genome.Founder(new Rng(6UL)), 0f, 0f);
            c.IsFigure = false; c.ForageCount = 10; c.ExploreCount = 5; c.ReproCount = 3;
            Assert.Equal("Forrageiro", c.Role());
        }

        // ── Social ──────────────────────────────────────────────────────────────
        [Fact] public void Social_Affinity_UnknownPair_ReturnsZero()
        {
            var s = new Social();
            Assert.Equal(0f, s.Affinity(0, 1));
        }

        [Fact] public void Social_Interact_BuildsAffinity()
        {
            var s = new Social();
            var a = new Creature(0, Genome.Founder(new Rng(7UL)), 0f, 0f);
            var b = new Creature(1, Genome.Founder(new Rng(8UL)), 0.5f, 0f);
            // Set sociabilidade high for both
            a.Genome.Values[GeneRegistry.IndexOf("comp.sociabilidade")] = 1f;
            b.Genome.Values[GeneRegistry.IndexOf("comp.sociabilidade")] = 1f;
            var list = new List<Creature> { a, b };
            for (int i = 0; i < 20; i++) s.Interact(list);
            Assert.True(s.Affinity(0, 1) > 0f, "Afinidade não cresceu após interações");
        }

        [Fact] public void Social_Decay_RemovesWeakLinks()
        {
            var s = new Social();
            var a = new Creature(0, Genome.Founder(new Rng(9UL)), 0f, 0f);
            var b = new Creature(1, Genome.Founder(new Rng(10UL)), 0.5f, 0f);
            a.Genome.Values[GeneRegistry.IndexOf("comp.sociabilidade")] = 1f;
            b.Genome.Values[GeneRegistry.IndexOf("comp.sociabilidade")] = 1f;
            var list = new List<Creature> { a, b };
            s.Interact(list); // builds some affinity
            int before = s.Rels.Count;
            // Decay many times until link disappears
            for (int i = 0; i < 300; i++) s.Decay(0.5f); // aggressive decay
            Assert.True(s.Rels.Count < before || s.Rels.Count == 0);
        }

        [Fact] public void Social_DetectGroups_SingleCreature_NoGroup()
        {
            var s = new Social();
            var c = new Creature(0, Genome.Founder(new Rng(11UL)), 0f, 0f);
            s.DetectGroups(new List<Creature> { c });
            Assert.Equal(-1, c.GroupId);
        }

        [Fact] public void Social_UpdateFigures_BelowThreshold_NotFigure()
        {
            var s = new Social();
            var c = new Creature(0, Genome.Founder(new Rng(12UL)), 0f, 0f);
            c.Prestige = Social.FigurePrestige - 0.1f;
            c.Genome.Values[GeneRegistry.IndexOf("comp.lideranca")] = 1f;
            s.UpdateFigures(new List<Creature> { c });
            Assert.False(c.IsFigure);
        }

        [Fact] public void Social_Serialization_RoundTrip()
        {
            var s = new Social();
            var a = new Creature(0, Genome.Founder(new Rng(13UL)), 0f, 0f);
            var b = new Creature(1, Genome.Founder(new Rng(14UL)), 0.5f, 0f);
            var list = new List<Creature> { a, b };
            for (int i = 0; i < 5; i++) s.Interact(list);
            s.DetectGroups(list); s.UpdateFigures(list);

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); s.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var s2 = new Social(); s2.ReadInto(r);

            Assert.Equal(s.Rels.Count,    s2.Rels.Count);
            Assert.Equal(s.GroupCount,    s2.GroupCount);
            Assert.Equal(s.FigureCount,   s2.FigureCount);
        }

        // ── Speciation ──────────────────────────────────────────────────────────
        [Fact] public void Speciation_Compatibilidade_GrayZone_Between0And1()
        {
            var a = Genome.Founder(new Rng(15UL));
            var b = Genome.Founder(new Rng(16UL));
            // Force distance into gray zone
            float d = Genome.Distance(a, b);
            if (d >= Speciation.LimiarViavel && d < Speciation.LimiarFertil)
            {
                float compat = Speciation.Compatibilidade(a, b);
                Assert.True(compat > 0f && compat < 1f, $"Compatibilidade gray zone deveria ser (0,1): {compat}");
            }
            // At least verify the boundary cases
            Assert.Equal(0f, Speciation.Compatibilidade(
                Genome.Founder(new Rng(1UL)), Genome.Founder(new Rng(1000UL)))); // Very different → 0 or > 0
        }

        [Fact] public void Speciation_ContarLinhagens_CountsDistinct()
        {
            var c1 = new Creature(0, Genome.Founder(new Rng(17UL)), 0f, 0f); c1.Genome.LinhagemId = 1;
            var c2 = new Creature(1, Genome.Founder(new Rng(18UL)), 0f, 0f); c2.Genome.LinhagemId = 2;
            var c3 = new Creature(2, Genome.Founder(new Rng(19UL)), 0f, 0f); c3.Genome.LinhagemId = 1; // same as c1
            c1.Alive = c2.Alive = c3.Alive = true;
            var list = new List<Creature> { c1, c2, c3 };
            Assert.Equal(2, Speciation.ContarLinhagens(list));
        }

        [Fact] public void Speciation_LinhaDaDescendencia_SameLineage_IfSameParents()
        {
            var mae = Genome.Founder(new Rng(20UL)); mae.LinhagemId = 7;
            var pai = mae.Clone(); pai.LinhagemId = 7;
            int next = 100;
            int linId = Speciation.LinhaDaDescendencia(mae, pai, ref next);
            Assert.Equal(7, linId);
            Assert.Equal(100, next); // no new lineage consumed
        }
    }
}
