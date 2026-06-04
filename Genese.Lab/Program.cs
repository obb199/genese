using System;
using System.Linq;
using Genese.Core;

// ============================================================================
// GÊNESE — Laboratório do núcleo (console)
// Roda Genese.Core de forma visível/legível para inspeção e proposta de melhorias.
// Uso:  dotnet run --project Genese.Lab -- [seed] [geracoes] [pressaoAmbiental]
//   ex: dotnet run --project Genese.Lab -- 12345 400 0.4
// ============================================================================

ulong seed = args.Length > 0 && ulong.TryParse(args[0], out var s) ? s : 20260603UL;
int gens = args.Length > 1 && int.TryParse(args[1], out var g) ? g : 300;
float pressao = args.Length > 2 && float.TryParse(args[2], out var p) ? p : 0.4f;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Title("GÊNESE — Laboratório do Núcleo");
Console.WriteLine($"semente={seed}  gerações={gens}  pressão ambiental={pressao:0.00}");
Console.WriteLine("(troque com: dotnet run --project Genese.Lab -- <seed> <gerações> <pressão>)");

// ---------------------------------------------------------------------------
Title("1) E01 — Determinismo, RNG e Snapshot");
var simA = new Simulation(seed);
var simB = new Simulation(seed);
for (int i = 0; i < 10_000; i++) { simA.Step(); simB.Step(); }
bool replay = simA.Snapshot().SequenceEqual(simB.Snapshot());
Console.WriteLine($"Replay (mesma semente, 10.000 ticks) idêntico? {YesNo(replay)}");

byte[] snap = simA.Snapshot();
var simC = new Simulation(999); simC.Restore(snap);
Console.WriteLine($"Snapshot round-trip exato?                {YesNo(simC.Snapshot().SequenceEqual(snap))}  (tick restaurado={simC.Tick})");
Console.WriteLine($"Tamanho do snapshot: {snap.Length} bytes");

var rng = new Rng(seed);
Console.Write("5 amostras de RNG em [0,1): ");
Console.WriteLine(string.Join("  ", Enumerable.Range(0, 5).Select(_ => rng.NextDouble().ToString("0.000"))));
Console.WriteLine($"Sub-streams independentes? {YesNo(Independentes(seed))} (mutação não muda com consumo de outro stream)");

// ---------------------------------------------------------------------------
Title("2) E02 — Genoma fundador (genótipo)");
var founder = Genome.Founder(new Rng(seed), linhagemId: 1);
PrintGenome(founder);

// ---------------------------------------------------------------------------
Title("3) E02 — Herança (sem mutação = combinação exata dos pais)");
var mae = Genome.Founder(new Rng(seed + 1));
var pai = Genome.Founder(new Rng(seed + 2));
var filhoPuro = Reproduction.Reproduce(mae, pai, new Rng(seed).Fork(Streams.Mutation), 0f, mutationScale: 0f);
Console.WriteLine("gene                 mãe    pai   → filho   (modo de herança)");
foreach (var id in new[] { "corpo.tamanho", "corpo.textura", "resistencia.doenca", "comp.agressividade" })
{
    int idx = GeneRegistry.IndexOf(id);
    Console.WriteLine($"  {id,-18} {mae.Values[idx]:0.00}  {pai.Values[idx]:0.00}  →  {filhoPuro.Values[idx]:0.00}   ({GeneRegistry.Def(idx).Modo})");
}

// ---------------------------------------------------------------------------
Title($"4) E02 — Linhagem evoluindo por {gens} gerações (deriva + mutação causal)");
var mut = new Rng(seed).Fork(Streams.Mutation);
var atual = founder.Clone();
Console.WriteLine("ger    tamanho      agressividade   curiosidade    dist.→fundador");
for (int gen = 1; gen <= gens; gen++)
{
    atual = Reproduction.Reproduce(atual, atual, mut, pressao); // assexuada (mãe==pai)
    if (gen % Math.Max(1, gens / 10) == 0 || gen == gens)
    {
        Console.WriteLine($"{gen,4}  {Bar(atual.Get("corpo.tamanho"))} {Bar(atual.Get("comp.agressividade"))} {Bar(atual.Get("comp.curiosidade"))}  {Genome.Distance(atual, founder):0.000}");
    }
}

