using System.Linq;
using Genese.Core;
using Xunit;

namespace Genese.Core.Tests
{
    /// <summary>Testes de E03/M06. Determinismo e "sem acaso plano" são bloqueadores (GDD §1.5).</summary>
    public class EnvironmentTests
    {
        // --- Determinismo: mesma semente ⇒ mundo idêntico após N ticks ---
        [Fact]
        public void Environment_IsDeterministic_OverTicks()
        {
            var a = new Simulation(42, 48, 32);
            var b = new Simulation(42, 48, 32);
            for (int i = 0; i < 3000; i++) { a.Step(); b.Step(); }
            Assert.Equal(a.Snapshot(), b.Snapshot());
            Assert.Equal(a.Env.Altitude, b.Env.Altitude);
            Assert.Equal(a.Env.Comida, b.Env.Comida);
        }

        // --- Bioma é DERIVADO de altitude+temp+umidade (não pintado) ---
        [Fact]
        public void Biome_IsDerived_FromClimate()
        {
            Assert.Equal(Biome.Oceano, Environment.Derive(0.1f, 0.6f, 0.5f));       // abaixo do mar
            Assert.Equal(Biome.Montanha, Environment.Derive(0.9f, 0.6f, 0.5f));     // alto
            Assert.Equal(Biome.Tundra, Environment.Derive(0.5f, 0.1f, 0.5f));       // frio
            Assert.Equal(Biome.Deserto, Environment.Derive(0.5f, 0.7f, 0.2f));      // quente e seco
            Assert.Equal(Biome.Floresta, Environment.Derive(0.5f, 0.5f, 0.7f));     // úmido
        }

        // --- Geração produz biomas variados (mundo plausível) ---
        [Fact]
        public void Generation_ProducesVariedBiomes()
        {
            var sim = new Simulation(2026, 64, 48);
            var distintos = sim.Env.Bioma.Distinct().Count();
            Assert.True(distintos >= 4, $"esperava variedade de biomas, veio {distintos}");
        }

        // --- Seca emerge do balanço hídrico (déficit acumulado), reproduzível ---
        [Fact]
        public void Drought_EmergesFromWaterBalance_AndIsReproducible()
        {
            // força uma célula desértica (quente e seca) em ambos os mundos
            static void MakeDesert(Simulation s)
            {
                int i = s.Env.Idx(5, 5);
                s.Env.Bioma[i] = (byte)Biome.Deserto; s.Env.BaseTemp[i] = 0.85f; s.Env.BaseUmid[i] = 0.08f;
                s.Env.Altitude[i] = 0.5f; s.Env.BalancoAgua[i] = 0f;
            }
            var a = new Simulation(7, 48, 32); var b = new Simulation(7, 48, 32);
            MakeDesert(a); MakeDesert(b);
            for (int i = 0; i < 1500; i++) { a.Step(); b.Step(); }

            Assert.Equal(a.Env.BalancoAgua, b.Env.BalancoAgua);  // reproduzível
            // a célula desértica entra em seca por déficit hídrico acumulado (não por sorteio)
            Assert.True(a.Env.BalancoAgua[a.Env.Idx(5, 5)] < Environment.DroughtThreshold);
        }

        // --- Recurso colhido se regenera com o tempo ---
        [Fact]
        public void Resource_Regenerates_AfterHarvest()
        {
            var sim = new Simulation(123, 48, 32, 0); // sem criaturas: testa o ambiente isolado
            for (int i = 0; i < 500; i++) sim.Step(); // estabiliza

            // acha uma célula fértil (floresta/pradaria)
            int cell = -1;
            for (int i = 0; i < sim.Env.Bioma.Length; i++)
                if ((Biome)sim.Env.Bioma[i] == Biome.Floresta || (Biome)sim.Env.Bioma[i] == Biome.Pradaria) { cell = i; break; }
            Assert.True(cell >= 0);

            int x = cell % sim.Env.W, y = cell / sim.Env.W;
            sim.Env.Harvest(x, y, sim.Env.Comida[cell]); // esgota
            float depois = sim.Env.Comida[cell];
            Assert.True(depois < 0.05f);
            for (int i = 0; i < 400; i++) sim.Step();
            Assert.True(sim.Env.Comida[cell] > depois + 0.1f, "comida deveria regenerar"); // recupera
        }

        // --- Barreira ⇒ isolamento: erguer uma cordilheira separa regiões (handoff M03) ---
        [Fact]
        public void Mountain_Ridge_Isolates_Regions()
        {
            var sim = new Simulation(555, 40, 24);
            var env = sim.Env;
            int midx = env.W / 2;

            // rebaixa tudo p/ garantir conectividade inicial, depois ergue uma coluna de montanha
            for (int i = 0; i < env.Altitude.Length; i++) { env.Altitude[i] = 0.5f; env.Bioma[i] = (byte)Biome.Pradaria; }
            Assert.True(env.Connected(2, 12, env.W - 3, 12)); // conectado antes

            for (int y = 0; y < env.H; y++) { int i = env.Idx(midx, y); env.Altitude[i] = 0.95f; env.Bioma[i] = (byte)Biome.Montanha; }
            Assert.False(env.Connected(2, 12, env.W - 3, 12)); // cordilheira isolou os dois lados
        }
    }
}
