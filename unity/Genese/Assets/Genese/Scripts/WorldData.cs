using System.Collections.Generic;
using UnityEngine;

namespace Genese
{
    /// <summary>Bioma: paletas de céu, chão, folhagem e neblina (espelha biomes.js).</summary>
    public struct Biome
    {
        public string Nome;
        public Color SkyTop, SkyHorizon, Ground, Foliage, Accent, Fog;
    }

    /// <summary>Cultura da aldeia: recolore cabanas/totem/fogueira (espelha buildings.js).</summary>
    public struct Culture
    {
        public string Nome;
        public Color Wall, Roof, Accent, Flame;
        public int RoofStyle;   // 0 pirâmide · 1 hip · 2 laje/parapeito · 3 duas-águas · 4 cúpula
        public string Monument; // tipo de monumento central da cultura
    }

    /// <summary>Perfil de geração de um bioma: que props nascem e em que densidade.</summary>
    public struct BiomeProfile
    {
        public string Tree;    // leafy | conifer | cactus | dead
        public string Rock;    // boulder | crystal | slab
        public int Trees, Rocks;
        public bool Grass;     // tufos no chão
        public Color Special;  // cor de cristal (gelo/lava) quando Rock=crystal
        public Color Trunk;    // cor de tronco
        public Color Water;    // cor da água (rios/lagos)
        public string Extras;  // props decorativos extra: "flower,bush,mushroom,..."
        public int ExtraCount;
    }

    public static class WorldData
    {
        static Color H(string h) => Palette.Hex(h);

        public static readonly Dictionary<string, Biome> Biomes = new()
        {
            ["pradaria"]  = new Biome { Nome = "Pradaria / Savana", SkyTop = H("#A9B36A"), SkyHorizon = H("#d9d6b0"), Ground = H("#6f7a3a"), Foliage = H("#56A06A"), Accent = H("#C9A86A"), Fog = H("#c8c6a6") },
            ["floresta"]  = new Biome { Nome = "Floresta",          SkyTop = H("#2C5247"), SkyHorizon = H("#88a890"), Ground = H("#274d40"), Foliage = H("#2F6F62"), Accent = H("#6E5638"), Fog = H("#7d9488") },
            ["deserto"]   = new Biome { Nome = "Deserto",           SkyTop = H("#D9B873"), SkyHorizon = H("#f0e2b8"), Ground = H("#C9A86A"), Foliage = H("#9DBB6A"), Accent = H("#B58A3E"), Fog = H("#e6d2a0") },
            ["tundra"]    = new Biome { Nome = "Tundra / Gelo",     SkyTop = H("#9FB4C4"), SkyHorizon = H("#e3edf2"), Ground = H("#C2D2DA"), Foliage = H("#9FB4C4"), Accent = H("#7FA0B6"), Fog = H("#d6e2e8") },
            ["montanha"]  = new Biome { Nome = "Montanha / Rocha",  SkyTop = H("#9A938A"), SkyHorizon = H("#cfc7ba"), Ground = H("#8A8377"), Foliage = H("#7E8A5E"), Accent = H("#5C564B"), Fog = H("#bdb4a6") },
            ["agua"]      = new Biome { Nome = "Água / Costa",      SkyTop = H("#5FA9B0"), SkyHorizon = H("#bfe3e0"), Ground = H("#5b7e6a"), Foliage = H("#3E8C7A"), Accent = H("#3E6B8C"), Fog = H("#a9d4d2") },
            ["pantano"]   = new Biome { Nome = "Pântano / Úmido",   SkyTop = H("#3C4A33"), SkyHorizon = H("#8a9a72"), Ground = H("#3a4a2e"), Foliage = H("#4a6a3a"), Accent = H("#6a6a3a"), Fog = H("#7e8c66") },
            ["vulcanico"] = new Biome { Nome = "Vulcânico",         SkyTop = H("#5A2E26"), SkyHorizon = H("#b06a4a"), Ground = H("#3A2A28"), Foliage = H("#7a4a3a"), Accent = H("#C0563A"), Fog = H("#8a5040") },
        };

