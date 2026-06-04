using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    public class CoreSim : MonoBehaviour
    {
        [Header("Mundo")]
        public int seed = 12345;
        public int width = 128, height = 128, initialCreatures = 8;

        [Header("Tempo")]
        public float stepsPerSecond = 10f;
        public bool playing = true;

        public static readonly (string name, float t, float u)[] Climates =
        {
            ("Temperado", 0f,    0f),
            ("Árido",     0.16f, -0.28f),
            ("Tropical",  0.18f,  0.22f),
            ("Frio",     -0.28f,  0f),
            ("Úmido",     0f,     0.26f)
        };

        public static readonly string[] CultureNames =
        {
            "Floresta", "Árido", "Medieval", "Arcana", "Imperial",
            "Tecnológica", "Aquática", "Nômade", "Subterrânea", "Ordem"
        };

        public static readonly string[] FantasyThemeNames =
        {
            "Clássico",
            "Cogumelos Gigantes", "Mar de Cristais",  "Floresta Mecânica",
            "Bioma das Nuvens",   "Oceano Vivo",       "Selva Bioluminescente",
            "Bioma Neural",       "Floresta de Vidro", "Bioma Fractal",
            "Ecossistema de Silício"
        };

        // Nomes descritivos dos níveis de diversidade de bioma
        public static readonly string[] BiomeVarietyNames =
        {
            "Mono (1 bioma)", "Baixa (2 biomas)", "Média (3 biomas)",
            "Alta (4 biomas)", "Máxima (5+ biomas)"
        };

        public CG.Simulation Sim { get; private set; }
        public int WorldVersion       { get; private set; }
        public int ClimateIndex       { get; private set; }
        public int ClimateIndex2      { get; private set; }
        public int CultureIndex       { get; private set; }
        public int CultureIndex2      { get; private set; }
        public int FantasyThemeIndex  { get; private set; }
        public int FantasyThemeIndex2 { get; private set; }

        /// <summary>Número de civilizações geradas (2-5).</summary>
        public int NumCivs       { get; private set; } = 2;
        /// <summary>Diversidade de biomas (1=mono, 5=máxima).</summary>
        public int BiomeVariety  { get; private set; } = 3;

        public string ClimateName        => Climates[ClimateIndex].name;
        public string ClimateName2       => Climates[ClimateIndex2].name;
        public string CultureName        => CultureNames[CultureIndex];
        public string CultureName2       => CultureNames[CultureIndex2];
        public string FantasyThemeName   => FantasyThemeNames[FantasyThemeIndex];
        public string FantasyThemeName2  => FantasyThemeNames[FantasyThemeIndex2];
        public string BiomeVarietyName   => BiomeVarietyNames[Mathf.Clamp(BiomeVariety - 1, 0, 4)];

        float _acc;

        void Awake() => NewWorld();

        void BuildWorld()
        {
            // Presets de biases (tempBias1, umidBias1, tempBias2, umidBias2) por variedade.
            // Os valores cruzam limiares do Derive() para garantir N biomas terrestres distintos:
            //   Tundra  : temp < 0.32
            //   Deserto : temp > 0.55 && umid < 0.35
            //   Floresta: umid > 0.55
            //   Pradaria: default (temp 0.32-0.55, umid 0.35-0.55)
            //   Pantano : umid > 0.62 && temp > 0.48 (região quente+úmida)
            // Oceano e Montanha dependem de altitude → sempre presentes.
            var rng2 = new System.Random(seed ^ 0xB10_0000);
            float t1, u1, t2, u2;
            switch (BiomeVariety)
            {
                case 1: // 1 bioma terrestre dominante
                    int p1 = rng2.Next(4);
                    (t1, u1) = p1 == 0 ? (-0.40f,  0.00f)  // Tundra
                             : p1 == 1 ? ( 0.30f, -0.40f)  // Deserto
                             : p1 == 2 ? ( 0.08f,  0.38f)  // Floresta
                                       : ( 0.02f, -0.05f); // Pradaria
                    (t2, u2) = (t1, u1); // mesma região nos dois lados
                    break;
                case 2: // 2 biomas terrestres
                    int p2 = rng2.Next(3);
                    (t1, u1, t2, u2) = p2 == 0
                        ? (-0.38f,  0.00f,  0.28f, -0.38f)  // Tundra | Deserto
                        : p2 == 1
                        ? ( 0.26f, -0.36f,  0.08f,  0.36f)  // Deserto | Floresta
                        : (-0.36f,  0.00f,  0.08f,  0.36f); // Tundra | Floresta
                    break;
                case 3: // 3 biomas terrestres
                    int p3 = rng2.Next(2);
                    (t1, u1, t2, u2) = p3 == 0
                        ? (-0.34f,  0.00f,  0.24f, -0.32f)  // Tundra | Pradaria | Deserto
                        : (-0.34f,  0.10f,  0.10f,  0.36f); // Tundra | Pradaria | Floresta
                    break;
                case 4: // 4 biomas terrestres
                    (t1, u1, t2, u2) = (-0.32f, 0.18f, 0.24f, 0.30f);
                    // Tundra | Pradaria | Floresta | Pantano
                    break;
                case 5: // 5 biomas terrestres
                    (t1, u1, t2, u2) = (-0.32f, 0.20f, 0.26f, -0.30f);
                    // Tundra | Pradaria | Floresta | Pantano | Deserto
                    break;
                default:
                    t1 = Climates[ClimateIndex].t;  u1 = Climates[ClimateIndex].u;
                    t2 = Climates[ClimateIndex2].t; u2 = Climates[ClimateIndex2].u;
                    break;
            }

            Sim = new CG.Simulation((ulong)seed, width, height, initialCreatures,
                                    t1, u1, t2, u2, numCivs: NumCivs);
            foreach (var civ in Sim.Civs) civ.Pop.Cap = int.MaxValue;
            WorldVersion++;
        }

        /// <summary>Gera novo mundo com civs (2-5) e bioma (1-5) aleatórios.</summary>
        public void NewWorld()
        {
            seed++;
            var rng = new System.Random(seed);
            NumCivs       = rng.Next(2, 6);   // 2-5 civilizações
            BiomeVariety  = rng.Next(1, 6);   // 1-5 diversidade de biomas
            ClimateIndex  = rng.Next(0, Climates.Length);
            ClimateIndex2 = rng.Next(0, Climates.Length);
            CultureIndex  = rng.Next(0, CultureNames.Length);
            CultureIndex2 = rng.Next(0, CultureNames.Length);
            FantasyThemeIndex  = rng.Next(0, FantasyThemeNames.Length);
            FantasyThemeIndex2 = rng.Next(0, FantasyThemeNames.Length);
            BuildWorld();
        }

        public void BumpVersion() => WorldVersion++;

        // ── Cicladores manuais ───────────────────────────────────────────────
        public void CycleClimate(int d)      { ClimateIndex  = Mod(ClimateIndex  + d, Climates.Length);     BuildWorld(); }
        public void CycleClimate2(int d)     { ClimateIndex2 = Mod(ClimateIndex2 + d, Climates.Length);     BuildWorld(); }
        public void CycleCulture(int d)      { CultureIndex  = Mod(CultureIndex  + d, CultureNames.Length); WorldVersion++; }
        public void CycleCulture2(int d)     { CultureIndex2 = Mod(CultureIndex2 + d, CultureNames.Length); WorldVersion++; }
        public void CycleFantasyTheme(int d) { FantasyThemeIndex  = Mod(FantasyThemeIndex  + d, FantasyThemeNames.Length); WorldVersion++; }
        public void CycleFantasyTheme2(int d){ FantasyThemeIndex2 = Mod(FantasyThemeIndex2 + d, FantasyThemeNames.Length); WorldVersion++; }
        public void CycleNumCivs(int d)      { NumCivs      = Mathf.Clamp(NumCivs      + d, 2, 5); BuildWorld(); }
        public void CycleBiomeVariety(int d) { BiomeVariety = Mathf.Clamp(BiomeVariety + d, 1, 5); BuildWorld(); }

        static int Mod(int v, int n) => ((v % n) + n) % n;

        void Update()
        {
            if (!playing || Sim == null) return;
            _acc += Time.deltaTime * stepsPerSecond;
            int guard = 0;
            while (_acc >= 1f && guard++ < 40) { Sim.Step(); _acc -= 1f; }
        }
    }
}