// ---------------------------------------------------------------------------
Title("5) M03 (prévia) — Duas linhagens isoladas divergem → especiação");
var a = founder.Clone(); var b = founder.Clone();
var rA = new Rng(seed ^ 0xA).Fork(Streams.Mutation);
var rB = new Rng(seed ^ 0xB).Fork(Streams.Mutation);
const float limiar = 0.18f;
Console.WriteLine("ger     distância(A,B)   estado");
for (int gen = 1; gen <= gens; gen++)
{
    a = Reproduction.Reproduce(a, a, rA, pressao);
    b = Reproduction.Reproduce(b, b, rB, pressao);
    if (gen % Math.Max(1, gens / 8) == 0 || gen == gens)
    {
        float dist = Genome.Distance(a, b);
        string estado = dist < limiar ? "mesma espécie (ainda cruzam)" : "★ DIVERGÊNCIA: incompatíveis (espécies distintas)";
        Console.WriteLine($"{gen,4}    {Bar(dist)}   {estado}");
    }
}
Console.WriteLine("(o limiar real e a consolidação da espécie virão em E06/M03)");

// ---------------------------------------------------------------------------
Title("6) E02 — População: média e dispersão de um traço (prévia de M02)");
int N = 60;
var pop = Enumerable.Range(0, N).Select(i => Genome.Founder(new Rng(seed + (ulong)i * 7))).ToList();
var rPop = new Rng(seed).Fork(Streams.Mutation);
for (int gen = 0; gen < gens; gen++)
    for (int i = 0; i < pop.Count; i++) pop[i] = Reproduction.Reproduce(pop[i], pop[i], rPop, pressao);
var vals = pop.Select(x => x.Get("comp.sociabilidade")).ToArray();
Console.WriteLine($"Sociabilidade da população (N={N}) após {gens} gerações:");
Histogram(vals);
Console.WriteLine($"média={vals.Average():0.000}  min={vals.Min():0.000}  max={vals.Max():0.000}");

// ---------------------------------------------------------------------------
Title($"7) E03 — Ambiente: mapa de biomas (mundo evoluído {gens} ticks)");
var world = new Simulation(seed, 64, 40);
for (int i = 0; i < gens; i++) world.Step();
var env = world.Env;
Console.WriteLine("(~=oceano  .=pradaria  T=floresta  :=deserto  *=tundra  ^=montanha  %=pântano  V=vulcânico)");
for (int y = 0; y < env.H; y += 2)
{
    var sb = new System.Text.StringBuilder("  ");
    for (int x = 0; x < env.W; x += 1) sb.Append(BiomeChar(env.BiomaAt(x, y)));
    Console.WriteLine(sb.ToString());
}
var counts = new int[8];
foreach (var bb in env.Bioma) counts[bb]++;
Console.WriteLine();
Console.WriteLine("Biomas: " + string.Join("  ", Enumerable.Range(0, 8).Select(i => $"{(Biome)i}={counts[i]}")));
Console.WriteLine($"altitude média={env.Altitude.Average():0.000} (deve ficar ~estável ao longo do tempo)");
int seca = env.BalancoAgua.Count(v => v < Genese.Core.Environment.DroughtThreshold);
Console.WriteLine($"Células em seca (balanço hídrico): {seca}   ·   Comida média={env.Comida.Average():0.00}   Água média={env.Agua.Average():0.00}");
bool conn = env.Connected(2, env.H / 2, env.W - 3, env.H / 2);
Console.WriteLine($"Conectividade terrestre de uma borda à outra (sem cruzar montanha/oceano)? {YesNo(conn)}");

