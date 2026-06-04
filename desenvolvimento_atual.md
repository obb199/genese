# Gênese — Desenvolvimento Atual

> Documento vivo do estado de construção do jogo. Atualizado a cada etapa.
> Última atualização: **2026-06-03**

---

## 🎯 Decisão de projeto

- **Jogo 3D**, motor **Unity + C#** (ver [DIRECAO_3D.md](DIRECAO_3D.md)).
- **Arquitetura do roadmap (E00 §2.1):** o **núcleo de simulação é uma biblioteca C# pura, SEM Unity**,
  determinística e testável; a **Unity é só apresentação 3D + entrada**.

```
Genese.sln
├─ Genese.Core/        ← C# puro (.NET): toda a lógica/determinismo (M01–M14). SEM Unity.
├─ Genese.Core.Tests/  ← xUnit: testa o núcleo isolado (sem abrir a engine).
└─ unity/Genese/       ← Genese.Game: apresentação 3D (consome o Core). É o protótipo atual.
```

> **Regra inegociável:** nada de lógica de simulação dentro de MonoBehaviour. A engine desenha e captura
> input; o Core decide. Toda aleatoriedade passa pelo RNG central semeado (GDD §1.5).

---

## ✅ Etapa 0 — Protótipo / Termômetro 3D (CONCLUÍDA — base de apresentação reaproveitada)

Projeto Unity em [unity/Genese/](unity/Genese/) — um **diorama vivo 3D 100% procedural** que valida a
direção de arte e o loop *observar → influenciar*. **Tudo isto é reaproveitado como a camada
`Genese.Game`** (renderização); a lógica “de verdade” migra para o `Genese.Core` nas próximas etapas.

**O que já existe (procedural, gerado por código):**
- **Criaturas paramétricas 3D** (`CreatureBuilder`): ~18 eixos fiéis ao Claude Design (forma, padrão,
  olhos, pupila, boca, orelhas, cauda, pernas, braços, antenas/chifres/crista/crina, acabamento,
  emissão, ornamento, cores+mistura, sinal), com **squash & stretch**.
- **Espécie por bioma**: cada bioma gera uma **espécie-base** (cor/traços típicos) e os indivíduos são
  **variações** dela — parecidos, mas únicos (`Genome.Species` + `Genome.Vary`).
- **Terreno procedural** (`WorldBuilder`): malha deformada por **Perlin** com **colinas, montanhas e rio
  (bônus ~40%)** + **plano de água** (rios/lagos; lava no vulcânico); centro plano para a aldeia.
- **8 biomas** com geração própria (árvores: folhosa/conífera/cacto/morta; rochas arredondadas; **props**:
  flores, arbustos, cogumelos, troncos, juncos, vitórias-régias, ossos, cristais de gelo, rochas de lava…).
- **Marcos por bioma**: **vulcão** (cratera de lava + luz), **pirâmide**, **iceberg**, **árvore gigante**,
  **arco de rocha**, **pico nevado**, **salgueiro gigante**.
- **Aldeia + cultura**: fogueira + **pedra-coração**, casas grandes variadas (1–2 andares, **5 telhados**:
  pirâmide/hip/laje-parapeito/duas-águas/cúpula, chaminé, janelas, **bandeira**), totem, e **monumento
  cultural** por cultura (zigurate, torre, cristal, obelisco, antena, totens). 6 culturas recoloríveis.
- **Iluminação + ciclo dia/noite**, **câmera orbital com inclinação até quase topo**, **sombras**.
- **Colisão** por círculo proporcional ao tamanho (criaturas × criaturas e × objetos) e **desvio de água**.
- **Nudges com efeitos** (Sinal/Faísca/Inspiração) — raio, anel, partículas e clarão suaves.

> Estado: é uma **vitrine de apresentação**. O movimento/“mutação” aqui são placeholders visuais — a
> simulação real (genoma herdável, decisão por traços, emergência) vem do `Genese.Core`.

---

## ✅ E00 — Fundação (CONCLUÍDA)

- Solução **`Genese.sln`** com separação **núcleo/engine** (Core puro + Tests + Game/Unity).
- **`Genese.Core`** mira `netstandard2.1` (compatível com Unity) e **não referencia a Unity**.
- Regras de **determinismo** adotadas (RNG central; sem `DateTime.Now`/`System.Random` na lógica).
- `.gitignore` para artefatos .NET e Unity.

