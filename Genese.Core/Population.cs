using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>
    /// Agentes (M04): cada indivíduo decide por UTILIDADE (utility AI) — pontua as ações
    /// possíveis a partir de necessidades + traços + ambiente e escolhe via softmax, com
    /// temperatura derivada da flexibilidade (curiosidade). Nada é sorteado "no vácuo":
    /// a dispersão da escolha vem do indivíduo (GDD §1.5). Forragear consome o recurso da
    /// célula (M07); energia cai com o metabolismo; reprodução usa M01/E02.
    /// </summary>
    public sealed class Population
    {
        public readonly List<Creature> Creatures = new();
        public readonly Social Social = new();       // camada social emergente (E05/M05)
        public readonly Speciation Spec = new();     // motor evolutivo (E06/M03)
        public int Cap = 1500;
        private int _nextId;
        private int _nextLinId = 1;  // 0 = linhagem original; >0 = novas linhagens híbridas (M03)
        private int[] _nearIdx = Array.Empty<int>();
        private float[] _nearDist = Array.Empty<float>();

        public int Count
        {
            get { int n = 0; for (int i = 0; i < Creatures.Count; i++) if (Creatures[i].Alive) n++; return n; }
        }

        public void Seed(Environment env, Rng rng, int n)
        {
            const float minDist = 10f;  // espaçamento mínimo entre indivíduos no spawn (evita sobreposição visual)
            int tries = 0;
            while (Creatures.Count < n && tries < n * 120)
            {
                tries++;
                int x = rng.Range(0, env.W), y = rng.Range(0, env.H);
                if (env.IsBarrier(x, y)) continue;
                float fx = x + 0.5f, fy = y + 0.5f;
                bool tooClose = false;
                for (int i = 0; i < Creatures.Count; i++)
                {
                    float dx = Creatures[i].X - fx, dy = Creatures[i].Y - fy;
                    if (dx * dx + dy * dy < minDist * minDist) { tooClose = true; break; }
                }
                if (tooClose) continue;
                Creatures.Add(new Creature(_nextId++, Genome.Founder(rng.Fork((ulong)_nextId)), fx, fy));
            }
        }

        // ---- passo de simulação ----
        public void Step(Environment env, ulong tick, Rng decision, Rng repro)
        {
            ComputeNearest();
            var births = new List<Creature>();
            for (int i = 0; i < Creatures.Count; i++)
            {
                var c = Creatures[i];
                if (!c.Alive) continue;

                c.Age++;
                // upkeep metabólico (necessidade de comer sobe com o tempo)
                c.Energy -= 0.003f + 0.005f * c.Metabolism + 0.003f * c.Size;

                int cx = Clamp((int)c.X, env.W), cy = Clamp((int)c.Y, env.H);
                Act(c, env, cx, cy, decision, repro, births, i);

                int maxAge = (int)(250 + 900 * c.Trait("longevidade"));
                if (c.Energy <= 0f || c.Age > maxAge) c.Alive = false;
            }

            int alive = Count;
            foreach (var b in births) { if (alive >= Cap) break; Creatures.Add(b); alive++; }
            Creatures.RemoveAll(x => !x.Alive);

            // camada social emergente (M05): interações locais, esquecimento, detecção de grupos
            if (tick % 4 == 0) Social.Interact(Creatures);
            if (tick % 20 == 0)
            {
                Social.Decay(0.97f);
                // prestígio também esmaece: reflete posição RECENTE, não soma vitalícia —
                // por isso Figuras permanecem RARAS (só quem sustenta vitórias/reprodução).
                for (int i = 0; i < Creatures.Count; i++) Creatures[i].Prestige *= 0.94f;
            }
            if (tick % 25 == 0) { Social.DetectGroups(Creatures); Social.UpdateFigures(Creatures); }
        }

        // vizinho vivo mais próximo de cada criatura (para a ação de aproximar/bando)
        void ComputeNearest()
        {
            int n = Creatures.Count;
            if (_nearIdx.Length < n) { _nearIdx = new int[n]; _nearDist = new float[n]; }
            for (int i = 0; i < n; i++) { _nearIdx[i] = -1; _nearDist[i] = 1e9f; }
            for (int i = 0; i < n; i++)
            {
                var a = Creatures[i]; if (!a.Alive) continue;
                for (int j = i + 1; j < n; j++)
                {
                    var b = Creatures[j]; if (!b.Alive) continue;
                    float dx = a.X - b.X, dy = a.Y - b.Y, d2 = dx * dx + dy * dy;
                    if (d2 < _nearDist[i]) { _nearDist[i] = d2; _nearIdx[i] = j; }
                    if (d2 < _nearDist[j]) { _nearDist[j] = d2; _nearIdx[j] = i; }
                }
            }
        }

        // ações: 0 Forragear · 1 Mover p/ comida · 2 Explorar · 3 Descansar · 4 Reproduzir · 5 Aproximar (social)
        private void Act(Creature c, Environment env, int cx, int cy, Rng decision, Rng repro, List<Creature> births, int self)
        {
            float hunger = 1f - c.Energy;
            int here = env.Idx(cx, cy);
            float food = env.Comida[here];

            // melhor vizinho em comida
            float bestFood = 0f; int bx = cx, by = cy;
            int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };
            for (int k = 0; k < 4; k++)
            {
                int nx = cx + dx[k], ny = cy + dy[k];
                if (nx < 0 || ny < 0 || nx >= env.W || ny >= env.H || env.IsBarrier(nx, ny)) continue;
                float f = env.Comida[env.Idx(nx, ny)];
                if (f > bestFood) { bestFood = f; bx = nx; by = ny; }
            }

            float expl = c.Trait("comp.exploracao"), curio = c.Trait("comp.curiosidade"), social = c.Trait("comp.sociabilidade");
            bool canReprod = c.Energy > 0.62f && c.Age > 40;

            // vizinho mais próximo (para socializar/formar bando — M05 §4.1)
            int nn = _nearIdx[self]; float nd = nn >= 0 ? (float)Math.Sqrt(_nearDist[self]) : 999f;
            float affin = nn >= 0 ? Social.Affinity(c.Id, Creatures[nn].Id) : 0f;

            const float Impossible = -1e9f; // ação inviável → prob. ~0 no softmax
            Span<float> u = stackalloc float[6];
            u[0] = hunger * 1.3f * (0.15f + food);                         // forragear aqui
            u[1] = bestFood > food ? hunger * (0.15f + bestFood) * (0.4f + 0.6f * expl) : Impossible; // ir até comida melhor
            u[2] = 0.18f + 0.5f * expl + 0.4f * curio;                     // explorar
            u[3] = c.Energy > 0.6f ? 0.4f : 0.08f;                         // descansar
            u[4] = canReprod ? c.Energy * 1.6f * (0.3f + c.Trait("fertilidade")) : Impossible; // reproduzir
            u[5] = (nn >= 0 && nd > 1.2f && nd < 12f) ? (0.15f + 0.6f * social + 0.5f * affin) * (0.4f + 0.6f * c.Energy) : Impossible; // aproximar

            float flex = 0.15f + 0.7f * curio;                            // flexibilidade comportamental
            int action = SoftmaxPick(u, flex, decision);

            switch (action)
            {
                case 0: // forragear
                    float got = env.Harvest(cx, cy, 0.12f + 0.12f * c.Size);
                    c.Energy = Math.Min(1f, c.Energy + got * 1.5f);
                    c.ForageCount++;
                    break;
                case 1: // mover em direção à comida
                    MoveToward(c, bx + 0.5f, by + 0.5f, env);
                    break;
                case 2: // explorar (passo p/ célula vizinha de terra)
                    int dir = decision.Range(0, 4);
                    int ex = cx + dx[dir], ey = cy + dy[dir];
                    if (ex >= 0 && ey >= 0 && ex < env.W && ey < env.H && !env.IsBarrier(ex, ey)) MoveToward(c, ex + 0.5f, ey + 0.5f, env);
                    c.ExploreCount++;
                    break;
                case 3: // descansar: NÃO gera energia (energia só vem de comida — escassez é real)
                    break;
                case 4: // reproduzir — sexual (se parceiro próximo e compatível) ou assexuada (M03/E06)
                    if (!canReprod) break;
                    float scarcity = 1f - food;

                    // M03 §4.1: pressão mutagênica causal (ambiente × pop)
                    float mutPress  = env.PressaoMutagenica(cx, cy);
                    float popFactor = Speciation.FatorTamanhoPop(Creatures.Count);
                    float effPress  = scarcity * 0.6f + (mutPress - 1f) * 0.25f;

                    // Reprodução sexual quando parceiro próximo e geneticamente compatível
                    Genome dadGen   = c.Genome; // fallback: assexuada
                    int   childLin  = c.Genome.LinhagemId;
                    if (nn >= 0 && nd < 3f)
                    {
                        var mate = Creatures[nn];
                        if (mate.Alive && mate.Energy > 0.55f && mate.Age > 40 &&
                            Speciation.PodeReproduzir(c, mate))
                        {
                            dadGen   = mate.Genome;
                            childLin = Speciation.LinhaDaDescendencia(c.Genome, mate.Genome, ref _nextLinId);
                            if (childLin != c.Genome.LinhagemId) Spec.EspeciacaoCount++;
                        }
                    }

                    var child = Reproduction.Reproduce(c.Genome, dadGen, repro, effPress, popFactor);
                    child.LinhagemId = childLin;  // M03: aplica cisão de linhagem se houver
                    c.Energy -= 0.45f;
                    c.ReproCount++; c.Prestige += 0.05f;
                    births.Add(new Creature(_nextId++, child, c.X, c.Y) { Energy = 0.38f });
                    break;
                case 5: // aproximar do vizinho (gregarismo → bandos emergem)
                    MoveToward(c, Creatures[nn].X, Creatures[nn].Y, env);
                    c.ExploreCount++;
                    break;
            }
        }

        private static void MoveToward(Creature c, float tx, float ty, Environment env)
        {
            float dx = tx - c.X, dy = ty - c.Y;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d < 1e-4f) return;
            float nx = c.X + dx / d * c.Speed, ny = c.Y + dy / d * c.Speed;
            int ix = Clamp((int)nx, env.W), iy = Clamp((int)ny, env.H);
            if (!env.IsBarrier(ix, iy)) { c.X = nx; c.Y = ny; }
        }

        // softmax sobre utilidades; temperatura = flexibilidade (rígido→argmax)
        private static int SoftmaxPick(Span<float> u, float flex, Rng rng)
        {
            float temp = Math.Max(0.05f, flex);
            float max = float.NegativeInfinity;
            for (int i = 0; i < u.Length; i++) if (u[i] > max) max = u[i];
            double sum = 0; Span<double> e = stackalloc double[8];
            for (int i = 0; i < u.Length; i++) { e[i] = Math.Exp((u[i] - max) / temp); sum += e[i]; }
            double r = rng.NextDouble() * sum, acc = 0;
            for (int i = 0; i < u.Length; i++) { acc += e[i]; if (r <= acc) return i; }
            return u.Length - 1;
        }

        private static int Clamp(int v, int n) => v < 0 ? 0 : (v >= n ? n - 1 : v);

        // ---- snapshot ----
        public void Write(BinaryWriter w)
        {
            w.Write(_nextId); w.Write(_nextLinId); w.Write(Cap);
            int alive = Count; w.Write(alive);
            for (int i = 0; i < Creatures.Count; i++)
            {
                var c = Creatures[i]; if (!c.Alive) continue;
                w.Write(c.Id); w.Write(c.X); w.Write(c.Y); w.Write(c.Energy); w.Write(c.Age);
                w.Write(c.Prestige); w.Write(c.GroupId); w.Write(c.DominanceWins);
                w.Write(c.ForageCount); w.Write(c.ExploreCount); w.Write(c.ReproCount); w.Write(c.IsFigure);
                c.Genome.Write(w);
            }
            Social.Write(w);
            Spec.Write(w);
        }
        public void ReadInto(BinaryReader r)
        {
            Creatures.Clear();
            _nextId = r.ReadInt32(); _nextLinId = r.ReadInt32(); Cap = r.ReadInt32();
            int n = r.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                int id = r.ReadInt32(); float x = r.ReadSingle(), y = r.ReadSingle(), en = r.ReadSingle(); int age = r.ReadInt32();
                float prestige = r.ReadSingle(); int grp = r.ReadInt32(), dw = r.ReadInt32();
                int fc = r.ReadInt32(), ec = r.ReadInt32(), rc = r.ReadInt32(); bool fig = r.ReadBoolean();
                var g = Genome.Read(r);
                Creatures.Add(new Creature(id, g, x, y) { Energy = en, Age = age, Prestige = prestige, GroupId = grp, DominanceWins = dw, ForageCount = fc, ExploreCount = ec, ReproCount = rc, IsFigure = fig });
            }
            Social.ReadInto(r);
            Spec.ReadInto(r);
        }
    }
}