// ---------------------------------------------------------------------------
Title($"8) E04 — Agentes vivos no mundo (IA de utilidade) — {gens} ticks");
var sim = new Simulation(seed, 64, 40, 60);
sim.Pop.Cap = 120;   // densidade representativa: o social é O(n²); mantém o Lab ágil
Console.WriteLine("tick    população              energia média");
for (int t = 1; t <= gens; t++)
{
    sim.Step();
    if (t % Math.Max(1, gens / 8) == 0 || t == gens)
    {
        var cr = sim.Pop.Creatures;
        double e = cr.Count > 0 ? cr.Average(x => x.Energy) : 0;
        Console.WriteLine($"{t,5}   {Bar(Math.Min(1f, sim.Pop.Count / 250f))} {sim.Pop.Count,4}      {e:0.00}");
    }
}
if (sim.Pop.Count > 0)
{
    var c = sim.Pop.Creatures[0];
    int maxGer = sim.Pop.Creatures.Max(z => z.Genome.Geracao);
    Console.WriteLine($"Geração máxima atingida: {maxGer}  ·  população final: {sim.Pop.Count}");
    Console.WriteLine($"Exemplo — criatura #{c.Id}: idade={c.Age} energia={c.Energy:0.00} geração={c.Genome.Geracao}");
    Console.WriteLine($"  tamanho={c.Genome.Get("corpo.tamanho"):0.00}  agressiv={c.Genome.Get("comp.agressividade"):0.00}  curiosid={c.Genome.Get("comp.curiosidade"):0.00}  social={c.Genome.Get("comp.sociabilidade"):0.00}");
}

// ---------------------------------------------------------------------------
Title("9) E05 — Comportamento coletivo (emergente)");
Console.WriteLine($"Grupos detectados: {sim.Pop.Social.GroupCount}   ·   Figuras: {sim.Pop.Social.FigureCount}   ·   laços sociais: {sim.Pop.Social.Rels.Count}");
var papeis = sim.Pop.Creatures.GroupBy(z => z.Role()).OrderByDescending(gp => gp.Count());
Console.WriteLine("Papéis: " + string.Join("  ", papeis.Select(gp => $"{gp.Key}={gp.Count()}")));
var topDom = sim.Pop.Creatures.OrderByDescending(z => z.DominanceWins).FirstOrDefault();
if (topDom != null) Console.WriteLine($"Mais dominante: #{topDom.Id} (vitórias={topDom.DominanceWins}, agressiv={topDom.Genome.Get("comp.agressividade"):0.00}, tamanho={topDom.Genome.Get("corpo.tamanho"):0.00})");
foreach (var fig in sim.Pop.Creatures.Where(z => z.IsFigure).Take(3))
    Console.WriteLine($"★ Figura #{fig.Id}: prestígio={fig.Prestige:0.0} liderança={fig.Genome.Get("comp.lideranca"):0.00} grupo={fig.GroupId}");
Console.WriteLine("(grupos = clusters de afinidade detectados, não criados; hierarquia = vitórias de dominância)");

// ---------------------------------------------------------------------------
Title("10) E06 — Mutação causal, especiação e LOD");
var linhagens = Genese.Core.Speciation.ContarLinhagens(sim.Pop.Creatures);
Console.WriteLine($"Linhagens vivas: {linhagens}   ·   cisões de linhagem: {sim.Pop.Spec.EspeciacaoCount}   ·   LOD: {sim.LOD}");
// Distância genômica média entre pares de indivíduos vivos (amostra até 10 pares)
var vivos = sim.Pop.Creatures.Where(c => c.Alive).ToList();
double distMedia = 0; int pares = 0;
for (int ii = 0; ii < vivos.Count && pares < 10; ii++)
    for (int jj = ii + 1; jj < vivos.Count && pares < 10; jj++)
    { distMedia += Genese.Core.Genome.Distance(vivos[ii].Genome, vivos[jj].Genome); pares++; }
if (pares > 0) distMedia /= pares;
Console.WriteLine($"Distância genômica média (amostra {pares} pares): {distMedia:0.000}   " +
                  $"[limiar viável={Genese.Core.Speciation.LimiarViavel:0.00}  fértil={Genese.Core.Speciation.LimiarFertil:0.00}]");
// Pressão mutagênica média das células habitadas
double pressMedia = 0; int habCells = 0;
foreach (var c in vivos) { int ix = System.Math.Clamp((int)c.X, 0, sim.Env.W-1), iy = System.Math.Clamp((int)c.Y, 0, sim.Env.H-1); pressMedia += sim.Env.PressaoMutagenica(ix, iy); habCells++; }
if (habCells > 0) pressMedia /= habCells;
Console.WriteLine($"Pressão mutagênica média habitat: {pressMedia:0.00}   ·   " +
                  $"fatorPop (N={sim.Pop.Count}): {Genese.Core.Speciation.FatorTamanhoPop(sim.Pop.Count):0.00}");