## ✅ E01 — Núcleo de Simulação (esqueleto) (CONCLUÍDA — 7/7 testes verdes)

Em [Genese.Core/](Genese.Core/):
- **`Rng.cs`** — RNG determinístico (xoshiro256** semeado por SplitMix64) com **sub-streams** via
  `Fork(streamId)` derivados da semente (independentes entre subsistemas). `Streams` lista os IDs.
- **`WorldState.cs`** — estado serializável (por ora: `Tick`, `Seed`, `EnvAccum`).
- **`ISimulation.cs`** / **`Simulation.cs`** — loop de tempo em **ticks inteiros**, `Step()`
  determinístico e **`Snapshot()`/`Restore()`** exatos (base de saves, replays e do GIF de evolução).

**Testes (xUnit, rodam sem Unity):**
`Replay determinístico` · `replay com sementes diferentes diverge` · `snapshot round-trip exato`
· `sub-streams independentes` · `streams diferentes divergem` · `NextDouble∈[0,1)` · `Range respeita limites`.

```bash
# rodar os testes do núcleo (precisa do dotnet-sdk-8.0):
cd /home/user/Desktop/apps/genese
dotnet test Genese.Core.Tests/Genese.Core.Tests.csproj
```

---

## ✅ E02 — Genoma e Hereditariedade (CONCLUÍDA — 6 testes; 13/13 no total)

Implementa M01 no `Genese.Core`:
- **`Genes.cs`** — registro mestre (`GeneRegistry`): 24 genes em 3 blocos (genético, comportamental,
  regulatório), cada um com modo de herança, taxa de mutação base, plasticidade e faixa. Índice estável
  (determinismo). Genoma **compacto** (struct-of-arrays: só `float[]` por criatura; metadados ficam no registro).
- **`Genome.cs`** — `float[] Values` + `Geração`/`LinhagemId`; `Founder(rng)`, `Clone`, `Get/Set`,
  **`Distance(a,b)`** (handoff para especiação M03/E06) e serialização.
- **`Reproduction.cs`** — os **4 modos de herança** (média/dominante/recessivo/poligênico) + **mutação
  causal**: probabilidade = `taxaBase × fatorMutação × pressão ambiental`, delta sempre do
  **sub-stream de mutação**. Sexuada e assexuada (mãe==pai).

**Testes (E02):** herança exata sem mutação · recessivo só expressa se ambos portam · reprodução
determinística · genes ficam na faixa após 2000 gerações · frequência de mutação ≈ taxa · distância
genética cresce com isolamento.

## ✅ E03 — Ambiente e Recursos (CONCLUÍDA — 6 testes; 19/19 no total)

Implementa M06/M07 no `Genese.Core` (`Environment.cs`):
- **Mundo em grade** (SoA: arrays planos de altitude, temperatura, umidade, comida, água, material,
  tensão geológica, balanço hídrico, bioma). Geração determinística por **ruído de valor fractal**.
- **Bioma derivado** de altitude+temperatura+umidade (Oceano/Pradaria/Floresta/Deserto/Tundra/Montanha/Pântano) — nunca pintado.
- **Estações** (ciclo determinístico) modulam temperatura/precipitação; **ruído climático de amplitude fixa**.
- **Recursos finitos com regeneração** ligada a clima/estação; `Harvest` reduz, regeneração repõe (escassez real).
- **Seca emergente** do **balanço hídrico** acumulado (precip − evap), não sorteada — concentra-se nos desertos.
- **Tensão geológica → ruptura por limiar** (vulcanismo): soergue o terreno e pode criar **montanhas/barreiras**.
- **Erosão** leve (relevo muda devagar) e **conectividade** (BFS): uma cordilheira **isola regiões** → handoff de especiação (M03/E06).
- Integrado à `Simulation` (Step atualiza o ambiente) e ao **snapshot/restore** exato.

**Testes (E03):** ambiente determinístico · bioma derivado do clima · geração com biomas variados · seca
emerge do balanço (reproduzível) · recurso regenera após colheita · cordilheira isola regiões.

