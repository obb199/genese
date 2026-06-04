# Gênese — Revisão Geral Final (2026-06-04)

Auditoria sistemática de cada documento de mecânica (M01–M14) contra o que foi
implementado. Formato: ✅ implementado · ⚠️ parcial · ❌ ausente.

---

## M01 — Genoma e Hereditariedade

| Checklist M01 | Estado |
|---|---|
| 24 genes em 3 blocos (Genético/Comportamental/Regulatório) | ✅ GeneRegistry 24 genes |
| 4 modos de herança (média/dominante/recessivo/poligênico) | ✅ Reproduction.Combine |
| Mutação causal: prob = taxaBase × fatorReg × pressao × fatorPop | ✅ Reproduction.Reproduce |
| Genoma compacto (struct-of-arrays) | ✅ Genome.Values float[] |
| Distance(a,b) para especiação | ✅ Genome.Distance |
| Serialização (Snapshot/Restore) | ✅ Genome.Write/Read |

**Nota**: M01 fala em "3–8 sub-genes por traço poligênico" (plasticidade M02 §4.2).
Implementado como 4 sub-mutações independentes no loop de Herança.Poligenico ✅

---

## M02 — Traços Comportamentais

| Checklist M02 | Estado |
|---|---|
| 14 traços comportamentais, normalizados [0,1] | ✅ (especificação diz "23" total com genéticos) |
| Média e desvio-padrão de cada gene sobre a população | ✅ PopStats.Compute |
| Macro-estatísticas (militarismo, coesão, inovação) | ✅ PopStats.Militarismo/CoesaoSocial/Inovacao |
| Alerta de homogeneidade (stddev colapsa) | ✅ PopStats.HomogeneityAlert |
| nomadismo, medoCoragem, vigilancia, armazenamento, invParental usados em M04 | ✅ E09 fix |
| Plasticidade fenotípica (fenótipo ≠ genótipo em vida) | ⚠️ Só via trait wiring; sem Phenotype separado |

**Nota**: A plasticidade completa (fenótipo muda com experiência sem alterar genótipo) não foi
implementada como campo separado em Creature — seria um passo adicional. A influência comportamental
está presente nos pesos de utilidade que dependem de traços, mas o genótipo e o "fenótipo"
são o mesmo valor por agora.

---

## M03 — Mutação Causal e Especiação

| Checklist M03 | Estado |
|---|---|
| FatorTamanhoPop (deriva amplificada em populações pequenas) | ✅ Speciation.FatorTamanhoPop |
| PressaoMutagenica por ambiente (vulcão/seca/extremos) | ✅ Environment.PressaoMutagenica |
| PodeReproduzir (isolamento reprodutivo por Genome.Distance) | ✅ Speciation.PodeReproduzir |
| Zona cinzenta: híbridos raros com nova LinhagemId | ✅ Speciation.LinhaDaDescendencia |
| LOD (Pleno/Agregado/Dormente) | ✅ SimLOD + Simulation.SetLOD |
| ContarLinhagens (proxy de espécies) | ✅ Speciation.ContarLinhagens |

---

## M04 — Decisão Individual

| Checklist M04 | Estado |
|---|---|
| IA de utilidade: pontuação de 6 ações | ✅ Population.Act (6 ações) |
| Softmax com temperatura = flexibilidade/curiosidade | ✅ SoftmaxPick |
| Forragear consome recurso real da célula | ✅ env.Harvest |
| Metabolismo cai com o tempo | ✅ c.Energy -= 0.003 + metabolismo |
| Todos os traços comportamentais influenciam ações | ✅ E09: 5 traços adicionados |
| Nenhuma ação comandada literalmente | ✅ tudo por utilidade |

---

## M05 — Comportamento Coletivo Emergente

| Checklist M05 | Estado |
|---|---|
| Relações esparsas (afinidade, dominância, dívida) | ✅ Social.Rels |
| Grupos por union-find sobre grafo de afinidade | ✅ Social.DetectGroups |
| Hierarquia por DomScore (atributos, não sorteio) | ✅ Social.DomScore |
| Altruísmo: divide energia com custo/benefício | ✅ Social.UpdatePair |
| Figuras por prestígio + liderança | ✅ Social.UpdateFigures |
| Papéis emergentes (Líder/Parental/Explorador/Forrageiro) | ✅ Creature.Role() |

