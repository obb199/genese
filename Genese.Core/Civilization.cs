using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Uma civilização = Population com identidade própria: linhagem, idioma, cultura,
    /// território e relações com outras civs (M11 §2). Roda as MESMAS regras M01-M10
    /// que a civ do jogador — sem IA roteirizada.
    /// </summary>
    public sealed class Civilization
    {
        public int Id;
        public Population Pop;
        /// <summary>Centro do território de origem (onde a civ foi semeada).</summary>
        public int SpawnX, SpawnY;
        /// <summary>Relações bilaterais com outras civs (chave = Id da outra civ).</summary>
        public readonly Dictionary<int, CivRelation> Relations = new();

        public Civilization(int id, Population pop, int spawnX, int spawnY)
        {
            Id = id; Pop = pop; SpawnX = spawnX; SpawnY = spawnY;
        }

        public CivRelation GetOrDefault(int otherId)
        {
            return Relations.TryGetValue(otherId, out var r) ? r
                 : new CivRelation { Stance = CivStance.Desconhecida };
        }

        // ---- Cálculos de afinidade (M11 §4.1) ---- sem tabela fixa, derivados do estado ----

        /// <summary>
        /// Afinidade de contato [0,1]: parentesco genético + inteligibilidade linguística
        /// + compatibilidade cultural. Determina a predisposição do encontro.
        /// </summary>
        public static float ContactAffinity(Civilization a, Civilization b)
        {
            float genKin    = GeneticKinship(a.Pop, b.Pop);
            float lingIntel = 1f - Language.Distance(a.Pop.Language, b.Pop.Language);
            float cultComp  = CulturalCompatibility(a.Pop.Culture, b.Pop.Culture);
            // Cultura tem maior peso — é o principal filtro de interpretação (M09/M10)
            return genKin * 0.25f + lingIntel * 0.30f + cultComp * 0.45f;
        }

        static float GeneticKinship(Population a, Population b)
        {
            if (a.Creatures.Count == 0 || b.Creatures.Count == 0) return 0.5f;
            float sum = 0f; int pairs = 0;
            for (int i = 0; i < Math.Min(5, a.Creatures.Count) && pairs < 10; i++)
                for (int j = 0; j < Math.Min(5, b.Creatures.Count) && pairs < 10; j++)
                    if (a.Creatures[i].Alive && b.Creatures[j].Alive)
                    { sum += Genome.Distance(a.Creatures[i].Genome, b.Creatures[j].Genome); pairs++; }
            return pairs == 0 ? 0.5f : 1f - sum / pairs;
        }

        static float CulturalCompatibility(Culture a, Culture b)
        {
            if (a.SymbolCount == 0 || b.SymbolCount == 0) return 0.5f;
            int shared = 0, total = 0;
            foreach (var kv in a.Pool)
            {
                total++;
                var matchB = b.Interpret(kv.Value.Type);
                if (matchB.HasValue && Math.Abs(matchB.Value.Force - kv.Value.Force) < 0.35f) shared++;
            }
            return total == 0 ? 0.5f : (float)shared / total;
        }

        // ---- snapshot ----
        public void Write(BinaryWriter w)
        {
            w.Write(Id); w.Write(SpawnX); w.Write(SpawnY);
            Pop.Write(w);
            var keys = new List<int>(Relations.Keys); keys.Sort();
            w.Write(keys.Count);
            foreach (int k in keys) { w.Write(k); Relations[k].Write(w); }
        }
        public static Civilization Read(BinaryReader r)
        {
            int id = r.ReadInt32(), sx = r.ReadInt32(), sy = r.ReadInt32();
            var pop = new Population(); pop.ReadInto(r);
            var civ = new Civilization(id, pop, sx, sy);
            int n = r.ReadInt32();
            for (int i = 0; i < n; i++) { int k = r.ReadInt32(); civ.Relations[k] = CivRelation.Read(r); }
            return civ;
        }
    }
}
