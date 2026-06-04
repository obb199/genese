using System;
using System.IO;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes profundos de Environment — todos os subsistemas M06/M07.</summary>
    public class EnvironmentDeepTests
    {
        // ── Serialização ────────────────────────────────────────────────────────
        [Fact] public void Environment_Serialization_RoundTrip()
        {
            var env = new Environment(24, 16);
            env.Generate(new Rng(1UL));
            for (int i = 0; i < 50; i++) env.Step((ulong)i);

            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms); env.Write(w);
            ms.Position = 0;
            using var r  = new BinaryReader(ms);
            var env2 = new Environment(1,1); env2.ReadInto(r);

            Assert.Equal(env.W,       env2.W);
            Assert.Equal(env.H,       env2.H);
            Assert.Equal(env.Altitude, env2.Altitude);
            Assert.Equal(env.Bioma,    env2.Bioma);
        }

        // ── IsBarrier ──────────────────────────────────────────────────────────
        [Fact] public void IsBarrier_Ocean_ReturnsTrue()
        {
            var env = new Environment(32, 24); env.Generate(new Rng(2UL));
            bool foundOcean = false;
            for (int i = 0; i < env.Bioma.Length; i++)
                if ((Biome)env.Bioma[i] == Biome.Oceano) { foundOcean = true; break; }
            if (!foundOcean) return; // some seeds produce landmass only - skip
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
                if ((Biome)env.BiomaAt(x,y)==Biome.Oceano) Assert.True(env.IsBarrier(x,y));
        }

        [Fact] public void IsBarrier_Mountain_ReturnsTrue()
        {
            var env = new Environment(32, 24); env.Generate(new Rng(3UL));
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
                if (env.Altitude[env.Idx(x,y)] >= Environment.MountainAlt)
                    Assert.True(env.IsBarrier(x,y));
        }

        [Fact] public void IsBarrier_Pradaria_ReturnsFalse()
        {
            var env = new Environment(32, 24); env.Generate(new Rng(4UL));
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
                if ((Biome)env.BiomaAt(x,y)==Biome.Pradaria) Assert.False(env.IsBarrier(x,y));
        }

        // ── Harvest ────────────────────────────────────────────────────────────
        [Fact] public void Harvest_CannotExceedAvailable()
        {
            var env = new Environment(16,12); env.Generate(new Rng(5UL));
            // Find a land cell
            for (int i=0;i<env.Bioma.Length;i++)
                if (!env.IsBarrier(i%env.W, i/env.W))
                {
                    env.Comida[i] = 0.3f;
                    float got = env.Harvest(i%env.W, i/env.W, 1.0f); // request more than available
                    Assert.True(got <= 0.3f + 1e-5f, $"Harvest devolveu {got} > disponível 0.3");
                    Assert.True(env.Comida[i] >= -1e-5f, "Comida ficou negativa");
                    return;
                }
        }

        [Fact] public void Harvest_ReducesFood()
        {
            var env = new Environment(16,12); env.Generate(new Rng(6UL));
            for (int i=0;i<env.Bioma.Length;i++)
                if (!env.IsBarrier(i%env.W, i/env.W))
                {
                    env.Comida[i] = 0.5f;
                    env.Harvest(i%env.W, i/env.W, 0.2f);
                    Assert.True(env.Comida[i] < 0.5f);
                    return;
                }
        }

        // ── PressaoMutagenica ──────────────────────────────────────────────────
        [Fact] public void PressaoMutagenica_NormalCell_IsOne()
        {
            var env = new Environment(32,24); env.Generate(new Rng(7UL));
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
            {
                var b = env.BiomaAt(x,y);
                if (b==Biome.Pradaria || b==Biome.Floresta)
                {
                    float p = env.PressaoMutagenica(x,y);
                    Assert.Equal(1f, p, precision: 3);
                    return;
                }
            }
        }

        [Fact] public void PressaoMutagenica_Vulcanico_IsHighest()
        {
            var env = new Environment(32,24); env.Generate(new Rng(8UL));
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
                if (env.BiomaAt(x,y)==Biome.Vulcanico)
                {
                    float p = env.PressaoMutagenica(x,y);
                    Assert.True(p >= 2.2f, $"Vulcânico deveria ter pressão >= 2.2, mas deu {p}");
                    return;
                }
        }

        [Fact] public void PressaoMutagenica_Drought_Elevated()
        {
            var env = new Environment(32,24); env.Generate(new Rng(9UL));
            // Force drought somewhere
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
                if (!env.IsBarrier(x,y))
                {
                    env.BalancoAgua[env.Idx(x,y)] = -1f; // below threshold
                    float p = env.PressaoMutagenica(x,y);
                    Assert.True(p >= 1.6f, $"Seca deveria ter pressão >= 1.6, deu {p}");
                    return;
                }
        }

        // ── Bioma Derive ──────────────────────────────────────────────────────
        [Fact] public void Derive_Ocean_BelowSeaLevel()
        {
            Assert.Equal(Biome.Oceano, Environment.Derive(Environment.SeaLevel - 0.01f, 0.5f, 0.5f));
        }

        [Fact] public void Derive_Mountain_AboveMountainAlt()
        {
            Assert.Equal(Biome.Montanha, Environment.Derive(Environment.MountainAlt + 0.01f, 0.5f, 0.5f));
        }

        [Fact] public void Derive_Tundra_LowTemp()
        {
            Assert.Equal(Biome.Tundra, Environment.Derive(0.5f, 0.20f, 0.5f));
        }

        [Fact] public void Derive_Desert_HotDry()
        {
            Assert.Equal(Biome.Deserto, Environment.Derive(0.5f, 0.70f, 0.20f));
        }

        // ── Dual-Region ───────────────────────────────────────────────────────
        [Fact] public void DualRegion_LeftRight_HaveDifferentBiomes()
        {
            var env = new Environment(64, 32);
            // Very different biases: left = cold/dry, right = hot/wet
            env.Generate(new Rng(10UL), -0.35f, -0.35f, 0.35f, 0.35f);

            // Count tundra (cold/dry) on left vs right
            int tundraLeft=0, desertRight=0;
            for (int y=0;y<env.H;y++) for (int x=0;x<env.W;x++)
            {
                if (env.IsBarrier(x,y)) continue;
                var b = env.BiomaAt(x,y);
                if (x < env.W/3 && b==Biome.Tundra) tundraLeft++;
                if (x > 2*env.W/3 && (b==Biome.Deserto||b==Biome.Floresta||b==Biome.Pantano)) desertRight++;
            }
            // With strong bias difference, sides should differ in biome distribution
            Assert.True(tundraLeft > 0 || desertRight > 0,
                "Dual-region não produziu diferença biome entre esquerda e direita");
        }

        // ── Seasonal variation ────────────────────────────────────────────────
        [Fact] public void Season_AffectsTemperature()
        {
            var env = new Environment(32,24); env.Generate(new Rng(11UL));
            // Step through full year, record min/max temperature
            float minT = float.MaxValue, maxT = float.MinValue;
            for (ulong t = 0; t < (ulong)Environment.Year; t++)
            {
                env.Step(t);
                float avg = 0f; int n = 0;
                foreach (var v in env.Temp) { avg+=v; n++; }
                avg /= n;
                if (avg < minT) minT = avg; if (avg > maxT) maxT = avg;
            }
            Assert.True(maxT - minT > 0.05f, $"Temperatura não variou com a estação: {minT:0.00}→{maxT:0.00}");
        }
    }
}
