using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>
    /// Testes de E06/M03 — motor evolutivo: mutação causal, distância genômica,
    /// isolamento reprodutivo e especiação emergente. Nada é sorteado no vácuo:
    /// cada resultado tem causa rastreável no estado do mundo (GDD §1.5).
    /// </summary>
    public class SpeciationTests
    {
        // ----- helpers -----
        static Simulation MakeFlat(ulong seed, int n, bool volcanic)
        {
            var sim = new Simulation(seed, 24, 18, 0);
            for (int i = 0; i < sim.Env.Bioma.Length; i++)
            {
                var biome = volcanic ? Biome.Vulcanico : Biome.Pradaria;
                sim.Env.Bioma[i]      = (byte)biome;
                sim.Env.Comida[i]     = 0.85f;
                sim.Env.Agua[i]       = 0.8f;
                sim.Env.Altitude[i]   = 0.5f;
                sim.Env.BaseTemp[i]   = volcanic ? 0.88f : 0.5f;
                sim.Env.BaseUmid[i]   = 0.5f;
                sim.Env.BalancoAgua[i]= volcanic ? -0.6f : 0.3f; // vulcânico = seca severa → +pressão
                sim.Env.Temp[i]       = sim.Env.BaseTemp[i];
                sim.Env.Umidade[i]    = sim.Env.BaseUmid[i];
            }
            var rng = new Rng(seed).Fork(Streams.Spawn);
            for (int k = 0; k < n; k++)
            {
                var g = Genome.Founder(rng.Fork((ulong)(k + 1)));
                g.Set("reg.fatorMutacao", 1.0f); // taxa regulatória máxima → divergência acelerada
                g.Set("metabolismo", 0.25f);     // consome pouco energia → sobrevive no vulcânico
                g.Set("fertilidade", 0.8f);
                sim.Pop.Creatures.Add(new Creature(k, g, 6f + k * 2f, 9f));
            }
            return sim;
        }

        // --- 1. FatorTamanhoPop: pop pequena tem fator maior (M03 §4.1) ---
        [Fact]
        public void FatorTamanhoPop_SmallPopHasHigherFactor()
        {
            float tiny   = Speciation.FatorTamanhoPop(3);
            float small  = Speciation.FatorTamanhoPop(10);
            float medium = Speciation.FatorTamanhoPop(30);
            float large  = Speciation.FatorTamanhoPop(100);

            Assert.True(tiny   > small,  $"3 > 10 ({tiny} vs {small})");
            Assert.True(small  > medium, $"10 > 30 ({small} vs {medium})");
            Assert.True(medium > large,  $"30 > 100 ({medium} vs {large})");
            Assert.True(tiny   >= 2.0f,  $"população mínima deve ter fator alto ({tiny})");
        }

        // --- 2. PressaoMutagenica: vulcânico/seca elevam a pressão (M03 §4.1) ---
        [Fact]
        public void PressaoMutagenica_VulcanicAndDrought_HigherThanNormal()
        {
            var sim = new Simulation(1, 20, 16, 0);
            // célula vulcânica com seca
            sim.Env.Bioma[0]       = (byte)Biome.Vulcanico;
            sim.Env.BalancoAgua[0] = -0.7f;
            sim.Env.Temp[0]        = 0.5f;
            // célula normal pradaria
            sim.Env.Bioma[1]       = (byte)Biome.Pradaria;
            sim.Env.BalancoAgua[1] = 0.3f;
            sim.Env.Temp[1]        = 0.5f;

            float pVulc    = sim.Env.PressaoMutagenica(0, 0);
            float pNormal  = sim.Env.PressaoMutagenica(1, 0);

            Assert.True(pVulc > pNormal, $"vulcânico ({pVulc:0.00}) deve ser maior que normal ({pNormal:0.00})");
            Assert.True(pVulc >= 2.0f,   $"vulcânico + seca deve ser ≥ 2.0 (é {pVulc:0.00})");
            Assert.True(pNormal >= 1.0f, $"pressão mínima é 1.0 (é {pNormal:0.00})");
        }

        // --- 3. PodeReproduzir: genomas muito distantes = reprodução bloqueada ---
        [Fact]
        public void PodeReproduzir_FarGenomes_Blocked()
        {
            // Dois genomas artificialmente opostos (todos os genes em extremos opostos)
            var a = new Genome(); for (int i = 0; i < 24; i++) a.Values[i] = 0f;
            var b = new Genome(); for (int i = 0; i < 24; i++) b.Values[i] = 1f;
            var ca = new Creature(0, a, 0f, 0f);
            var cb = new Creature(1, b, 1f, 1f);

            float dist = Genome.Distance(a, b);
            Assert.True(dist >= Speciation.LimiarFertil, $"distância máxima ({dist:0.00}) deve ultrapassar o limiar");
            Assert.False(Speciation.PodeReproduzir(ca, cb), "genomas opostos não devem poder reproduzir");
            Assert.Equal(0f, Speciation.Compatibilidade(a, b));
        }

        // --- 4. Alta pressão mutagênica → descendência mais divergente ---
        [Fact]
        public void HighPressure_ProducesMoreDivergentOffspring()
        {
            // Rng com mesma semente → pressão é o único fator diferente
            var rng = new Rng(42).Fork(Streams.Mutation);

            var base_ = new Genome(); for (int i = 0; i < 24; i++) base_.Values[i] = 0.5f;
            base_.Set("reg.fatorMutacao", 1.0f);

            // 50 descendentes com pressão normal (envPressure = 0)
            double distNormal = 0;
            var rng1 = new Rng(42).Fork(Streams.Mutation);
            for (int k = 0; k < 50; k++)
            {
                var child = Reproduction.Reproduce(base_, base_, rng1, envPressure: 0f, mutationScale: 1.0f);
                distNormal += Genome.Distance(base_, child);
            }

            // 50 descendentes com pressão alta (envPressure ≈ vulcânico + seca)
            double distHigh = 0;
            var rng2 = new Rng(42).Fork(Streams.Mutation);
            for (int k = 0; k < 50; k++)
            {
                var child = Reproduction.Reproduce(base_, base_, rng2, envPressure: 0.55f, mutationScale: 2.2f);
                distHigh += Genome.Distance(base_, child);
            }

            Assert.True(distHigh > distNormal,
                $"pressão alta deve gerar filhos mais divergentes ({distHigh:0.000} vs {distNormal:0.000})");
        }

        // --- 5. Especiação é determinística: mesma semente → mesmos resultados ---
        [Fact]
        public void Speciation_IsDeterministic()
        {
            var a = MakeFlat(7, 8, false);
            var b = MakeFlat(7, 8, false);
            for (int i = 0; i < 500; i++) { a.Step(); b.Step(); }

            Assert.Equal(a.Pop.Spec.EspeciacaoCount, b.Pop.Spec.EspeciacaoCount);
            Assert.Equal(Speciation.ContarLinhagens(a.Pop.Creatures),
                         Speciation.ContarLinhagens(b.Pop.Creatures));
            Assert.Equal(a.Snapshot(), b.Snapshot());
        }
    }
}
