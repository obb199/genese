# Gênese — Changelog

## v0.9.0-alpha (2026-06-04) — Implementação completa do núcleo (E00–E12)

### Etapas de engenharia

| Etapa | M-docs | Destaques |
|---|---|---|
| **E00** | — | Solução C#; separação Core/Game; determinismo; .gitignore |
| **E01** | — | RNG xoshiro256**+SplitMix64; sub-streams por Fork; WorldState; Snapshot/Restore |
| **E02** | M01 | GeneRegistry (24 genes); Genome; Reproduction (4 modos herança + mutação causal) |
| **E03** | M06/M07 | Environment SoA; bioma derivado; estações; seca por balanço hídrico; geologia→ruptura; erosão |
| **E04** | M04 | Population; IA de utilidade; softmax (temperatura=flexibilidade); Harvest consome recursos |
| **E05** | M05 | Social (relações esparsas); grupos por union-find; hierarquia por atributos; Figuras por prestígio |
| **E06** | M03 | Speciation; FatorTamanhoPop; PressaoMutagenica; PodeReproduzir; LOD |
| **E07** | M08/09/10 | Language (fonemas, léxico, deriva, 5 estágios); Culture (memes, lente interpretativa); Belief (fervor, imagem do jogador) |
| **E08** | M11/M14 | Civilization; ContactSystem (trade/war/cultura/fusão); EventSystem (7 tipos, argmax); Chronicle; destinos |
| **E09** | M02/M12/M13 | PopStats; InfluenceSystem (Atenção formal, 5 nudges, Alavanca 3); Chronicle; correção de 5 traços não usados |
| **E10** | — | SaveManager (3 slots); SettingsManager; overlays de terreno (6 modos); overlay selector funcional |
| **E11** | — | 61 testes; 6 testes QA (casos-limite + auditoria causalidade); chronicle idempotente |
| **E12** | — | GameVersion; CHANGELOG; revisão geral; build Release |

### Princípio de causalidade — conformidade

Toda probabilidade deriva do estado do mundo. Verificações (E11):
- Reprodução: delta=f(taxaMut×fatorReg×pressaoAmbiental×fatorPop)
- Mutação: sub-stream independente por Fork determinístico
- Decisão: softmax sobre utilidades derivadas de traços+ambiente
- Social: DomScore=f(agressividade+tamanho+prestígio+vitórias)
- Eventos: limiar de estado, nunca por chance por tick
- Crônica: só narra eventos do log, nenhum fabricado
- Overlays: função pura do estado, não alteram simulação

### Testes

```
61 testes xUnit — todos passam (dotnet test Genese.Core.Tests)
  CoreTests (7)       — determinismo, RNG, snapshot
  GenomeTests (6)     — herança, mutação, faixa
  EnvironmentTests (6)— bioma, seca, recursos, conectividade
  PopulationTests (5) — crescimento, extinção, barreiras
  SocialTests (5)     — grupos, hierarquia, Figuras
  SpeciationTests (5) — pressão, isolamento, distância
  SymbolicTests (6)   — língua, cultura, crença, determinismo
  CivTests (7)        — civs, contato, guerra, eventos, destinos
  InfluenceTests (6)  — PopStats, Atenção, foco, Crônica, destinos
  E11QATests (6)      — casos-limite, causalidade, conformidade overlay
```

### Arquitectura

```
Genese.sln
├─ Genese.Core/          — C# puro, netstandard2.1 (SEM Unity)
│   Rng · WorldState · ISimulation · Simulation (SnapshotVersion=8)
│   Genes · Genome · Reproduction               M01
│   Environment                                  M06/M07
│   Creature · Population                        M04
│   Social · Speciation                          M05/M03
│   Language · Culture · Belief                  M08/M09/M10
│   Civilization · CivRelation · ContactSystem   M11
│   EventSystem · Chronicle                      M13/M14
│   InfluenceSystem · PopStats                   M12/M02
├─ Genese.Core.Tests/    — xUnit, 61 testes
├─ Genese.Lab/           — console, 13 secções de inspeção
└─ unity/Genese/         — Unity 6 LTS (Genese.Game)
    Assets/Plugins/Genese.Core.dll
    Scripts/Nucleo/
      CoreBootstrap · CoreSim · CoreWorldView · CoreCreatureView
      CoreHud · CoreDayNight
      SaveManager · SettingsManager · GameVersion
      BuildingBuilder · FantasyPropBuilder · GenomeMap
```
