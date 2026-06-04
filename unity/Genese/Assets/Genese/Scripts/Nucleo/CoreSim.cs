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
        public int WorldVersion    { get; private set; }
        public int ClimateIndex    { get; private set; }
        public int CultureIndex    { get; private set; }
        public int FantasyThemeIndex { get; private set; }

        float _acc;

        void Awake() => NewWorld(); // primeiro mundo já é aleatório

        void BuildWorld()
        {
            var cl = Climates[ClimateIndex];
            Sim = new CG.Simulation((ulong)seed, width, height, initialCreatures, cl.t, cl.u);
            Sim.Pop.Cap = popCap;
            WorldVersion++;
        }

        /// <summary>
        /// Gera um novo mundo completamente aleatório: nova semente, clima,
        /// cultura e bioma fantástico escolhidos ao acaso — tudo do zero.
        /// </summary>
        public void NewWorld()
        {
            seed++;
            var rng = new System.Random(seed);
            ClimateIndex      = rng.Next(0, Climates.Length);
            CultureIndex      = rng.Next(0, CultureNames.Length);
            FantasyThemeIndex = rng.Next(0, FantasyThemeNames.Length);
            BuildWorld();
        }

        public void CycleClimate(int d)      { ClimateIndex      = (ClimateIndex      + d + Climates.Length)          % Climates.Length;          BuildWorld(); }
        public void CycleCulture(int d)      { CultureIndex      = (CultureIndex      + d + CultureNames.Length)      % CultureNames.Length;      WorldVersion++; }
        public void CycleFantasyTheme(int d) { FantasyThemeIndex = (FantasyThemeIndex + d + FantasyThemeNames.Length) % FantasyThemeNames.Length; WorldVersion++; }

        public string ClimateName     => Climates[ClimateIndex].name;
        public string CultureName     => CultureNames[CultureIndex];
        public string FantasyThemeName=> FantasyThemeNames[FantasyThemeIndex];

        void Update()
        {
            if (!playing || Sim == null) return;
            _acc += Time.deltaTime * stepsPerSecond;
            int guard = 0;
            while (_acc >= 1f && guard++ < 40) { Sim.Step(); _acc -= 1f; }
        }
    }
}
