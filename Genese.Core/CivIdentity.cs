using System;
using System.Collections.Generic;

namespace Genese.Core
{
    /// <summary>
    /// Identidade emergente de uma civilização: nome do léxico e traço cultural
    /// derivado das estatísticas coletivas da população. Tudo calculado do estado —
    /// nunca atribuído manualmente.
    /// </summary>
    public static class CivIdentity
    {
        // ── Nome ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Nome do povo a partir do léxico emergente. Usa palavras que a civilização
        /// já cunhou; fallback para um nome fonético gerado da semente.
        /// </summary>
        public static string NameOf(Civilization civ)
        {
            if (civ == null) return "?";
            var lex = civ.Pop.Language.Lexicon;
            if (lex.TryGetValue("grupo", out var g)) return $"«{Capitalize(g)}»";
            if (lex.TryGetValue("terra", out var t)) return $"«{Capitalize(t)}»";
            if (lex.TryGetValue("lider", out var l)) return $"Povo {Capitalize(l)}";
            // Fallback: gera nome fonético curto da semente + id
            return PhoneticName(civ);
        }

        static string PhoneticName(Civilization civ)
        {
            // Conjunto de sílabas simples — produz nomes memoráveis de 2-3 sílabas
            string[] syllables = {
                "ar","el","or","un","ka","ma","ra","ti","vo","na",
                "se","li","du","ko","pa","al","em","ru","fi","zo"
            };
            var rng = new Random((int)(civ.Id * 1337 + 42));
            int count = 2 + rng.Next(2); // 2 ou 3 sílabas
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append(syllables[rng.Next(syllables.Length)]);
            return Capitalize(sb.ToString());
        }

        // ── Traço cultural ────────────────────────────────────────────────────

        /// <summary>
        /// Traço cultural dominante derivado das médias do genoma + estado simbólico.
        /// Retorna uma palavra única que o jogador pode aprender a reconhecer.
        /// </summary>
        public static string TraitOf(Civilization civ)
        {
            if (civ == null || civ.Pop.Count == 0) return "extinta";

            PopStats.Compute(civ.Pop.Creatures, out float[] means, out _);

            float agg  = means[GeneRegistry.IndexOf("comp.agressividade")];
            float soc  = means[GeneRegistry.IndexOf("comp.sociabilidade")];
            float expl = means[GeneRegistry.IndexOf("comp.exploracao")];
            float nom  = means[GeneRegistry.IndexOf("comp.nomadismo")];
            float fervor      = civ.Pop.Belief.Fervor;
            float org         = civ.Pop.Belief.Organization;
            float cohesion    = civ.Pop.Culture.CulturalCohesion;
            float militarismo = PopStats.Militarismo(means);
            float inovacao    = PopStats.Inovacao(means);

            // Ordem de precedência: o traço mais alto "vence"
            if (fervor > 0.65f && org > 0.55f)   return "Religiosa";
            if (militarismo > 0.62f)              return "Belicosa";
            if (nom > 0.62f)                      return "Nômade";
            if (expl > 0.60f && inovacao > 0.55f) return "Exploradora";
            if (cohesion > 0.78f && soc > 0.55f)  return "Coesa";
            if (soc < 0.32f && nom < 0.35f)       return "Isolada";
            if (org > 0.60f)                       return "Organizada";
            if (inovacao > 0.58f)                  return "Inovadora";
            return "Estável";
        }

        /// <summary>Ícone (emoji) que representa o traço visualmente.</summary>
        public static string TraitIcon(string trait) => trait switch
        {
            "Religiosa"   => "✦",
            "Belicosa"    => "⚔",
            "Nômade"      => "→",
            "Exploradora" => "◆",
            "Coesa"       => "●",
            "Isolada"     => "○",
            "Organizada"  => "▲",
            "Inovadora"   => "★",
            _             => "◆"
        };

        static string Capitalize(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
    }
}