## ✅ E04 — Agentes (decisão individual) (CONCLUÍDA — 5 testes; 24/24 no total)

Implementa M04 no `Genese.Core`:
- **`Creature.cs`** — indivíduo: genoma + posição na grade + energia (fome) + idade. Fenótipo derivado.
- **`Population.cs`** — **IA de utilidade**: cada agente pontua ações (forragear, ir até comida, explorar,
  descansar, reproduzir) por necessidades+traços+ambiente e escolhe via **softmax** (temperatura = flexibilidade/
  curiosidade). Forragear **consome o recurso** da célula (M07); energia cai pelo metabolismo; **reprodução**
  usa M01 (com mutação causal) e **conserva energia** (sem criar do nada). Integrado à `Simulation` (Step →
  ambiente + agentes) e ao snapshot (RNGs de decisão/mutação **persistentes** e serializados).
- **Testes (E04):** população determinística · snapshot round-trip com população · **abundância faz crescer**
  · **escassez extingue** · criaturas **nunca entram em água/montanha**.

## 🎮 INTEGRAÇÃO NA UNITY — o núcleo rodando e desenhado (NOVO)

Agora o protótipo Unity **roda o núcleo de verdade** e o desenha. A DLL do núcleo está em
`unity/Genese/Assets/Plugins/Genese.Core.dll`; os scripts da ponte em `Assets/Genese/Scripts/Nucleo/`:
- **`CoreSim`** roda a `Simulation` em ticks (velocidade/pausa).
- **`CoreWorldView`** desenha a grade do **ambiente E03** (células coloridas por bioma, altura = relevo, água).
- **`CoreCreatureView`** desenha os **agentes E04** com o `CreatureBuilder` (genoma→aparência via `GenomeMap`),
  movendo-os conforme o núcleo; nascem e morrem na tela.
