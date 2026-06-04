using System.Collections.Generic;

namespace Genese.Core
{
    /// <summary>Bloco do genoma (espelha a ficha do indivíduo, GDD §4.1 / M01 §2).</summary>
    public enum Bloco { Genetico, Comportamental, Regulatorio }

    /// <summary>Modo de herança de um gene (M01 §4.1).</summary>
    public enum Heranca { Media, Dominante, Recessivo, Poligenico }

    /// <summary>
    /// Metadados invariantes de um gene (M01 §3.1). Ficam UMA vez aqui no registro;
    /// cada criatura guarda só o vetor de valores (struct-of-arrays) — compacto p/ E06.
    /// </summary>
    public readonly struct GeneDef
    {
        public readonly string Id;
        public readonly Bloco Bloco;
        public readonly Heranca Modo;
        public readonly float TaxaMutBase;   // propensão intrínseca a mutar (M03 modula)
        public readonly float Plasticidade;  // quanto ambiente/experiência afastam fenótipo do genótipo
        public readonly float Min, Max;      // faixa válida (normalizada salvo exceção)

        public GeneDef(string id, Bloco bloco, Heranca modo, float taxaMut, float plast, float min = 0f, float max = 1f)
        {
            Id = id; Bloco = bloco; Modo = modo; TaxaMutBase = taxaMut; Plasticidade = plast; Min = min; Max = max;
        }
    }

    /// <summary>
    /// Registro mestre dos genes do protótipo (M01 §3.3 + bloco comportamental de M02).
    /// Expansível. A ordem aqui é estável e define o índice de cada gene (determinismo).
    /// </summary>
    public static class GeneRegistry
    {
        public static readonly GeneDef[] Defs;
        private static readonly Dictionary<string, int> Index;

        public static int Count => Defs.Length;
        public static int IndexOf(string id) => Index[id];
        public static GeneDef Def(int i) => Defs[i];

        static GeneRegistry()
        {
            Defs = new[]
            {
                // --- Genético (físico) — M01 §3.3 ---
                new GeneDef("corpo.tamanho",       Bloco.Genetico,    Heranca.Media,      0.010f, 0.3f),
                new GeneDef("corpo.cor",           Bloco.Genetico,    Heranca.Media,      0.022f, 0.1f), // deriva visível
                new GeneDef("corpo.textura",       Bloco.Genetico,    Heranca.Dominante,  0.012f, 0.1f),
                new GeneDef("metabolismo",         Bloco.Genetico,    Heranca.Media,      0.010f, 0.4f),
                new GeneDef("sentidos.percepcao",  Bloco.Genetico,    Heranca.Media,      0.010f, 0.3f),
                new GeneDef("fertilidade",         Bloco.Genetico,    Heranca.Media,      0.012f, 0.2f),
                new GeneDef("longevidade",         Bloco.Genetico,    Heranca.Media,      0.008f, 0.2f),
                new GeneDef("resistencia.doenca",  Bloco.Genetico,    Heranca.Recessivo,  0.012f, 0.2f),

                // --- Regulatório (controla expressão/mutação) — M01 §3.3 ---
                new GeneDef("reg.fatorMutacao",    Bloco.Regulatorio, Heranca.Media,      0.005f, 0.0f),
                new GeneDef("reg.dominanciaSexual",Bloco.Regulatorio, Heranca.Dominante,  0.006f, 0.0f),

                // --- Comportamental (predisposições) — M02 / GDD §3.3 (poligênico = suave) ---
                new GeneDef("comp.medoCoragem",    Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.curiosidade",    Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.agressividade",  Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.altruismo",      Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.sociabilidade",  Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.territorialidade",Bloco.Comportamental,Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.exploracao",     Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.aprendizagem",   Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.6f),
                new GeneDef("comp.vigilancia",     Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.cooperacao",     Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.lideranca",      Bloco.Comportamental, Heranca.Poligenico, 0.010f, 0.5f),
                new GeneDef("comp.nomadismo",      Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.armazenamento",  Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
                new GeneDef("comp.invParental",    Bloco.Comportamental, Heranca.Poligenico, 0.012f, 0.5f),
            };

            Index = new Dictionary<string, int>(Defs.Length);
            for (int i = 0; i < Defs.Length; i++) Index[Defs[i].Id] = i;
        }
    }
}
