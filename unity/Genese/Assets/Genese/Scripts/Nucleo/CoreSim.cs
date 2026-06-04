using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Ponte: roda o NÚCLEO determinístico e o avança em ticks. Permite gerar um
    /// NOVO MUNDO (nova semente), escolher o CLIMA (mundos mais áridos/frios/úmidos →
    /// biomas dominantes) e a CULTURA da aldeia. WorldVersion sobe a cada novo mundo,
    /// e as views (mundo/criaturas) se reconstroem.
    /// </summary>
    public class CoreSim : MonoBehaviour
    {
        [Header("Mundo (ilha circular)")]
        public int seed = 12345;
        public int width = 128, height = 128, initialCreatures = 10, popCap = 100; // tamanho equilibrado

        [Header("Tempo")]
        public float stepsPerSecond = 10f;
        public bool playing = true;

        public static readonly (string name, float t, float u)[] Climates =
        {
            ("Temperado", 0f, 0f), ("Árido", 0.16f, -0.28f), ("Tropical", 0.18f, 0.22f),
            ("Frio", -0.28f, 0f), ("Úmido", 0f, 0.26f)
        };

        // 10 culturas do Claude Design (buildings.js + buildings-extra*.js)
        public static readonly string[] CultureNames =
        {
            "Floresta", "Árido", "Medieval", "Arcana", "Imperial",
            "Tecnológica", "Aquática", "Nômade", "Subterrânea", "Ordem"
        };

        // 10 biomas fantásticos de biomes-fantasy.js (0 = Clássico, 1-10 = fantasia)
        public static readonly string[] FantasyThemeNames =
        {
            "Clássico",
            "Cogumelos Gigantes", "Mar de Cristais",  "Floresta Mecânica",
            "Bioma das Nuvens",   "Oceano Vivo",       "Selva Bioluminescente",
            "Bioma Neural",       "Floresta de Vidro", "Bioma Fractal",
            "Ecossistema de Silício"
        };

        public CG.Simulation Sim { get; private set; }
        public int WorldVersion       { get; private set; }
        public int ClimateIndex       { get; private set; }
        public int ClimateIndex2      { get; private set; }  // polo direito (x >= W/2)
        public int CultureIndex       { get; private set; }  // polo esquerdo
        public int CultureIndex2      { get; private set; }  // polo direito
        public int FantasyThemeIndex  { get; private set; }  // polo esquerdo
        public int FantasyThemeIndex2 { get; private set; }  // polo direito

        float _acc;

        void Awake() => NewWorld();

        void BuildWorld()
        {
            var cl1 = Climates[ClimateIndex];
            var cl2 = Climates[ClimateIndex2];
            Sim = new CG.Simulation((ulong)seed, width, height, initialCreatures,
                                    cl1.t, cl1.u, cl2.t, cl2.u);
            Sim.Pop.Cap = popCap;
            WorldVersion++;
        }

        /// <summary>
        /// Gera um novo mundo aleatório: cada polo recebe clima, bioma fantástico e cultura próprios.
        /// </summary>
        public void NewWorld()
        {
            seed++;
            var rng = new System.Random(seed);
            ClimateIndex       = rng.Next(0, Climates.Length);
            ClimateIndex2      = rng.Next(0, Climates.Length);
            CultureIndex       = rng.Next(0, CultureNames.Length);
            CultureIndex2      = rng.Next(0, CultureNames.Length);
            FantasyThemeIndex  = rng.Next(0, FantasyThemeNames.Length);
            FantasyThemeIndex2 = rng.Next(0, FantasyThemeNames.Length);
            BuildWorld();
        }

        public void CycleClimate(int d)       { ClimateIndex       = (ClimateIndex       + d + Climates.Length)          % Climates.Length;          BuildWorld(); }
        public void CycleClimate2(int d)      { ClimateIndex2      = (ClimateIndex2      + d + Climates.Length)          % Climates.Length;          BuildWorld(); }
        public void CycleCulture(int d)       { CultureIndex       = (CultureIndex       + d + CultureNames.Length)      % CultureNames.Length;      WorldVersion++; }
        public void CycleCulture2(int d)      { CultureIndex2      = (CultureIndex2      + d + CultureNames.Length)      % CultureNames.Length;      WorldVersion++; }
        public void CycleFantasyTheme(int d)  { FantasyThemeIndex  = (FantasyThemeIndex  + d + FantasyThemeNames.Length) % FantasyThemeNames.Length; WorldVersion++; }
        public void CycleFantasyTheme2(int d) { FantasyThemeIndex2 = (FantasyThemeIndex2 + d + FantasyThemeNames.Length) % FantasyThemeNames.Length; WorldVersion++; }

        public string ClimateName       => Climates[ClimateIndex].name;
        public string ClimateName2      => Climates[ClimateIndex2].name;
        public string CultureName       => CultureNames[CultureIndex];
        public string CultureName2      => CultureNames[CultureIndex2];
        public string FantasyThemeName  => FantasyThemeNames[FantasyThemeIndex];
        public string FantasyThemeName2 => FantasyThemeNames[FantasyThemeIndex2];

        void Update()
        {
            if (!playing || Sim == null) return;
            _acc += Time.deltaTime * stepsPerSecond;
            int guard = 0;
            while (_acc >= 1f && guard++ < 40) { Sim.Step(); _acc -= 1f; }
        }
    }
}