- **`CoreHud`** — clique numa **célula** → bioma/altitude/**temperatura/umidade/comida/água/material/seca**;
  clique numa **criatura** → **genoma (traços) + energia/idade/geração/linhagem**. Controle de tempo.

**Como ver/testar na Unity:** abra `unity/Genese` → menu **Gênese ▸ Criar cena Mundo (Núcleo)** → **▶ Play**.
(Há também a cena antiga **Mundo Vivo**, que é só a vitrine de arte com movimento placeholder — a nova
**Mundo (Núcleo)** é a que roda a simulação real.)

> ⚠️ Os scripts da Unity **não foram compilados aqui** (sem Editor no ambiente). Se houver erro no Console,
> me mande que eu corrijo. Performance: render limitado a `maxRender` criaturas (padrão 160).

## ✅ E05 — Comportamento coletivo (emergente) (CONCLUÍDA — 5 testes; 29/29 no total)

Implementa M05 no `Genese.Core` — estruturas coletivas **emergem** das interações, sem hardcode de "grupo":
- **`Social.cs`** (novo) — relações **esparsas** entre pares próximos: `Rel{Afinidade, Dominância, Dívida}`.
  - **Interação** (raio social): afinidade cresce entre quem se aproxima (puxado pelo traço `sociabilidade`/
    `cooperação`); **dominância** decidida por atributos (`agressividade`+tamanho+prestígio+vitórias) — o
    vencedor ganha **prestígio**; **altruístas** dividem energia (M05 — cooperação tem custo/benefício).
  - **Grupos** = **clustering por união-find** sobre laços de afinidade ≥ limiar (detectados, não criados);
    chaves ordenadas p/ **determinismo**.
  - **Hierarquia** emerge da comparação de atributos (mais vitórias ⇒ mais prestígio).
  - **Figuras** emergem por **prestígio + liderança** acima do limiar (não por nomeação aleatória).
- **`Creature.cs`** ganha `Prestige, GroupId, DominanceWins, IsFigure` e `Role()` (Líder/Parental/Explorador/
  Forrageiro, derivado do comportamento). **`Population.Step`** chama o social em cadência (interação a cada
  4 ticks; decay a cada 20; detecção de grupos/Figuras a cada 25). Tudo serializado no snapshot (versão 4).
- **Testes (E05):** sociáveis **formam grupos** · solitários formam **menos laços** · atributo de dominância
  ⇒ **mais vitórias** (hierarquia) · **Figuras** só acima do limiar de prestígio/liderança · **determinismo**
  (mesma semente ⇒ mesma estrutura social + snapshot idêntico).
- **Na Unity:** o HUD da criatura agora mostra **★ FIGURA**, **papel**, **grupo**, **prestígio** e
  **dominância**; o status mostra **grupos · figuras**. O Lab ganhou a **seção 9** (grupos/figuras/papéis).

## ✅ E06 — Mutação causal & Especiação (CONCLUÍDA — 5 testes; 34/34 no total)

Implementa M03 no `Genese.Core` — o motor evolutivo completo:
- **`Speciation.cs`** (novo) — três sistemas causais, sem acaso arbitrário:
  1. **Taxa de mutação causal** (`FatorTamanhoPop`): populações pequenas mutam/fixam mais rápido
     (variância 1/N); escala 0.7 (grande) → 2.2 (mínima). Integrado como `mutationScale` em
     `Reproduction.Reproduce`.
  2. **Pressão mutagênica ambiental** (`Environment.PressaoMutagenica`): vulcão/seca/extremos
     térmicos elevam a taxa efectiva; integrada em `Population.Act` (reproduce).
  3. **Isolamento reprodutivo emergente**: `Speciation.PodeReproduzir(a, b)` bloqueia cruzamento
     quando `Genome.Distance ≥ LimiarFertil(0.48)`. Zona cinzenta [0.28, 0.48]: híbridos raros
     com nova `LinhagemId`. Espécies distintas: separação permanente.
- **Reprodução sexual** adicionada: quando parceiro próximo e compatível está a ≤3 células,
  `Population.Act` usa-o como segundo progenitor (antes era sempre assexuada). Deriva + seleção
  divergente → especiação emergente ao longo de gerações.
- **LOD de simulação** (`SimLOD.Pleno/Agregado/Dormente`): `Simulation.SetLOD()` permite escalar
  a profundidade de simulação por região (base da performance mobile, E08).
- **SnapshotVersion = 5** — `_nextLinId` + `Speciation.EspeciacaoCount` serializados.
- **Testes (E06):** `FatorTamanhoPop` decrescente com N · `PressaoMutagenica` maior no vulcânico/seca
  · genomas opostos bloqueados · alta pressão → descendência mais divergente · determinismo (mesma
  semente ⇒ mesmo padrão de especiação + snapshot idêntico).

## ✅ E07 — Camada Simbólica (CONCLUÍDA — 6 testes; 40/40 no total)

Implementa M08/M09/M10 no `Genese.Core` — a civilização ganha voz, valores e fé emergentes:
- **`Language.cs`** (M08): inventário fonêmico único por população (7-10 fonemas), léxico procedural
  (pares significado→forma gerados a partir do inventário), **5 estágios causais** desbloqueados por
  pré-condições de estado (Gestual→Vocal→Proto→Gramática→Escrita — nunca por tempo ou sorteio),
  **deriva fonológica** causal (palavras pouco usadas derivam com o `DriftCount`), distância
  linguística entre populações (fonêmica + lexical — espelha a distância genômica de M03).
- **`Culture.cs`** (M09): pool de **memes** (`Valor/Tabu/Mito/Arte/Rito`) com força, rigidez e
  prevalência; nascem de **Figuras** ou eventos (nunca espontâneos); **propagação por imitação**
  proporcional ao prestígio das Figuras; competição por atenção (soma por tipo ≤ 1.5); **lente
  interpretativa** = argmax de força × prevalência (nunca sorteio); **coesão cultural** mede
  concentração das prevalências; cisma implícito em coesão baixa.
- **`Belief.cs`** (M10): 5 dimensões (fervor, dogmatismo, organização, estágio, imagem do jogador);
  **4 estágios causais** (Animismo→Politeísmo→Monoteísmo→Transcendente); **imagem do jogador**
  derivada do histórico circular de 20 nudges (Benevolente/Hostil/Trickster/Adormecido/Neutro);
  **fervor realimenta Atenção** do jogador em +30% do bônus (M12).
- **Integração na `Population`**: Language/Culture/Belief inicializados em `Seed()` com sub-stream
  `Streams.Symbol = 8`; passo da língua a cada 30 ticks, propagação cultural a cada 20, crença a cada
  40; **Figuras criam memes de Valor/Rito** a cada 50 ticks quando presentes; nudges registram
  resultado na imagem do jogador via `Belief.RecordNudge()`.
- **`SnapshotVersion = 6`** — Language + Culture + Belief totalmente serializados.
- **+6 testes → 40/40 verdes** (`SymbolicTests.cs`): léxico cresce, estágio avança por pré-condição,
  meme/coesão em faixa válida, imagem Benevolente após nudges bons, imagem Hostil após nudges maus,
  determinismo + snapshot round-trip da camada simbólica.
- **Lab** ganhou **seção 11** (inventário fonêmico, léxico, memes ativos, fervor, imagem do jogador).
- **DLL** reconstruída e copiada para `unity/Genese/Assets/Plugins/Genese.Core.dll`.

**Extras Unity (na mesma entrega):**
- **Mapa dual-bioma**: `Environment.Generate` agora aceita `tempBias2/umidBias2`; `CoreSim` expõe
  `ClimateIndex2/ClimateName2/CycleClimate2()` — a metade esquerda e direita do mapa têm climas
  independentes com transição suave em 20% central (smoothstep).
- **Ciclo dia/noite contínuo**: `CoreDayNight` refatorado com **8 keyframes** em fase [0,1) e
  interpolação SmoothStep entre eles — zero descontinuidades em qualquer transição de fase.
- **HUD atualizado**: linha de status por língua (`Stage/léxico/deriva`), cultura (`memes/coesão`) e
  religião (`Stage/fervor/imagem`); controle `Clima N:` + `Clima S:` separados; inspeção de célula
  mostra fervor/coesão/organização e meme dominante; Atenção máxima sobe com o fervor (M12).

## ✅ E08 — Mundo & Eventos (CONCLUÍDA — 7 testes; 47/47 no total)

Implementa M11/M14 no `Genese.Core` — múltiplas civilizações com identidade própria, rodando as
**mesmas regras M01-M10** (sem IA roteirizada), mais eventos causais detonados por limiares:

- **`CivRelation.cs`** (M11 §4.3): `CivStance` (Desconhecida → PrimeiroContato → Cautelosa →
  Comercial → Aliada → Guerra → Vassalagem), `Trust`, `Resentment`, `TradeCount`/`WarCount` —
  evolui pelo histórico, nunca por evento aleatório.
- **`Civilization.cs`** (M11 §2): encapsula `Population` + território de origem + `Relations` com
  outras civs. `ContactAffinity(a, b)` = parentesco genético (M03) × inteligibilidade linguística
  (M08) × compatibilidade cultural (M09/M10) — sem tabela fixa.
- **`ContactSystem.cs`** (M11 §4.1-4.2): detecta pares de criaturas dentro de `ContactRadius=5`;
  escolhe ação por **argmax de scores** (guerra/comércio/troca cultural, calculados de traços +
  recursos + histórico); `DoTrade`/`DoConflict`/`DoCultureExchange`/`DoFusionStep`. `UpdateStance`
  deriva a stance do histórico de confiança/resentimento.
- **`EventSystem.cs`** (M14): situações latentes ativam por **limiar de estado** (nunca por tick/chance);
  resolvem pelo argmax de compatibilidade estado/cultura — **mesma seca, resolução diferente** em
  culturas com imagens de jogador opostas. 7 tipos: Seca, Fome, ColapsoPop, GuerraDeclarada,
  Transcendência, Expansão, Fusão. Log rastreável (base da Crônica, M13).
- **`Simulation.cs`** refatorado: `List<Civilization> Civs` (padrão 2); `Pop` é atalho para
  `Civs[0].Pop` (backward compat). Cada civ tem RNGs independentes `_civDec[i]`/`_civMut[i]`
  (offsets 0x100×i). `ContactSystem.CheckAndInteract` corre a cada 10 ticks; `EventSystem.Step`
  a cada 30 ticks. `SnapshotVersion = 7`.
- **Bug de determinismo corrigido**: `Language.Step` usava `Lexicon.Keys` sem sort para seleção
  de derive — Dictionary muda ordem interna após restore → sort explícito garante bit-a-bit idêntico.
  Mesmo padrão aplicado a `Culture.Propagate` (defensivo).
- **+7 testes → 47/47 verdes** (`CivTests.cs`): civs em metades separadas, backward compat Pop,
  contato detectado por proximidade, interação produz trust/comércio/guerra, seca ativa por limiar,
  mesmo gatilho→resolução diferente por cultura, determinismo + snapshot round-trip multi-civ.
- **Lab** ganhou **seção 12** (civs, relações, eventos, Crônica).
- **DLL** recompilada e copiada.

**Unity (E08):**
- **CoreCreatureView**: itera todas as civs; criaturas da Civ 1 têm tint laranja, Civ 2 roxo, etc.
- **CoreHud**: painel esquerdo com lista de civs, relações (stance/trust/resentimento), eventos
  activos e últimos 3 eventos resolvidos (Crônica).

## ▶️ Próxima etapa — E09: Influência & Apresentação (M12/M13)

Atenção aprofundada, nudges com consequências culturais/religiosas, Crônica visual, observação.
Roadmap em [ROADMAP.md](ROADMAP.md).

---

## ✅ Checklists de teste

### A) ★ Núcleo NA UNITY (simulação real) — `unity/Genese` → menu **Gênese ▸ Criar cena Mundo (Núcleo)** → ▶
- [ ] Cena abre sem erros no Console; aparece a **grade do mundo** (biomas em cores, relevo) + **água**.
- [ ] **Criaturas** surgem, **se movem**, **nascem e morrem** (a população muda com o tempo).
- [ ] Clique numa **célula** → painel com **bioma, altitude, temperatura, umidade, comida, água, material, seca**.
- [ ] Clique numa **criatura** → painel com **genoma (traços), energia, idade, geração, linhagem** +
  **papel, grupo, prestígio, dominância** e marca **★ FIGURA**; o status mostra **grupos · figuras**.
- [ ] **Pausar/velocidade** funcionam; a câmera gira/zoom/inclina.

### B) Vitrine de arte (protótipo, movimento placeholder) — menu **Gênese ▸ Criar cena Mundo Vivo** → ▶
- [ ] Trocar **bioma/cultura** regenera tudo; achar **marco** (vulcão…) e **rio** (~40%); **nudges**/dia-noite.

