using UnityEngine;
using CG = Genese.Core;

namespace Genese.Nucleo
{
    /// <summary>
    /// Traduz o GENÓTIPO do núcleo (Genese.Core.Genome) no FENÓTIPO visual que o
    /// CreatureBuilder (arte do protótipo) sabe desenhar. Determinístico: o mesmo
    /// genoma sempre vira a mesma criatura, então linhagens parecidas parecem parecidas.
    /// </summary>
    public static class GenomeMap
    {
        static readonly string[] Shapes = { "egg", "round", "tall", "squat", "pear", "bean" };
        static string Pick(string[] a, float t) => a[Mathf.Clamp((int)(t * a.Length), 0, a.Length - 1)];

        public static Genome ToVisual(CG.Genome g)
        {
            float Get(string id) => g.Get(id);
            float hue = Get("corpo.cor");

            var v = new Genome
            {
                shape = Pick(Shapes, Get("corpo.textura")),
                color = Color.HSVToRGB(hue, 0.5f, 0.82f),
                color2 = Color.HSVToRGB(Mathf.Repeat(hue + 0.08f, 1f), 0.5f, 0.7f),
                blend = Get("corpo.cor") > 0.5f ? "belly" : "none",
                pattern = Pick(new[] { "none", "belly", "spots", "stripes" }, Get("resistencia.doenca")),
                size = 0.82f + Get("corpo.tamanho") * 0.55f,
                eyes = Pick(new[] { "two", "two", "big", "small" }, Get("sentidos.percepcao")),
                pupil = Get("comp.agressividade") > 0.6f ? "vertical" : "round",
                mouth = Get("comp.agressividade") > 0.72f ? "tusks" : "simple",
                ears = Pick(new[] { "none", "pointy", "round", "tuft", "fan" }, Get("comp.vigilancia")),
                tail = Pick(new[] { "none", "short", "long", "curl", "tuft" }, Get("comp.nomadismo")),
                legs = Get("metabolismo") > 0.5f ? "tall" : "stubby",
                arms = Pick(new[] { "none", "none", "stubby", "claws" }, Get("comp.agressividade")),
                antennae = Pick(new[] { "none", "pair", "horns", "crest", "mane" }, Get("comp.lideranca")),
                finish = Get("corpo.textura") > 0.7f ? "metallic" : "matte",
                emit = Get("reg.fatorMutacao") > 0.75f ? "eyes" : "none",
                ornament = Get("comp.altruismo") > 0.72f ? "headband" : "none",
                signal = Color.HSVToRGB(Get("comp.medoCoragem"), 0.6f, 0.9f),
                glow = Get("reg.fatorMutacao") > 0.85f,
            };
            return v;
        }
    }
}
