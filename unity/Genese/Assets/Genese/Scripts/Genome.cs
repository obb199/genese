using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Genoma visível da criatura — espelha os eixos de personalização do
    /// creature.js do Claude Design. Na Etapa 1 ganha herança/mutação reais.
    /// </summary>
    [System.Serializable]
    public class Genome
    {
        public string shape = "egg";    // egg|round|tall|squat|pear|bean|blob
        public Color color = Palette.Hex("#7FB29E");
        public Color color2 = Palette.Hex("#5FA9B0");
        public string blend = "none";   // none|gradient|twotone|belly2|dorsal
        public string pattern = "belly"; // none|belly|spots|stripes
        public float size = 1f;          // 0.82 | 1 | 1.18
        public string eyes = "two";      // two|big|small|three|one
        public string pupil = "round";   // round|vertical
        public string mouth = "simple";  // simple|beak|tusks|none
        public string ears = "none";     // none|pointy|round|tuft|fan
        public string tail = "none";     // none|short|long|curl|tuft
        public string legs = "stubby";   // stubby|tall|none
        public string arms = "none";     // none|stubby|claws|flippers
        public string antennae = "none"; // none|pair|horns|crest|mane
        public string finish = "matte";  // matte|satin|metallic|iridescent
        public string emit = "none";     // none|eyes|antenna
        public string ornament = "none"; // none|headband|facepaint|crest|religious
        public Color signal = Palette.Hex("#8C5BAA");
        public bool glow = false;

        static string P(params string[] a) => a[Random.Range(0, a.Length)];

        /// <summary>Genoma aleatório plausível (pesos espelham lib.js cRand).</summary>
        public static Genome RandomGenome()
        {
            return new Genome
            {
                shape = P("egg", "round", "tall", "squat", "pear", "bean", "blob"),
                color = Palette.PickBody(), color2 = Palette.PickBody(),
                blend = P("none", "none", "gradient", "twotone", "belly2", "dorsal"),
                pattern = P("none", "belly", "belly", "spots", "stripes"),
                size = new[] { 0.82f, 1f, 1f, 1.18f }[UnityEngine.Random.Range(0, 4)],
                eyes = P("two", "two", "big", "small", "three", "one"),
                pupil = P("round", "round", "vertical"),
                mouth = P("simple", "simple", "beak", "tusks", "none"),
                ears = P("none", "none", "pointy", "round", "tuft", "fan"),
                tail = P("none", "none", "short", "long", "curl", "tuft"),
                legs = P("stubby", "stubby", "tall", "none"),
                arms = P("none", "none", "stubby", "claws", "flippers"),
                antennae = P("none", "none", "pair", "horns", "crest", "mane"),
                finish = P("matte", "matte", "satin", "metallic", "iridescent"),
                emit = P("none", "none", "none", "eyes", "antenna"),
                ornament = P("none", "none", "headband", "facepaint", "crest", "religious"),
                signal = Palette.PickSignal(),
                glow = UnityEngine.Random.value < 0.25f,
            };
        }

        public Genome Clone() => (Genome)MemberwiseClone();

        // jitter em HSV: varia cor mantendo identidade
        static Color ShiftHSV(Color c, float dh, float ds, float dv)
        {
            Color.RGBToHSV(c, out var h, out var s, out var v);
            h = Mathf.Repeat(h + Random.Range(-dh, dh), 1f);
            s = Mathf.Clamp01(s + Random.Range(-ds, ds));
            v = Mathf.Clamp01(v + Random.Range(-dv, dv));
            return Color.HSVToRGB(h, s, v);
        }

        // cor de corpo "típica" do bioma (criaturas combinam com o ambiente)
        static Color BiomeBody(string biome) => Palette.Hex(biome switch
        {
            "deserto" => "#D8B36A", "tundra" => "#C9D6DE", "floresta" => "#7FB29E",
            "vulcanico" => "#B0644C", "agua" => "#5FA9B0", "montanha" => "#A89E90",
            "pantano" => "#7E8C4A", _ => "#8FB36A",
        });

        /// <summary>Genoma-base de uma ESPÉCIE, influenciado pelo bioma. Os traços
        /// estruturais (forma, orelhas, antenas, membros, acabamento) definem a
        /// identidade; os indivíduos depois variam por cima (ver Vary).</summary>
        public static Genome Species(string biome)
        {
            var baseC = BiomeBody(biome);
            return new Genome
            {
                shape = P("egg", "round", "tall", "squat", "pear", "bean"),
                color = baseC, color2 = ShiftHSV(baseC, 0.04f, 0.06f, 0.12f),
                blend = P("none", "belly", "dorsal"),
                pattern = P("none", "belly", "spots", "stripes"),
                eyes = P("two", "two", "big"), pupil = P("round", "round", "vertical"),
                mouth = P("simple", "simple", "beak", "tusks"),
                ears = P("none", "pointy", "round", "tuft", "fan"),
                tail = P("none", "short", "long", "curl", "tuft"),
                legs = P("stubby", "stubby", "tall"),
                arms = P("none", "none", "stubby", "claws", "flippers"),
                antennae = P("none", "pair", "horns", "crest", "mane"),
                finish = P("matte", "matte", "satin"),
                signal = Palette.PickSignal(),
                size = Random.Range(0.95f, 1.12f),
            };
        }

        /// <summary>Indivíduo: a mesma espécie, com pequenas variações (cor, tamanho,
        /// olhos, ornamento cultural). Mantém os traços estruturais do template.</summary>
        public static Genome Vary(Genome sp)
        {
            var g = sp.Clone();
            g.color = ShiftHSV(sp.color, 0.025f, 0.08f, 0.13f);
            g.color2 = ShiftHSV(sp.color2, 0.025f, 0.08f, 0.13f);
            g.size = sp.size * Random.Range(0.82f, 1.18f);
            if (Random.value < 0.3f) g.eyes = P("two", "big", "small", "three");
            if (Random.value < 0.25f) g.pupil = P("round", "vertical");
            if (Random.value < 0.2f) g.pattern = P("none", "belly", "spots", "stripes");
            g.ornament = P("none", "none", "none", "headband", "facepaint", "crest", "religious");
            if (Random.value < 0.15f) g.emit = "eyes";
            g.glow = Random.value < 0.12f;
            return g;
        }
    }
}
