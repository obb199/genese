using System;
using System.Collections.Generic;

namespace Genese.Core
{
    /// <summary>
    /// Sistema de contato e interação entre civilizações (M11 §4.1-4.2).
    /// Todas as decisões derivam do estado — sem tabela de hostilidade nem IA roteirizada.
    /// Cada tipo de interação (comércio, conflito, troca cultural, fusão gradual) é ativado
    /// pelo argmax de scores calculados a partir de traços, recursos e histórico.
    /// </summary>
    public static class ContactSystem
    {
        public const float ContactRadius = 5f;

        /// <summary>
        /// Verifica pares de criaturas próximas entre duas civs e executa a interação causal.
        /// </summary>
        public static void CheckAndInteract(Civilization a, Civilization b,
                                            Environment env, ulong tick, Rng rng)
        {
            if (a.Pop.Count == 0 || b.Pop.Count == 0) return;

            var pairs = FindContactPairs(a.Pop.Creatures, b.Pop.Creatures);
            if (pairs.Count == 0) return;

            var rel = a.GetOrDefault(b.Id);
            // Primeiro contato registrado
            if (rel.Stance == CivStance.Desconhecida)  rel.Stance = CivStance.PrimeiroContato;
            else if (rel.Stance == CivStance.PrimeiroContato) rel.Stance = CivStance.Cautelosa;
            rel.LastContactTick = (int)(tick & 0x7FFFFFFF);

            float affinity = Civilization.ContactAffinity(a, b);
            ExecuteInteraction(a, b, ref rel, pairs, affinity, env, rng);

            a.Relations[b.Id] = rel;
            // Relação é simétrica (mesmos dados em ambos os lados)
            var relB = b.GetOrDefault(a.Id);
            relB.Trust           = rel.Trust;
            relB.Resentment      = rel.Resentment;
            relB.Stance          = rel.Stance;
            relB.TradeCount      = rel.TradeCount;
            relB.WarCount        = rel.WarCount;
            relB.LastContactTick = rel.LastContactTick;
            b.Relations[a.Id] = relB;
        }

        static List<(Creature ca, Creature cb)> FindContactPairs(
            List<Creature> listA, List<Creature> listB)
        {
            var result = new List<(Creature, Creature)>();
            float r2 = ContactRadius * ContactRadius;
            foreach (var ca in listA)
            {
                if (!ca.Alive) continue;
                foreach (var cb in listB)
                {
                    if (!cb.Alive) continue;
                    float dx = ca.X - cb.X, dy = ca.Y - cb.Y;
                    if (dx*dx + dy*dy < r2) { result.Add((ca, cb)); if (result.Count >= 4) return result; }
                }
            }
            return result;
        }

        static void ExecuteInteraction(Civilization a, Civilization b,
                                       ref CivRelation rel,
                                       List<(Creature ca, Creature cb)> pairs,
                                       float affinity, Environment env, Rng rng)
        {
            // Scores das três ações principais (M11 §4.2) — derivados do estado, sem sorteio
            float warScore = rel.Resentment * 0.55f
                           + AvgTrait(a.Pop, "comp.agressividade") * 0.25f
                           + ResourceScarcity(a.Pop, env) * 0.20f;

            float tradeScore = rel.Trust * 0.45f
                             + affinity * 0.30f
                             + ComplementaryNeeds(a.Pop, b.Pop, env) * 0.25f;

            float cultureScore = affinity * 0.50f
                               + AvgTrait(a.Pop, "comp.aprendizagem") * 0.30f
                               + AvgTrait(a.Pop, "comp.sociabilidade") * 0.20f;

            bool atWar = rel.Stance == CivStance.Guerra;

            if (atWar || warScore > tradeScore * 1.35f && warScore > cultureScore * 1.35f)
                DoConflict(ref rel, pairs, rng);
            else if (tradeScore >= cultureScore)
                DoTrade(a, b, ref rel, pairs);
            else
                DoCultureExchange(a, b, ref rel, rng);

            // Fusão gradual: confiança muito alta + alta compatibilidade (M11 §4.2)
            if (!atWar && rel.Trust >= 0.78f && affinity >= 0.62f)
                DoFusionStep(a, b, ref rel, rng);

            UpdateStance(ref rel);
        }

        // ---- Ações de interação ----