### C) Núcleo C# (lógica) — sem Unity
- [ ] `dotnet test Genese.Core.Tests/Genese.Core.Tests.csproj` → **40 passed**.
- [ ] `dotnet run --project Genese.Lab -- 12345 300 0.4` → laboratório imprime genoma, herança, deriva, mapa de biomas, **agentes vivos**, **(seção 9) comportamento coletivo**, **(seção 10) especiação** e **(seção 11) camada simbólica** (língua/cultura/religião).

> **Guia completo para testar tudo e propor melhorias:** [GUIA_DE_TESTES.md](GUIA_DE_TESTES.md).

---

## Changelog
- **2026-06-03 (E05)** — **Comportamento coletivo emergente (M05)**: novo `Social.cs` (relações esparsas,
  grupos por união-find sobre afinidade, hierarquia por atributos, Figuras por prestígio/liderança);
  `Creature` ganhou prestígio/grupo/dominância/Figura/`Role()`; `Population.Step` roda o social em cadência;
  snapshot v4. **+5 testes → 29/29 verdes.** DLL reconstruída e copiada para a Unity; HUD mostra
  grupo/papel/prestígio/Figura e status grupos·figuras; Lab ganhou seção 9. Próxima: **E06 Mutação & Especiação**.
- **2026-06-03** — Documentado o protótipo (reaproveitado como `Genese.Game`). Iniciado o jogo pelo
  roadmap: **E00** (solução Core/Game, determinismo), **E01** (núcleo: RNG causal, tick, snapshot) e
  **E02** (genoma M01), **E03** (ambiente M06/M07) e **E04** (agentes/decisão M04, IA de utilidade).
  **24 testes xUnit verdes.** **Integração na Unity:** DLL do núcleo + cena **Mundo (Núcleo)** que roda e
  desenha o mundo + criaturas, com HUD de inspeção (célula e genoma).
