using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    public enum LanguageStage : byte { Gestual, Vocal, Proto, Gramatica, Escrita }

    /// <summary>
    /// Língua emergente da civilização (M08). Inventário fonêmico determinístico,
    /// léxico procedural (pares significado→forma), cinco estágios causais desbloqueados
    /// por pré-condições de estado (nunca por tempo), e drift que espelha o isolamento
    /// genômico (M03): populações isoladas acumulam divergência linguística independente.
    /// </summary>
    public sealed class Language
    {
        // Inventário fonêmico único desta população (7-10 símbolos de 0-15)
        public readonly byte[] Phonemes;
        // Léxico: significado → forma (gerada a partir do inventário)
        public readonly Dictionary<string, string> Lexicon = new();
        public LanguageStage Stage = LanguageStage.Gestual;
        // Contador de eventos de deriva (palavras pouco usadas que derivaram)
        public int DriftCount;

        // Conjunto de significados nomeáveis — ordenado para determinismo
        static readonly string[] Meanings = {
            "agua", "ceu", "chegar", "comida", "criar", "cria", "deus",
            "fogo", "forte", "fraco", "fuga", "grupo", "lider", "lua",
            "morte", "outro", "partir", "perigo", "sol", "terra"
        };

        public Language(Rng rng)
        {
            int n = rng.Range(7, 11);
            var seen = new System.Collections.Generic.HashSet<byte>();
            var list = new System.Collections.Generic.List<byte>(n);
            while (list.Count < n)
            {
                byte f = (byte)rng.Range(0, 16);
                if (seen.Add(f)) list.Add(f);
            }
            Phonemes = list.ToArray();
        }

        // Construtor privado para deserialização
        Language(byte[] phonemes) { Phonemes = phonemes; }

        string GenForm(string meaning, Rng rng)
        {
            int len = Stage <= LanguageStage.Vocal ? 2 :
                      Stage <= LanguageStage.Proto  ? 3 : 4;
            // hash do significado + DriftCount → semente do sub-stream (deriva causal)
            ulong h = 5381UL;
            foreach (char c in meaning) h = h * 33 + c;
            var lr = rng.Fork(h + (ulong)DriftCount);
            var buf = new char[len];
            for (int i = 0; i < len; i++)
                buf[i] = (char)('a' + Phonemes[lr.Range(0, Phonemes.Length)]);
            return new string(buf);
        }

        /// <summary>Nomeia um significado: retorna a forma existente ou cria uma nova.</summary>
        public string Name(string meaning, Rng rng)
        {
            if (!Lexicon.ContainsKey(meaning))
                Lexicon[meaning] = GenForm(meaning, rng);
            return Lexicon[meaning];
        }

        /// <summary>
        /// Passo da camada linguística. Expande léxico por necessidade causal e avança
        /// estágio quando pré-condições de estado (população, grupos, figuras) são atingidas.
        /// </summary>
        public void Step(int popCount, int groupCount, int figureCount, ulong tick, Rng rng)
        {
            // Nomear novos conceitos (causal: precisa de ao menos 2 criaturas vivas)
            if (tick % 50UL == 0 && popCount >= 2)
            {
                int toName = Stage >= LanguageStage.Proto ? 3 : 1;
                for (int i = 0; i < toName; i++)
                {
                    int idx = rng.Range(0, Meanings.Length);
                    Name(Meanings[idx], rng);
                }
            }

            // Deriva causal: palavra muda ao ser pouco usada (fonologia viva)
            if (tick % 200UL == 0 && Lexicon.Count > 0)
            {
                DriftCount++;
                // Ordenação explícita garante determinismo após restore (Dictionary não preserva ordem)
                var keys = new List<string>(Lexicon.Keys); keys.Sort();
                string k = keys[rng.Range(0, keys.Count)];
                Lexicon[k] = GenForm(k, rng);
            }

            // Avança estágio por pré-condições CAUSAIS (M08 §4.1)
            AdvanceStage(popCount, groupCount, figureCount);
        }

        void AdvanceStage(int pop, int groups, int figures)
        {
            Stage = Stage switch
            {
                LanguageStage.Gestual   when pop    >= 3                              => LanguageStage.Vocal,
                LanguageStage.Vocal     when groups >= 1 && Lexicon.Count >= 5        => LanguageStage.Proto,
                LanguageStage.Proto     when figures >= 1 && Lexicon.Count >= 12      => LanguageStage.Gramatica,
                LanguageStage.Gramatica when figures >= 2 && Lexicon.Count >= 18      => LanguageStage.Escrita,
                _ => Stage
            };
        }

        /// <summary>
        /// Distância linguística entre dois idiomas (M08 §4.3).
        /// Combina distância fonêmica (jaccard inverso) e lexical (formas divergentes).
        /// </summary>
        public static float Distance(Language a, Language b)
        {
            // Fonêmica: jaccard inverso dos inventários
            var setA = new System.Collections.Generic.HashSet<byte>(a.Phonemes);
            int shared = 0, union = setA.Count;
            foreach (byte f in b.Phonemes) { if (setA.Contains(f)) shared++; else union++; }
            float phonDist = union == 0 ? 0f : 1f - (float)shared / union;

            // Lexical: formas em comum sobre total de conceitos abrangidos
            int lexShared = 0, lexTotal = 0;
            foreach (var kv in a.Lexicon)
            {
                lexTotal++;
                if (b.Lexicon.TryGetValue(kv.Key, out string f2) && f2 == kv.Value) lexShared++;
            }
            foreach (var kv in b.Lexicon) if (!a.Lexicon.ContainsKey(kv.Key)) lexTotal++;
            float lexDist = lexTotal == 0 ? 0f : 1f - (float)lexShared / lexTotal;

            return 0.4f * phonDist + 0.6f * lexDist;
        }

        // ----- snapshot -----
        public void Write(BinaryWriter w)
        {
            w.Write((byte)Stage);
            w.Write(DriftCount);
            w.Write((byte)Phonemes.Length);
            foreach (byte p in Phonemes) w.Write(p);
            // Léxico em ordem de chave para bytes idênticos entre execuções
            var keys = new List<string>(Lexicon.Keys); keys.Sort();
            w.Write(keys.Count);
            foreach (string k in keys) { w.Write(k); w.Write(Lexicon[k]); }
        }

        public static Language Read(BinaryReader r)
        {
            var stage = (LanguageStage)r.ReadByte();
            int drift = r.ReadInt32();
            int pn = r.ReadByte();
            var ph = new byte[pn];
            for (int i = 0; i < pn; i++) ph[i] = r.ReadByte();
            var lang = new Language(ph) { Stage = stage, DriftCount = drift };
            int lc = r.ReadInt32();
            for (int i = 0; i < lc; i++) { string k = r.ReadString(), v = r.ReadString(); lang.Lexicon[k] = v; }
            return lang;
        }
    }
}