        public static readonly Dictionary<string, Culture> Cultures = new()
        {
            ["floresta"]    = new Culture { Nome = "Floresta",    Wall = H("#C9A86A"), Roof = H("#B0644C"), Accent = H("#8C5BAA"), Flame = H("#E0A24A"), RoofStyle = 0, Monument = "totens" },
            ["arido"]       = new Culture { Nome = "Árido",       Wall = H("#D8C49A"), Roof = H("#B8862F"), Accent = H("#C0563A"), Flame = H("#E0C46A"), RoofStyle = 1, Monument = "ziggurat" },
            ["medieval"]    = new Culture { Nome = "Reino",       Wall = H("#C7BBA2"), Roof = H("#7E4A3E"), Accent = H("#B7402E"), Flame = H("#E08A3A"), RoofStyle = 3, Monument = "torre" },
            ["arcana"]      = new Culture { Nome = "Arcana",      Wall = H("#9579B6"), Roof = H("#5E2E9E"), Accent = H("#5FE0C2"), Flame = H("#A07CC4"), RoofStyle = 4, Monument = "cristal" },
            ["imperial"]    = new Culture { Nome = "Imperial",    Wall = H("#D2C0A0"), Roof = H("#8E2D17"), Accent = H("#E0C46A"), Flame = H("#E0A24A"), RoofStyle = 2, Monument = "obelisco" },
            ["tecnologica"] = new Culture { Nome = "Tecnológica", Wall = H("#9FB4C4"), Roof = H("#3E6B8C"), Accent = H("#5FE0C2"), Flame = H("#7FD0E0"), RoofStyle = 2, Monument = "antena" },
        };

        public static readonly Dictionary<string, BiomeProfile> Profiles = new()
        {
            ["pradaria"]  = new BiomeProfile { Tree = "leafy",   Rock = "boulder", Trees = 12, Rocks = 8,  Grass = true,  Trunk = H("#6E5638"), Water = H("#3E6B8C"), Extras = "flower,bush,flower", ExtraCount = 30 },
            ["floresta"]  = new BiomeProfile { Tree = "conifer", Rock = "boulder", Trees = 26, Rocks = 7,  Grass = true,  Trunk = H("#5a4630"), Water = H("#2E5A6E"), Extras = "mushroom,bush,log,fern", ExtraCount = 34 },
            ["deserto"]   = new BiomeProfile { Tree = "cactus",  Rock = "slab",    Trees = 9,  Rocks = 14, Grass = false, Trunk = H("#3E8C5A"), Water = H("#5C97A6"), Extras = "bone,shrub", ExtraCount = 16 },
            ["tundra"]    = new BiomeProfile { Tree = "conifer", Rock = "crystal", Trees = 11, Rocks = 12, Grass = false, Trunk = H("#6E6A60"), Special = H("#BFEAF2"), Water = H("#BFE0EA"), Extras = "iceChunk,shrub", ExtraCount = 20 },
            ["montanha"]  = new BiomeProfile { Tree = "dead",    Rock = "boulder", Trees = 7,  Rocks = 20, Grass = false, Trunk = H("#6E665A"), Water = H("#4A6B7A"), Extras = "bush,log", ExtraCount = 14 },
            ["agua"]      = new BiomeProfile { Tree = "leafy",   Rock = "boulder", Trees = 9,  Rocks = 9,  Grass = true,  Trunk = H("#6E5638"), Water = H("#2E6F8C"), Extras = "reed,lilypad,bush", ExtraCount = 34 },
            ["pantano"]   = new BiomeProfile { Tree = "dead",    Rock = "boulder", Trees = 14, Rocks = 6,  Grass = true,  Trunk = H("#4a4030"), Water = H("#3A4A38"), Extras = "reed,mushroom,log", ExtraCount = 34 },
            ["vulcanico"] = new BiomeProfile { Tree = "dead",    Rock = "crystal", Trees = 7,  Rocks = 16, Grass = false, Trunk = H("#3a302c"), Special = H("#E06A3A"), Water = H("#E0552A"), Extras = "lavaRock,bone", ExtraCount = 22 },
        };
    }
}