Console.WriteLine("(especiação emerge: dist genômica > limiar fértil → cisão de linhagem irreversível)");

// ---------------------------------------------------------------------------
Title("11) E07 — Camada Simbólica: língua, cultura e religião emergentes");
Console.WriteLine($"Língua: estágio={sim.Pop.Language.Stage}  ·  fonemas={sim.Pop.Language.Phonemes.Length}  ·  léxico={sim.Pop.Language.Lexicon.Count} palavras  ·  deriva={sim.Pop.Language.DriftCount}");
if (sim.Pop.Language.Lexicon.Count > 0)
{
    Console.Write("  Palavras: ");
    int shown = 0;
    foreach (var kv in sim.Pop.Language.Lexicon) { Console.Write($"{kv.Key}='{kv.Value}'  "); if (++shown >= 8) break; }
    Console.WriteLine();
}
Console.WriteLine($"Cultura: memes={sim.Pop.Culture.SymbolCount}  ·  coesão={sim.Pop.Culture.CulturalCohesion:0.00}");
foreach (var kv in sim.Pop.Culture.Pool)
    Console.WriteLine($"  [{kv.Value.Type}] força={kv.Value.Force:0.00}  prevalência={kv.Value.Prevalence:0.00}  rigidez={kv.Value.Rigidity:0.00}  origem=#{kv.Value.OriginId}");
if (sim.Pop.Culture.Pool.Count == 0) Console.WriteLine("  (nenhum meme activo — figuras ainda não emergiram ou pool vazio)");
Console.WriteLine($"Religião: estágio={sim.Pop.Belief.Stage}  ·  fervor={sim.Pop.Belief.Fervor:0.00}  ·  dogmatismo={sim.Pop.Belief.Dogmatism:0.00}  ·  organização={sim.Pop.Belief.Organization:0.00}");
Console.WriteLine($"  Imagem do jogador: {sim.Pop.Belief.Image}  (baseada no histórico de nudges)");
Console.WriteLine("(língua: invntário fonêmico único por civilização; cultura: memes de Figuras; religião: fervor→Atenção)");

// ---------------------------------------------------------------------------
Title("12) E08 — Múltiplas Civilizações (M11) e Eventos Causais (M14)");
Console.WriteLine($"Civilizações: {sim.Civs.Count}");
for (int ci = 0; ci < sim.Civs.Count; ci++)
{
    var civ = sim.Civs[ci];
    Console.WriteLine($"  Civ {ci}: pop={civ.Pop.Count} · língua={civ.Pop.Language.Stage} · " +
                      $"religião={civ.Pop.Belief.Stage} · memes={civ.Pop.Culture.SymbolCount}");
    if (civ.Relations.Count > 0)
    {
        foreach (var kv in civ.Relations)
        {
            var r = kv.Value;
            Console.WriteLine($"    ↔ Civ {kv.Key}: stance={r.Stance} trust={r.Trust:0.00} resentimento={r.Resentment:0.00} " +
                              $"comércio={r.TradeCount} guerras={r.WarCount}");
        }
    }
    else Console.WriteLine("    (sem contatos registados)");
}
Console.WriteLine($"Eventos totais (Crônica): {sim.Events.TotalEvents}");
foreach (var ev in sim.Events.Log.GetRange(0, Math.Min(5, sim.Events.Log.Count)))
    Console.WriteLine($"  tick {ev.Tick,6}: [{ev.Type}] {ev.Resolution}");
if (sim.Events.Active.Count > 0)
{
    Console.WriteLine($"Eventos activos ({sim.Events.Active.Count}):");
    foreach (var ev in sim.Events.Active)
        Console.WriteLine($"  [{ev.Type}] civ {ev.CivId} desde tick {ev.Tick}");
}
Console.WriteLine("(civilizações correm as mesmas regras M01-M10 — sem IA roteirizada; interação por estado)");