        static void DoTrade(Civilization a, Civilization b, ref CivRelation rel,
                            List<(Creature ca, Creature cb)> pairs)
        {
            foreach (var (ca, cb) in pairs)
            {
                float give = 0.04f;
                if (ca.Energy > cb.Energy + 0.20f) { ca.Energy -= give; cb.Energy = Math.Min(1f, cb.Energy + give); }
                else if (cb.Energy > ca.Energy + 0.20f) { cb.Energy -= give; ca.Energy = Math.Min(1f, ca.Energy + give); }
            }
            rel.Trust      = Math.Min(1f, rel.Trust + 0.014f);
            rel.Resentment = Math.Max(0f, rel.Resentment - 0.004f);
            rel.TradeCount++;
        }

        static void DoConflict(ref CivRelation rel,
                               List<(Creature ca, Creature cb)> pairs, Rng rng)
        {
            foreach (var (ca, cb) in pairs)
            {
                float strA = 0.4f + 0.4f * ca.Trait("comp.agressividade") + 0.2f * ca.Trait("comp.medoCoragem");
                float strB = 0.4f + 0.4f * cb.Trait("comp.agressividade") + 0.2f * cb.Trait("comp.medoCoragem");
                // Pequena variação residual do RNG — justificada (condições de terreno/acaso de batalha)
                float noise = (float)rng.NextDouble() * 0.14f;
                float drain = 0.07f;
                if (strA + noise > strB)
                    { cb.Energy = Math.Max(0f, cb.Energy - drain); ca.Energy = Math.Min(1f, ca.Energy + drain * 0.25f); }
                else
                    { ca.Energy = Math.Max(0f, ca.Energy - drain); cb.Energy = Math.Min(1f, cb.Energy + drain * 0.25f); }
            }
            rel.Resentment = Math.Min(1f, rel.Resentment + 0.022f);
            rel.Trust      = Math.Max(0f, rel.Trust      - 0.018f);
            rel.WarCount++;
        }

        static void DoCultureExchange(Civilization a, Civilization b, ref CivRelation rel, Rng rng)
        {
            // A civ com Figura ativa dissemina um meme para a outra (M09 §4.1)
            if (a.Pop.Social.FigureCount > 0)
            {
                var dom = a.Pop.Culture.Dominant();
                if (dom.HasValue)
                    b.Pop.Culture.SpawnMeme(dom.Value.Type, dom.Value.Force * 0.65f, dom.Value.OriginId, rng);
            }
            rel.Trust = Math.Min(1f, rel.Trust + 0.007f);
        }

        static void DoFusionStep(Civilization a, Civilization b, ref CivRelation rel, Rng rng)
        {
            // Fusão gradual: 1 criatura migra da civ menor para a maior (adoção, M11 §4.2)
            var smaller = a.Pop.Count <= b.Pop.Count ? a : b;
            var larger  = a.Pop.Count <= b.Pop.Count ? b : a;
            for (int i = 0; i < smaller.Pop.Creatures.Count; i++)
            {
                var c = smaller.Pop.Creatures[i];
                if (!c.Alive || rng.NextDouble() > 0.04) continue;
                smaller.Pop.Creatures.RemoveAt(i);
                larger.Pop.Creatures.Add(c);
                break;
            }
            rel.Stance = CivStance.Aliada;
        }

        // ---- Stance derivada do histórico (nunca sorteada) ----
        static void UpdateStance(ref CivRelation rel)
        {
            if      (rel.Resentment >= 0.60f)    rel.Stance = CivStance.Guerra;
            else if (rel.Trust      >= 0.68f)    rel.Stance = CivStance.Aliada;
            else if (rel.TradeCount >= 3)         rel.Stance = CivStance.Comercial;
            else if (rel.Stance == CivStance.Guerra && rel.Resentment < 0.28f)
                                                  rel.Stance = CivStance.Cautelosa;
        }

        // ---- helpers ----
        internal static float AvgTrait(Population pop, string trait)
        {
            float sum = 0f; int n = 0;
            foreach (var c in pop.Creatures) if (c.Alive) { sum += c.Trait(trait); n++; }
            return n == 0 ? 0.5f : sum / n;
        }

        static float ResourceScarcity(Population pop, Environment env)
        {
            float sum = 0f; int n = 0;
            foreach (var c in pop.Creatures)
            {
                if (!c.Alive) continue;
                int x = Math.Clamp((int)c.X, 0, env.W-1), y = Math.Clamp((int)c.Y, 0, env.H-1);
                sum += env.Comida[env.Idx(x, y)]; n++;
            }
            return n == 0 ? 0f : 1f - sum / n;
        }

        static float ComplementaryNeeds(Population a, Population b, Environment env)
            => Math.Abs(ResourceScarcity(a, env) - ResourceScarcity(b, env));
    }
}
