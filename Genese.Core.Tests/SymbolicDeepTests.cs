using System.IO;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de Language, Culture e Belief — M08/M09/M10.</summary>
    public class SymbolicDeepTests
    {
        // ── Language ───────────────────────────────────────────────────────────
        [Fact] public void Language_Constructor_GeneratesPhonemes()
        {
            var lang = new Language(new Rng(1UL));
            Assert.True(lang.Phonemes.Length >= 7 && lang.Phonemes.Length <= 10,
                $"Fonemas: {lang.Phonemes.Length}");
        }

        [Fact] public void Language_Name_Consistent()
        {
            var lang = new Language(new Rng(2UL));
            var rng  = new Rng(3UL);
            string form1 = lang.Name("comida", rng);
            string form2 = lang.Name("comida", rng); // already in lexicon
            Assert.Equal(form1, form2);
        }

        [Fact] public void Language_Distance_Zero_ForSameLang()
        {
            var lang = new Language(new Rng(4UL));
            Assert.Equal(0f, Language.Distance(lang, lang), precision: 3);
        }

        [Fact] public void Language_Distance_Symmetric()
        {
            var a = new Language(new Rng(5UL));
            var b = new Language(new Rng(6UL));
            var rng = new Rng(7UL);
            a.Name("comida", rng); b.Name("agua", rng);
            float dAB = Language.Distance(a, b);
            float dBA = Language.Distance(b, a);
            Assert.Equal(dAB, dBA, precision: 4);
        }

        [Fact] public void Language_Distance_GreaterAfterDivergence()
        {
            var rng1 = new Rng(1UL);
            var rng2 = new Rng(2UL);
            var a = new Language(new Rng(8UL));
            var b = new Language(new Rng(8UL)); // same initial phonemes
            float d0 = Language.Distance(a, b);
            // Add words to both — different words → diverge
            for (int i = 0; i < 10; i++) { a.Name($"word{i}", rng1); b.Name($"term{i}", rng2); }
            float d1 = Language.Distance(a, b);
            Assert.True(d1 >= d0, $"Distância deveria ter crescido: {d0} → {d1}");
        }

        [Fact] public void Language_Serialization_RoundTrip()
        {
            var lang = new Language(new Rng(9UL));
            var rng  = new Rng(10UL);
            for (int i = 0; i < 10; i++) lang.Name($"concept{i}", rng);
            lang.Stage = LanguageStage.Proto; lang.DriftCount = 3;

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); lang.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var lang2 = Language.Read(r);

            Assert.Equal(lang.Stage,           lang2.Stage);
            Assert.Equal(lang.DriftCount,      lang2.DriftCount);
            Assert.Equal(lang.Phonemes.Length, lang2.Phonemes.Length);
            Assert.Equal(lang.Lexicon.Count,   lang2.Lexicon.Count);
        }

        [Fact] public void Language_Stage_GestualToVocal_WhenPop3()
        {
            var lang = new Language(new Rng(11UL));
            Assert.Equal(LanguageStage.Gestual, lang.Stage);
            var rng = new Rng(12UL);
            lang.Step(3, 0, 0, 50UL, rng); // pop=3 should trigger Vocal
            Assert.Equal(LanguageStage.Vocal, lang.Stage);
        }

        [Fact] public void Language_Stage_DoesNotAdvance_WhenPop1()
        {
            var lang = new Language(new Rng(13UL));
            var rng  = new Rng(14UL);
            lang.Step(1, 0, 0, 50UL, rng);
            Assert.Equal(LanguageStage.Gestual, lang.Stage); // pop < 3
        }

        // ── Culture ───────────────────────────────────────────────────────────
        [Fact] public void Culture_SpawnSymbol_AddsToPool()
        {
            var c   = new Culture();
            var rng = new Rng(15UL);
            Assert.Equal(0, c.SymbolCount);
            c.SpawnSymbol(SymbolType.Valor, 0.5f, 0, rng);
            Assert.Equal(1, c.SymbolCount);
        }

        [Fact] public void Culture_Dominant_ReturnsHighestScore()
        {
            var c   = new Culture();
            var rng = new Rng(16UL);
            int id1 = c.SpawnSymbol(SymbolType.Valor, 0.8f, 0, rng);
            int id2 = c.SpawnSymbol(SymbolType.Tabu,  0.3f, 1, rng);
            // Force prevalences
            var m1 = c.Pool[id1]; m1.Prevalence = 0.9f; c.Pool[id1] = m1;
            var m2 = c.Pool[id2]; m2.Prevalence = 0.1f; c.Pool[id2] = m2;
            var dom = c.Dominant();
            Assert.True(dom.HasValue);
            Assert.Equal(id1, dom.Value.Id); // higher score wins
        }

        [Fact] public void Culture_Interpret_ReturnsCorrectType()
        {
            var c   = new Culture();
            var rng = new Rng(17UL);
            c.SpawnSymbol(SymbolType.Mito,  0.9f, 0, rng);
            c.SpawnSymbol(SymbolType.Valor, 0.3f, 1, rng);
            var result = c.Interpret(SymbolType.Mito);
            Assert.True(result.HasValue);
            Assert.Equal(SymbolType.Mito, result.Value.Type);
        }

        [Fact] public void Culture_Interpret_ReturnsNull_WhenTypeAbsent()
        {
            var c   = new Culture();
            var rng = new Rng(18UL);
            c.SpawnSymbol(SymbolType.Valor, 0.5f, 0, rng);
            Assert.False(c.Interpret(SymbolType.Rito).HasValue);
        }

        [Fact] public void Culture_CulturalCohesion_InRange()
        {
            var c   = new Culture();
            var rng = new Rng(19UL);
            for (int i = 0; i < 5; i++) c.SpawnSymbol((SymbolType)(i%5), 0.5f, i, rng);
            var creatures = new System.Collections.Generic.List<Creature>();
            c.Propagate(creatures, new Social(), rng);
            Assert.InRange(c.CulturalCohesion, 0f, 1f);
        }

        [Fact] public void Culture_Serialization_RoundTrip()
        {
            var c   = new Culture();
            var rng = new Rng(20UL);
            c.SpawnSymbol(SymbolType.Arte,  0.7f, 0, rng);
            c.SpawnSymbol(SymbolType.Tabu,  0.4f, 1, rng);

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); c.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var c2 = new Culture(); c2.ReadInto(r);

            Assert.Equal(c.SymbolCount,        c2.SymbolCount);
            Assert.Equal(c.CulturalCohesion, c2.CulturalCohesion);
        }

        // ── Belief ────────────────────────────────────────────────────────────
        [Fact] public void Belief_Stage_Advances_Animismo_To_Politeismo()
        {
            var b = new Belief();
            // Force preconditions
            for (int i=0;i<200;i++) b.Step(6, 3, 3, new Rng((ulong)i));
            Assert.True(b.Stage >= BeliefStage.Politeismo,
                $"Estágio deveria ter avançado para Politeismo, está: {b.Stage}");
        }

        [Fact] public void Belief_Image_Trickster_WhenMixed()
        {
            var b = new Belief();
            for (int i=0;i<20;i++) b.RecordNudge(i%2==0?1:-1); // alternating
            Assert.Equal(PlayerImage.Trickster, b.Image);
        }

        [Fact] public void Belief_AttentionBonus_InRange()
        {
            var b = new Belief(); b.Step(5, 2, 2, new Rng(1UL));
            Assert.InRange(b.AttentionBonus, 0f, 0.3f);
        }

        [Fact] public void Belief_Fervor_InRange_AfterManySteps()
        {
            var b = new Belief();
            for (int i=0;i<500;i++) b.Step(10,3,2,new Rng((ulong)i));
            Assert.InRange(b.Fervor, 0f, 1f);
        }

        [Fact] public void Belief_Serialization_RoundTrip()
        {
            var b = new Belief();
            b.RecordNudge(1); b.RecordNudge(-1); b.RecordNudge(0);
            for (int i=0;i<50;i++) b.Step(5,2,1,new Rng((ulong)i));

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); b.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var b2 = new Belief(); b2.ReadInto(r);

            Assert.Equal(b.Stage,        b2.Stage);
            Assert.Equal(b.Image,        b2.Image);
            Assert.Equal(b.Fervor,       b2.Fervor);
            Assert.Equal(b.Dogmatism,    b2.Dogmatism);
            Assert.Equal(b.Organization, b2.Organization);
        }
    }
}