// ---------------------------------------------------------------------------
Title("13) E09 — Influência (M12), Observação/Crônica (M13), PopStats (M02), Destinos (M14)");
// PopStats M02
Genese.Core.PopStats.Compute(sim.Pop.Creatures, out float[] means, out float[] stds);
Console.WriteLine($"Macro-estatísticas coletivas (civ 0):");
Console.WriteLine($"  militarismo   {Bar(Genese.Core.PopStats.Militarismo(means))}");
Console.WriteLine($"  coesão social {Bar(Genese.Core.PopStats.CoesaoSocial(means))}");
Console.WriteLine($"  inovação      {Bar(Genese.Core.PopStats.Inovacao(means))}");
Console.WriteLine($"  alerta homogeneidade: {(Genese.Core.PopStats.HomogeneityAlert(stds) ? "⚠ SIM" : "não")}");
// InfluenceSystem M12
var infl = sim.Influence;
Console.WriteLine($"Atenção: {infl.Attention:0}/{infl.MaxAttention(sim.Pop.Belief.Fervor):0}  total nudges: {infl.TotalNudges}");
Console.WriteLine($"Custos por nudge: Sinal={Genese.Core.InfluenceSystem.Cost[0]} Faísca={Genese.Core.InfluenceSystem.Cost[1]} Inspiração={Genese.Core.InfluenceSystem.Cost[2]} Proteção={Genese.Core.InfluenceSystem.Cost[3]} Pressão={Genese.Core.InfluenceSystem.Cost[4]}");
// Crônica M13
Console.WriteLine($"Crônica: {sim.Chronicle.Count} entradas");
foreach (var e in sim.Chronicle.Entries.GetRange(0, System.Math.Min(4, sim.Chronicle.Count)))
    Console.WriteLine($"  {e.Text}");
if (sim.Chronicle.Count > 4) Console.WriteLine($"  … + {sim.Chronicle.Count - 4} mais");
// Destino M14
Console.WriteLine($"Destino atual: {sim.Destiny}");
Console.WriteLine("(destinos: Continuidade, Transcendência, Extinção, Fusão, Prosperidade, Estagnação, Divergência)");

Console.WriteLine();
Console.WriteLine("Fim. Edite os parâmetros (seed/gerações/pressão) e rode de novo para comparar.");
Console.WriteLine("Onde mexer: InfluenceSystem/Chronicle (M12/M13) · PopStats (M02) · EventSystem (M14).");

// ============================ helpers ============================
static void Title(string t)
{
    Console.WriteLine();
    Console.WriteLine("══ " + t + " " + new string('═', Math.Max(0, 64 - t.Length)));
}
static string YesNo(bool b) => b ? "SIM ✔" : "NÃO ✘";
static string Bar(float v)
{
    int n = (int)Math.Round(Math.Clamp(v, 0, 1) * 16);
    return ("[" + new string('#', n).PadRight(16, '·') + $"] {v:0.00}");
}
static void PrintGenome(Genome g)
{
    foreach (Bloco bloco in Enum.GetValues(typeof(Bloco)))
    {
        Console.WriteLine($"  · {bloco}:");
        for (int i = 0; i < GeneRegistry.Count; i++)
        {
            var d = GeneRegistry.Def(i);
            if (d.Bloco != bloco) continue;
            Console.WriteLine($"      {d.Id,-20} {Bar(g.Values[i])}");
        }
    }
}
static void Histogram(float[] vals)
{
    int[] bins = new int[10];
    foreach (var v in vals) bins[Math.Min(9, (int)(Math.Clamp(v, 0, 0.999f) * 10))]++;
    for (int i = 0; i < 10; i++)
        Console.WriteLine($"  {i / 10.0:0.0}-{(i + 1) / 10.0:0.0} {new string('█', bins[i])} ({bins[i]})");
}
static char BiomeChar(Biome b) => b switch
{
    Biome.Oceano => '~', Biome.Pradaria => '.', Biome.Floresta => 'T', Biome.Deserto => ':',
    Biome.Tundra => '*', Biome.Montanha => '^', Biome.Pantano => '%', Biome.Vulcanico => 'V', _ => '?'
};
static bool Independentes(ulong seed)
{
    var r1 = new Rng(seed); var m1 = r1.Fork(Streams.Mutation);
    var seqA = Enumerable.Range(0, 5).Select(_ => m1.NextULong()).ToArray();
    var r2 = new Rng(seed); var d2 = r2.Fork(Streams.Decision);
    for (int i = 0; i < 30; i++) d2.NextULong();
    var m2 = r2.Fork(Streams.Mutation);
    var seqB = Enumerable.Range(0, 5).Select(_ => m2.NextULong()).ToArray();
    return seqA.SequenceEqual(seqB);
}
