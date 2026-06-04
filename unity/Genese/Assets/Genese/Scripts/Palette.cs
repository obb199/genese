using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Paleta-base e rampas semânticas (DV §3 — tokens.json do Claude Design).
    /// Cor é dado: as mesmas cores valem para criaturas, mundo e overlays.
    /// </summary>
    public static class Palette
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // Base
        public static readonly Color Fundo      = Hex("#1F2933");
        public static readonly Color Vida        = Hex("#2F6F62");
        public static readonly Color Influencia  = Hex("#8C5BAA");
        public static readonly Color Tensao      = Hex("#C0563A");
        public static readonly Color UI          = Hex("#EAF2EF");
        public static readonly Color Fogo        = Hex("#E0A24A");
        public static readonly Color PedraCoracao = Hex("#5FE0C2");

        // Cores de corpo de criatura (espelha lib.js BODY_COLORS)
        public static readonly string[] Body = {
            "#7FB29E", "#9FB4C4", "#C9A86A", "#9579B6", "#5FA9B0",
            "#B0644C", "#8FB36A", "#D292B6", "#7C4DBE", "#E0C46A"
        };

        // Cores de sinal/emoção (espelha lib.js SIGNAL_COLORS)
        public static readonly string[] Signal = {
            "#8C5BAA", "#C0563A", "#2F6F62", "#B8862F", "#3E6B8C"
        };

        public static Color PickBody()   => Hex(Body[Random.Range(0, Body.Length)]);
        public static Color PickSignal() => Hex(Signal[Random.Range(0, Signal.Length)]);
    }
}
