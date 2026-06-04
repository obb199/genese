using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes de E05/M05 — estruturas coletivas EMERGEM das interações (sem hardcode).</summary>
    public class SocialTests
    {
        // mundo fértil e plano para focar no social
        static Simulation Make(ulong seed, int n, System.Action<Genome> tweak)
        {
            var sim = new Simulation(seed, 40, 28, 0);
            for (int i = 0; i < sim.Env.Bioma.Length; i++)
            { sim.Env.Bioma[i] = (byte)Biome.Pradaria; sim.Env.Comida[i] = 0.8f; sim.Env.Altitude[i] = 0.5f; sim.Env.BaseTemp[i] = 0.5f; sim.Env.BaseUmid[i] = 0.5f; sim.Env.BalancoAgua[i] = 0.3f; }
            var rng = new Rng(seed).Fork(Streams.Spawn);
            for (int k = 0; k < n; k++)
            {
                var g = Genome.Founder(rng.Fork((ulong)(k + 1)));
                tweak(g);
                float a = rng.NextDouble() < 0.5 ? 0 : 0; // posições espalhadas mas no centro
                sim.Pop.Creatures.Add(new Creature(k, g, 12f + (float)rng.Range(0, 16), 8f + (float)rng.Range(0, 12)));
            }
            return sim;
        }

        // --- Sociáveis formam GRUPOS detectáveis (sem código de "criar grupo") ---
        [Fact]
        public void Sociable_FormGroups()
        {
            var sim = Make(1, 24, g => { g.Set("comp.sociabilidade", 1f); g.Set("comp.cooperacao", 0.9f); g.Set("metabolismo", 0.6f); g.Set("fertilidade", 0f); });
            for (int i = 0; i < 600; i++) sim.Step();
            Assert.True(sim.Pop.Social.GroupCount >= 1, $"sociáveis deveriam formar grupos; veio {sim.Pop.Social.GroupCount}");
            Assert.Contains(sim.Pop.Creatures, c => c.GroupId >= 0);
        }

        // --- Solitários formam MENOS grupos que sociáveis (mesma semente/condições) ---
        [Fact]
        public void Solitary_FormFewerGroups()
        {
            var soc = Make(2, 24, g => { g.Set("comp.sociabilidade", 1f); g.Set("comp.cooperacao", 0.9f); g.Set("fertilidade", 0f); });
            var sol = Make(2, 24, g => { g.Set("comp.sociabilidade", 0f); g.Set("comp.cooperacao", 0f); g.Set("fertilidade", 0f); });
            for (int i = 0; i < 600; i++) { soc.Step(); sol.Step(); }
            Assert.True(soc.Pop.Social.Rels.Count > sol.Pop.Social.Rels.Count, "sociáveis deveriam ter mais laços");
        }

        // --- Hierarquia mensurável: atributo de dominância ⇒ mais vitórias ---
        [Fact]
        public void Dominance_FormsHierarchy()
        {
            // 2 fortes (agressivos/grandes) + 2 fracos, todos juntos
            var sim = new Simulation(3, 20, 16, 0);
            for (int i = 0; i < sim.Env.Bioma.Length; i++) { sim.Env.Bioma[i] = (byte)Biome.Pradaria; sim.Env.Comida[i] = 0.6f; sim.Env.Altitude[i] = 0.5f; sim.Env.BaseUmid[i] = 0.5f; sim.Env.BalancoAgua[i] = 0.3f; }
            Creature Strong(int id) { var g = new Genome(); g.Set("comp.agressividade", 1f); g.Set("corpo.tamanho", 1f); g.Set("metabolismo", 0.3f); return new Creature(id, g, 10f + id * 0.4f, 10f); }
            Creature Weak(int id) { var g = new Genome(); g.Set("comp.agressividade", 0f); g.Set("corpo.tamanho", 0f); g.Set("metabolismo", 0.3f); return new Creature(id, g, 10.5f + id * 0.4f, 10.2f); }
            var s1 = Strong(0); var s2 = Strong(1); var w1 = Weak(2); var w2 = Weak(3);
            sim.Pop.Creatures.AddRange(new[] { s1, s2, w1, w2 });
            for (int i = 0; i < 400; i++) sim.Step();
            float strong = (s1.DominanceWins + s2.DominanceWins) * 0.5f;
            float weak = (w1.DominanceWins + w2.DominanceWins) * 0.5f;
            Assert.True(strong > weak, $"fortes deveriam vencer mais ({strong} vs {weak})");
        }

        // --- Figuras emergem por prestígio (não por nomeação aleatória) ---
        [Fact]
        public void Figures_EmergeFromPrestige()
        {
            var sim = Make(4, 30, g => { g.Set("comp.sociabilidade", 0.8f); g.Set("comp.lideranca", 0.9f); g.Set("fertilidade", 0.8f); g.Set("longevidade", 1f); g.Set("metabolismo", 0.4f); });
            for (int i = 0; i < 1500; i++) sim.Step();
            // toda Figura satisfaz o limiar de prestígio e liderança
            foreach (var c in sim.Pop.Creatures.Where(x => x.IsFigure))
            {
                Assert.True(c.Prestige >= Social.FigurePrestige);
                Assert.True(c.Trait("comp.lideranca") > 0.55f);
            }
        }

        // --- Determinismo: mesma semente ⇒ mesma estrutura social ---
        [Fact]
        public void Social_IsDeterministic()
        {
            var a = new Simulation(9, 36, 24, 30);
            var b = new Simulation(9, 36, 24, 30);
            for (int i = 0; i < 800; i++) { a.Step(); b.Step(); }
            Assert.Equal(a.Pop.Social.GroupCount, b.Pop.Social.GroupCount);
            Assert.Equal(a.Pop.Social.Rels.Count, b.Pop.Social.Rels.Count);
            Assert.Equal(a.Snapshot(), b.Snapshot());
        }
    }
}
