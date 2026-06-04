using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Camada social EMERGENTE (M05). Não existe objeto "grupo" ou "hierarquia" imposto:
    /// guardamos só relações ESPARSAS entre pares que se encontraram (afinidade, dominância,
    /// dívida de reciprocidade) e DETECTAMOS grupos por clustering (union-find) sobre o grafo
    /// de afinidade. Hierarquia = soma das vitórias de dominância (comparação de atributos,
    /// nunca sorteio). Figuras emergem por limiar de prestígio. Tudo determinístico.
    /// </summary>
    public sealed class Social
    {
        public struct Rel { public float Affinity; public sbyte Dom; public float Debt; }

        public readonly Dictionary<long, Rel> Rels = new();
        public int GroupCount;          // nº de grupos (componentes com ≥2 indivíduos)
        public int FigureCount;

        public const float GroupAffinity = 0.4f;   // afinidade mínima para ligar no grafo
        public const float FigurePrestige = 1.0f;  // limiar de Figura
        public const float InteractRadius = 2.0f;

        static long Key(int a, int b) => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;

        public float Affinity(int a, int b) => Rels.TryGetValue(Key(a, b), out var r) ? r.Affinity : 0f;

        // comparação de dominância (M05 §4.2): determinística, por atributos
        public static float DomScore(Creature c)
            => 0.5f * c.Trait("comp.agressividade") + 0.3f * c.Size
             + 0.12f * Clamp01(c.Prestige) + 0.08f * Clamp01(c.DominanceWins / 25f);

        /// <summary>Interações locais entre pares próximos (O(n²) — ok p/ N moderado).</summary>
        public void Interact(List<Creature> cs)
        {
            float r2 = InteractRadius * InteractRadius;
            for (int i = 0; i < cs.Count; i++)
            {
                var a = cs[i]; if (!a.Alive) continue;
                for (int j = i + 1; j < cs.Count; j++)
                {
                    var b = cs[j]; if (!b.Alive) continue;
                    float dx = a.X - b.X, dy = a.Y - b.Y, d2 = dx * dx + dy * dy;
                    if (d2 > r2) continue;
                    UpdatePair(a, b, d2);
                }
            }
        }

        void UpdatePair(Creature a, Creature b, float d2)
        {
            long k = Key(a.Id, b.Id);
            Rels.TryGetValue(k, out var r);

            // afinidade cresce com sociabilidade/cooperação dos dois (laço social)
            float pull = 0.05f * ((a.Trait("comp.sociabilidade") + b.Trait("comp.sociabilidade")) * 0.5f
                                + (a.Trait("comp.cooperacao") + b.Trait("comp.cooperacao")) * 0.25f);
            r.Affinity = Clamp01(r.Affinity + pull);

            // dominância: o de maior atributo vence; registra e ganha prestígio
            float da = DomScore(a), db = DomScore(b);
            Creature win = da >= db ? a : b, lose = da >= db ? b : a;
            r.Dom = (sbyte)(da >= db ? 1 : -1);
            win.DominanceWins++; win.Prestige += 0.0015f;

            // reciprocidade: altruísta divide energia com o parceiro (cria dívida + afinidade)
            float alt = win.Trait("comp.altruismo");
            if (alt > 0.6f && win.Energy > lose.Energy + 0.15f)
            {
                float give = 0.01f * alt;
                win.Energy -= give; lose.Energy = Clamp01(lose.Energy + give);
                r.Debt = Clamp01(r.Debt + give); r.Affinity = Clamp01(r.Affinity + 0.02f);
            }
            else if (d2 < 1.5f) // disputa por espaço/recurso: dominante leva vantagem
            {
                float take = 0.006f;
                win.Energy = Clamp01(win.Energy + take); lose.Energy = Math.Max(0f, lose.Energy - take);
            }

            Rels[k] = r;
        }

        public void Decay(float factor)
        {
            if (Rels.Count == 0) return;
            var dead = new List<long>();
            var keys = new List<long>(Rels.Keys);
            foreach (var k in keys)
            {
                var r = Rels[k]; r.Affinity *= factor; r.Debt *= factor;
                if (r.Affinity < 0.02f && r.Debt < 0.02f) dead.Add(k); else Rels[k] = r;
            }
            foreach (var k in dead) Rels.Remove(k);
        }

        /// <summary>Detecta grupos por densidade de laços (union-find). Grupos são LIDOS, não criados.</summary>
        public void DetectGroups(List<Creature> cs)
        {
            int n = cs.Count;
            var index = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++) index[cs[i].Id] = i;
            var parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            void Union(int a, int b) { int ra = Find(a), rb = Find(b); if (ra != rb) parent[Math.Max(ra, rb)] = Math.Min(ra, rb); }

            // ligar pares com afinidade alta (ordenado por chave p/ determinismo)
            var keys = new List<long>(Rels.Keys); keys.Sort();
            foreach (var k in keys)
            {
                if (Rels[k].Affinity < GroupAffinity) continue;
                int ai = (int)(k >> 32), bi = (int)(uint)k;
                if (index.TryGetValue(ai, out int ia) && index.TryGetValue(bi, out int ib)) Union(ia, ib);
            }

            // tamanho de cada componente
            var size = new int[n];
            for (int i = 0; i < n; i++) size[Find(i)]++;

            // rótulos compactos só para componentes com ≥2 (um "grupo")
            var label = new Dictionary<int, int>();
            int next = 0;
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (size[root] >= 2)
                {
                    if (!label.TryGetValue(root, out int lab)) { lab = next++; label[root] = lab; }
                    cs[i].GroupId = lab;
                }
                else cs[i].GroupId = -1;
            }
            GroupCount = next;
        }

        /// <summary>Figuras: prestígio alto + pendor de liderança (M05 §4.4).</summary>
        public void UpdateFigures(List<Creature> cs)
        {
            int f = 0;
            for (int i = 0; i < cs.Count; i++)
            {
                var c = cs[i];
                c.IsFigure = c.Prestige >= FigurePrestige && c.Trait("comp.lideranca") > 0.55f;
                if (c.IsFigure) f++;
            }
            FigureCount = f;
        }

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        // ----- snapshot (ordenado por chave p/ bytes idênticos entre execuções) -----
        public void Write(BinaryWriter w)
        {
            var keys = new List<long>(Rels.Keys); keys.Sort();
            w.Write(keys.Count);
            foreach (var k in keys) { var r = Rels[k]; w.Write(k); w.Write(r.Affinity); w.Write(r.Dom); w.Write(r.Debt); }
            w.Write(GroupCount); w.Write(FigureCount);
        }
        public void ReadInto(BinaryReader rd)
        {
            Rels.Clear();
            int n = rd.ReadInt32();
            for (int i = 0; i < n; i++) { long k = rd.ReadInt64(); Rels[k] = new Rel { Affinity = rd.ReadSingle(), Dom = rd.ReadSByte(), Debt = rd.ReadSingle() }; }
            GroupCount = rd.ReadInt32(); FigureCount = rd.ReadInt32();
        }
    }
}