---

## M06 — Ambiente

| Checklist M06 | Estado |
|---|---|
| Grade SoA: altitude/temp/umid/comida/água/material/tensão/balanço/bioma | ✅ Environment arrays |
| Bioma derivado (alt+temp+umid → enum) nunca pintado à mão | ✅ Environment.Derive |
| Estações (ciclo determinístico) | ✅ Season = sin(tick/Year) |
| Seca emergente por balanço hídrico (não sorteada) | ✅ BalancoAgua < DroughtThreshold |
| Tensão geológica → ruptura por limiar (vulcanismo) | ✅ TensaoGeo ≥ RuptureLimiar |
| Erosão (relevo muda devagar) | ✅ Environment.Erode |
| BFS de conectividade (isolamento → M03) | ✅ Environment.Connected |
| Dual-bioma (tempBias2/umidBias2) | ✅ Environment.Generate |

---

## M07 — Recursos e Economia

| Checklist M07 | Estado |
|---|---|
| Recursos finitos por bioma com teto | ✅ FoodCap/WaterCap por bioma |
| Regeneração ligada a clima/estação | ✅ foodCap × season |
| Harvest reduz, escassez é real | ✅ env.Harvest |
| Seca reduz regeneração a 30% | ✅ seca ? 0.3f : 1f |

---

## M08 — Comunicação e Linguagem

| Checklist M08 | Estado |
|---|---|
| Inventário fonêmico único por população | ✅ Language.Phonemes |
| Léxico procedural (forma gerada do inventário) | ✅ Language.GenForm |
| 5 estágios causais por pré-condição de estado | ✅ Language.AdvanceStage |
| Deriva fonológica proporcional ao isolamento | ✅ Language.DriftCount |
| Distância linguística (fonêmica + lexical) | ✅ Language.Distance |
| Determinismo: sort explícito em Dictionary.Keys | ✅ corrigido E09 |

---

## M09 — Cultura

| Checklist M09 | Estado |
|---|---|
| Memes (Valor/Tabu/Mito/Arte/Rito) com força e rigidez | ✅ Culture.Meme |
| Nascem de Figuras ou eventos, nunca espontâneos | ✅ SpawnCulturalMemes + DoCultureExchange |
| Propagação proporcional ao prestígio | ✅ Culture.Propagate |
| Lente interpretativa = argmax(força × prevalência) | ✅ Culture.Interpret |
| Coesão cultural (1 = unida, 0 = cisma) | ✅ Culture.CulturalCohesion |
| Cisma implícito quando coesão < limiar | ⚠️ medido mas não gera civ split automático (→ M11) |

---

## M10 — Religião

| Checklist M10 | Estado |
|---|---|
| 5 dimensões (fervor, dogmatismo, organização, imagem, estágio) | ✅ Belief fields |
| 4 estágios causais (Animismo→Politeísmo→Monoteísmo→Transcendente) | ✅ Belief.AdvanceStage |
| Imagem do jogador derivada do histórico de nudges | ✅ Belief.RecordNudge → DeriveImage |
| Fervor realimenta Atenção | ✅ InfluenceSystem.MaxAttention(fervor) |
| Poder do clero emerge de organização + recursos | ⚠️ organização cresce com Figuras; sem clero como entidade separada |

---

## M11 — Múltiplas Civilizações

| Checklist M11 | Estado |
|---|---|
| Civs com identidade (linhagem/idioma/cultura/território) | ✅ Civilization |
| Civs da IA correm mesmas regras M01–M10 | ✅ Population por civ |
| Contato por proximidade (ContactRadius = 5) | ✅ ContactSystem.FindContactPairs |
| Comércio: troca de energia, trust cresce | ✅ DoTrade |
| Conflito: força de combate por traços | ✅ DoConflict |
| Troca cultural: memes propagam entre civs | ✅ DoCultureExchange |
| Fusão gradual: criatura migra da menor para maior | ✅ DoFusionStep |
| Stance derivada do histórico (não sorteada) | ✅ UpdateStance |
| Distância linguística media contato (M08 integ.) | ✅ ContactAffinity usa Language.Distance |

