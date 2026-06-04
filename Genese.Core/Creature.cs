namespace Genese.Core
{
    /// <summary>
    /// Indivíduo simulado (M04). Genótipo (Genome) + estado de vida (posição na grade,
    /// energia/fome, idade). O fenótipo (tamanho, velocidade, etc.) é derivado do genoma.
    /// </summary>
    public sealed class Creature
    {
        public int Id;
        public Genome Genome;
        public float X, Y;       // posição contínua na grade do ambiente
        public float Energy;     // 0..1 (fome = 1 - Energy)
        public int Age;
        public bool Alive;

        // --- social (E05/M05) — emergente, lido das interações ---
        public float Prestige;    // acumulado por feitos (dominar, reproduzir, liderar)
        public int GroupId = -1;  // grupo detectado por clustering (-1 = sem grupo)
        public int DominanceWins; // disputas vencidas (proxy de posição na hierarquia)
        public int ForageCount, ExploreCount, ReproCount; // base do papel
        public bool IsFigure;     // prestígio acima do limiar → Figura

        public Creature(int id, Genome genome, float x, float y)
        {
            Id = id; Genome = genome; X = x; Y = y; Energy = 0.7f; Age = 0; Alive = true;
        }

        // --- fenótipo derivado (GDD §8.3 / M02) ---
        public float Size => Genome.Get("corpo.tamanho");
        public float Speed => 0.16f + 0.34f * Genome.Get("metabolismo");
        public float Metabolism => Genome.Get("metabolismo");
        public float Trait(string id) => Genome.Get(id);

        /// <summary>Papel emergente (do que mais faz / liderança).</summary>
        public string Role()
        {
            if (IsFigure) return "Líder";
            if (ReproCount >= ForageCount && ReproCount >= ExploreCount && ReproCount > 0) return "Parental";
            if (ExploreCount > ForageCount) return "Explorador";
            return "Forrageiro";
        }
    }
}
