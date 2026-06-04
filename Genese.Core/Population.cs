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
        public readonly Social     Social = new();   // camada social emergente (E05/M05)
        public readonly Speciation Spec   = new();   // motor evolutivo (E06/M03)
        // Camada simbólica (E07/M08-M10): inicializada em Seed()
#pragma warning disable CS8618
        public Language Language { get; private set; }
        public Culture  Culture  { get; private set; }
        public Belief   Belief   { get; private set; }
        private Rng _symbol;
#pragma warning restore CS8618

        public int Cap = int.MaxValue;
        private int _nextId;
        private int _nextLinId = 1;  // 0 = linhagem original; >0 = novas linhagens híbridas (M03)
        private int[] _nearIdx   = Array.Empty<int>();
        private float[] _nearDist = Array.Empty<float>();
        private int[] _nearCount  = Array.Empty<int>();  // vizinhos dentro do raio de densidade

        // ── Stats de dinâmica populacional (janela de 50 ticks) ──────────────
        public int BirthsRecent { get; private set; }
        public int DeathsRecent { get; private set; }
        private int _birthsPeriod, _deathsPeriod;

        /// <summary>Pressão global de população sobre a reprodução [0..0.09].</summary>
        public float GlobalPopPressure { get; private set; }
        private float _globalPopPressure;

        public int Count
        {
            get { int n = 0; for (int i = 0; i < Creatures.Count; i++) if (Creatures[i].Alive) n++; return n; }
        }

        /// <param name="xMin">Limite esquerdo da faixa de spawn (inclusivo).</param>
        /// <param name="xMax">Limite direito da faixa de spawn (exclusivo; -1 = env.W).</param>
        public void Seed(Environment env, Rng rng, int n, Rng symbolRng = null, int xMin = 0, int xMax = -1)
        {
            // Camada simbólica: inicializa com sub-stream dedicado (ou derivado do spawn rng)
            _symbol  = symbolRng ?? rng.Fork(0xCE17_B001UL);
            Language = new Language(_symbol);
            Culture  = new Culture();
            Belief   = new Belief();

            if (xMax < 0) xMax = env.W;
            int bandW = Math.Max(1, xMax - xMin);
            // minDist proporcional à faixa disponível (evita deadlock em mapas estreitos)
            float minDist = Math.Min(10f, bandW * 0.35f);

            int tries = 0;
            while (Creatures.Count < n && tries < n * 160)
            {
                tries++;
                int x = rng.Range(xMin, xMax), y = rng.Range(0, env.H);
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
            ApplyDensityFoodPressure(env);

            // Pressão global de população: quanto mais criaturas, mais difícil reproduzir.
            // Escala linear até ao cap de 0.09 — ao redor de 60 criaturas fica quase impossível.
            int alive = Count;
            _globalPopPressure = Math.Min(0.09f, alive * 0.0015f);
            GlobalPopPressure  = _globalPopPressure;

            var births = new List<Creature>();
            int aliveBefore = alive;
            for (int i = 0; i < Creatures.Count; i++)
            {
                var c = Creatures[i];
                if (!c.Alive) continue;

                c.Age++;
                // upkeep metabólico + estresse de superlotação (cada vizinho próximo aumenta o custo)
                float densityStress = _nearCount[i] * 0.0022f;
                c.Energy -= 0.003f + 0.005f * c.Metabolism + 0.003f * c.Size + densityStress;

                int cx = Clamp((int)c.X, env.W), cy = Clamp((int)c.Y, env.H);
                Act(c, env, cx, cy, decision, repro, births, i, tick);

                int maxAge = (int)(250 + 900 * c.Trait("longevidade"));
                if (c.Energy <= 0f || c.Age > maxAge) c.Alive = false;
            }

            // Contabiliza mortes e nascimentos para stats da HUD
            int aliveAfterProcess = Count;
            _deathsPeriod += aliveBefore - aliveAfterProcess;
            _birthsPeriod += births.Count;

            foreach (var b in births) { Creatures.Add(b); }
            Creatures.RemoveAll(x => !x.Alive);

            // Publica stats numa janela de 50 ticks
            if (tick % 50 == 0)
            {
                BirthsRecent = _birthsPeriod;
                DeathsRecent = _deathsPeriod;
                _birthsPeriod = 0;
                _deathsPeriod = 0;
            }

            // camada social emergente (M05) — cadências mais frequentes p/ interações ricas
            if (tick % 2 == 0) Social.Interact(Creatures);
            if (tick % 12 == 0)
            {
                Social.Decay(0.97f);
                for (int i = 0; i < Creatures.Count; i++) Creatures[i].Prestige *= 0.94f;
                Culture.Propagate(Creatures, Social, _symbol);
            }
            if (tick % 15 == 0) { Social.DetectGroups(Creatures); Social.UpdateFigures(Creatures); }

            // Camada simbólica (E07/M08-M10)
            if (tick % 20 == 0)
                Language.Step(Count, Social.GroupCount, Social.FigureCount, tick, _symbol);
            if (tick % 30 == 0)
                Belief.Step(Count, Social.GroupCount, Social.FigureCount, _symbol);
            if (tick % 35 == 0 && Social.FigureCount > 0)
                SpawnCulturalSymbols();
        }

        /// <summary>
        /// Figuras criam memes de forma causal (M09 §4.1): líderes geram Valores,
        /// parentais geram Ritos — nunca espontâneos.
        /// </summary>
        void SpawnCulturalSymbols()
        {
            for (int i = 0; i < Creatures.Count; i++)
            {
                var c = Creatures[i];
                if (!c.IsFigure || !c.Alive) continue;
                if (Culture.SymbolCount >= 12) break; // limite de atenção coletiva
                string role = c.Role();
                if (role == "Líder" && c.Trait("comp.lideranca") > 0.65f)
                    Culture.SpawnSymbol(SymbolType.Valor, 0.25f + 0.45f * c.Trait("comp.lideranca"), c.Id, _symbol);
                else if (role == "Parental")
                    Culture.SpawnSymbol(SymbolType.Rito, 0.20f + 0.40f * c.Trait("comp.invParental"), c.Id, _symbol);
            }
        }

        // Reduz a regeneração de comida nas células mais populosas (depleção por concentração).
        // Cada criatura viva numa célula consome uma fração extra da capacidade de recuperação.
        void ApplyDensityFoodPressure(Environment env)
        {
            // Conta criaturas vivas por célula
            var count = new int[env.W * env.H];
            foreach (var c in Creatures)
            {
                if (!c.Alive) continue;
                int cx = Clamp((int)c.X, env.W), cy = Clamp((int)c.Y, env.H);
                count[env.Idx(cx, cy)]++;
            }
            // Aplica depleção proporcional à ocupação local
            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] < 2) continue;
                // Cada habitante extra drena 3% da comida disponível na célula
                float drain = (count[i] - 1) * 0.03f;
                env.Comida[i] = Math.Max(0f, env.Comida[i] - drain * env.Comida[i]);
            }
        }

        // vizinho vivo mais próximo + contagem de vizinhos no raio de densidade
        void ComputeNearest()
        {
            int n = Creatures.Count;
            if (_nearIdx.Length < n)
            {
                _nearIdx   = new int[n];
                _nearDist  = new float[n];
                _nearCount = new int[n];
            }
            for (int i = 0; i < n; i++) { _nearIdx[i] = -1; _nearDist[i] = 1e9f; _nearCount[i] = 0; }

            const float DensityR2 = 36f; // raio 6: conta todos os vizinhos nessa esfera

            for (int i = 0; i < n; i++)
            {
                var a = Creatures[i]; if (!a.Alive) continue;
                for (int j = i + 1; j < n; j++)
                {
                    var b = Creatures[j]; if (!b.Alive) continue;
                    float dx = a.X - b.X, dy = a.Y - b.Y, d2 = dx * dx + dy * dy;
                    if (d2 < _nearDist[i]) { _nearDist[i] = d2; _nearIdx[i] = j; }
                    if (d2 < _nearDist[j]) { _nearDist[j] = d2; _nearIdx[j] = i; }
                    if (d2 < DensityR2)    { _nearCount[i]++; _nearCount[j]++; }
                }
            }
        }

        // ações: 0 Forragear · 1 Mover p/ comida · 2 Explorar · 3 Descansar · 4 Reproduzir · 5 Aproximar (social)
        private void Act(Creature c, Environment env, int cx, int cy, Rng decision, Rng repro, List<Creature> births, int self, ulong tick)
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

            float expl  = c.Trait("comp.exploracao"),  curio  = c.Trait("comp.curiosidade");
            float social = c.Trait("comp.sociabilidade");
            float coragem = c.Trait("comp.medoCoragem");   // M02: alto = corajoso, baixo = medroso
            float nomad   = c.Trait("comp.nomadismo");     // M02: alto = ânsia de se mover
            float vigil   = c.Trait("sentidos.percepcao"); // M02: percepção — detecta comida melhor
            float armaz   = c.Trait("comp.armazenamento"); // M02: poupa → repousa mais cedo
            // Limiar de reprodução = base + pressão local (densidade) + pressão global (pop total)
            float localPressure  = Math.Min(0.06f, _nearCount[self] * 0.025f);
            float reproThresh    = 0.90f + localPressure + _globalPopPressure;
            bool canReprod = c.Energy > reproThresh
                          && c.Age > 110
                          && (tick - c.LastReproTick) > 350;

            // vizinho mais próximo (para socializar/formar bando — M05 §4.1)
            int nn = _nearIdx[self]; float nd = nn >= 0 ? (float)Math.Sqrt(_nearDist[self]) : 999f;
            float affin = nn >= 0 ? Social.Affinity(c.Id, Creatures[nn].Id) : 0f;

            const float Impossible = -1e9f; // ação inviável → prob. ~0 no softmax
            float[] u = new float[6];
            u[0] = hunger * 1.3f * (0.15f + food);                                              // forragear aqui
            // Vigilância/percepção amplifica detecção de comida distante (M02 §3)
            float foodBonus = 0.4f + 0.6f * expl + 0.35f * vigil;
            u[1] = bestFood > food ? hunger * (0.15f + bestFood) * foodBonus : Impossible;      // ir até comida
            // Nomadismo e curiosidade amplificam explorar; coragem reduz hesitação (M02 §6)
            u[2] = 0.14f + 0.45f * expl + 0.30f * curio + 0.20f * nomad + 0.10f * coragem;    // explorar
            // Armazenamento: poupa energia mais cedo; medo aumenta tendência a pousar (M02 §6)
            float restThresh = 0.55f - 0.12f * armaz + 0.08f * (1f - coragem);
            u[3] = c.Energy > restThresh ? 0.38f + 0.15f * armaz : 0.07f;                      // descansar
            u[4] = canReprod ? c.Energy * 0.38f * (0.12f + c.Trait("fertilidade")) : Impossible; // reproduzir
            // Coragem baixa diminui aproximação de estranhos (M02 §6 — interação com M05)
            u[5] = (nn >= 0 && nd > 1.2f && nd < 12f)
                 ? (0.12f + 0.55f * social + 0.45f * affin + 0.12f * coragem) * (0.4f + 0.6f * c.Energy)
                 : Impossible;                                                                   // aproximar

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
                    // Investimento parental (M02 §4.2): alta invParental → mais energia ao filho
                    float invPar = c.Trait("comp.invParental");
                    float parentCost      = 0.75f + 0.15f * invPar;   // reprodução muito custosa
                    float offspringEnergy = 0.13f + 0.07f * invPar;   // filho nasce fraco
                    c.Energy -= parentCost;
                    c.LastReproTick = tick;
                    c.ReproCount++; c.Prestige += 0.05f;
                    births.Add(new Creature(_nextId++, child, c.X, c.Y) { Energy = offspringEnergy });
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
        private static int SoftmaxPick(float[] u, float flex, Rng rng)
        {
            float temp = Math.Max(0.05f, flex);
            float max = float.NegativeInfinity;
            for (int i = 0; i < u.Length; i++) if (u[i] > max) max = u[i];
            double sum = 0; double[] e = new double[u.Length];
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
            // Camada simbólica (E07)
            _symbol.WriteState(w);
            Language.Write(w);
            Culture.Write(w);
            Belief.Write(w);
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
            // Camada simbólica (E07)
            _symbol  = Rng.ReadState(r);
            Language = Language.Read(r);
            Culture  = new Culture(); Culture.ReadInto(r);
            Belief   = new Belief();  Belief.ReadInto(r);
        }
    }
}