**Tipo ausente**: vassalagem completa (só o enum existe; dinâmica de dominação → E10 backlog).

---

## M12 — Influência do Jogador

| Checklist M12 | Estado |
|---|---|
| Atenção regenera por tempo + fervor | ✅ InfluenceSystem.Step |
| 5 nudges com custo proporcional | ✅ Sinal/Faísca/Inspiração/Proteção/Pressão |
| Histórico de intervenções → imagem do jogador | ✅ Belief.RecordNudge |
| Alavanca 1: pressão ambiental (alter clima local) | ✅ nudge Pressão altera BaseTemp/BaseUmid |
| Alavanca 3: foco acumula por célula | ✅ InfluenceSystem.AddFocus / FocusMultiplier |
| Nenhuma alavanca comanda individuo diretamente | ✅ tudo por injeção de causa |
| Intervir contra tabu pode sair pela culatra | ⚠️ imagem Hostil registada; sem "backfire" automático em EventSystem |

---

## M13 — Observação e Apresentação

| Checklist M13 | Estado |
|---|---|
| Overlays (6 modos): Bioma/Comida/Água/Temperatura/Densidade/Grupos | ✅ CoreWorldView.VertexColor |
| Trocar overlay não altera simulação | ✅ função pura; E11 testa isso |
| Crônica narrativa com nomes do idioma emergente | ✅ Chronicle.PeopleNameOf |
| Crônica só narra eventos reais do log | ✅ Chronicle.Sync idempotente |
| Resumo por era para partidas longas | ✅ Chronicle.SummaryByEra |
| GIF/lapso de evolução (buffer de snapshots) | ❌ não implementado (requer Unity Timeline ou ffmpeg) |
| Fichas/códice de criaturas | ⚠️ HUD tem inspeção básica; sem painel dedicado |

**GIF/lapso**: requer captura de frames Unity → buffer → exportação ffmpeg/gifski. Implementável
em E10 futura iteração; nesta versão o foco foi nos overlays e Crônica (mais impactantes).

---

## M14 — Eventos Causais e Finais

| Checklist M14 | Estado |
|---|---|
| Situações latentes ativam por limiar de estado | ✅ EventSystem.Step |
| Resolução por argmax de compatibilidade estado/cultura | ✅ ResolveDrought etc. |
| Eventos encadeiam (efeitos de 2ª/3ª ordem) | ⚠️ Log rastreável; mas encadeamento automático não | 
| 7 destinos finais emergentes | ✅ EventSystem.EvaluateDestiny |
| Nenhum evento por probabilidade plana por turno | ✅ só por limiar |
| Log rastreável (base da Crônica) | ✅ EventSystem.Log → Chronicle |

---

## Resumo dos gaps restantes (candidatos à v1.0)

| Gap | Complexidade | Impacto |
|---|---|---|
| Plasticidade fenotípica (M02 §4.2) | Alta — novo campo em Creature | Médio |
| Vassalagem completa (M11) | Média — dinâmica de dominação | Baixo |
| Poder do clero como entidade (M10) | Alta — novo actor | Baixo |
| Backfire automático contra tabu (M12) | Média — EventSystem + Culture | Médio |
| GIF/timelapse exportável (M13) | Alta — Unity recording pipeline | Médio |
| Encadeamento automático de eventos 2ª ordem (M14) | Média — EventSystem refactor | Alto |

## Veredicto de conformidade

**Princípio de causalidade (GDD §1.5):** CONFORME em todos os subsistemas críticos.
Verificado por 61 testes, incluindo 6 de auditoria E11.

**Pilares GDD:** "influência, não controle" ✅ · emergência sem script ✅ · determinismo ✅ · 
estocasticidade causal ✅ · sem vitória forçada ✅ (7 destinos emergentes).
