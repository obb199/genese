# Gênese

**Simulador evolutivo de civilização emergente — 3D, em Unity + C#.**

Você é uma presença invisível que **influencia, mas não controla** uma civilização de criaturas. Elas
evoluem em camadas paralelas (biologia, comportamento, linguagem, cultura, religião, sociopolítica), e
toda intervenção sua é **interpretada** pela cultura — a mesma ação gera resultados diferentes. Não há
vitória, há **destinos**.

> **Dimensão/motor:** 3D em Unity/C#. Ver [DIRECAO_3D.md](DIRECAO_3D.md) (registro autoritativo).

---

## Por onde começar

| Quero… | Vá para |
|---|---|
| **Ver a arte 3D agora** (termômetro) | [demo3d/index.html](demo3d/index.html) — duplo-clique no navegador |
| Entender a demo 3D e suas personalizações | [demo3d/README.md](demo3d/README.md) |
| **Abrir o jogo no Unity** | [unity/Genese/](unity/Genese/) — ver [unity/Genese/README.md](unity/Genese/README.md) |
| Saber o **estado do desenvolvimento** + checklist de testes | [desenvolvimento_atual.md](desenvolvimento_atual.md) |
| Ver o **roadmap** (E00–E12) e o status | [ROADMAP.md](ROADMAP.md) |
| **Testar tudo** (núcleo + protótipo) e propor melhorias | [GUIA_DE_TESTES.md](GUIA_DE_TESTES.md) |
| Rodar os **testes do núcleo** (lógica, sem Unity) | `dotnet test Genese.Core.Tests/Genese.Core.Tests.csproj` |
| **Ver o núcleo rodando** (genoma/herança/mutação no terminal) | `dotnet run --project Genese.Lab -- 12345 300 0.4` |
| Entender a decisão **3D/Unity** e o que ficou superado | [DIRECAO_3D.md](DIRECAO_3D.md) |
| Ler o design do jogo | `Genese_GDD.docx` + `explicação mecânicas/` (M00–M14) |
| Ver a direção de arte 3D | `DV_Design_Visual_3D.docx` · `AA_Catalogo_Assets_3D.docx` · `E10_Render_UX_Loja_3D.docx` |
| Ver o roadmap | `plano_desenvolvimento/` (`E_Roadmap_3D.docx`, `out_E00…E12`) |

---

## Estrutura do projeto

```
genese/
├── README.md                      Este arquivo (mapa do projeto)
├── ROADMAP.md                     Etapas E00–E12 + mecânicas M01–M14 e status
├── DIRECAO_3D.md                  Registro autoritativo: 3D + Unity; o que está superado
├── desenvolvimento_atual.md       Estado das etapas + checklist de testes (documento vivo)
│
├── Genese.sln                     Solução .NET (núcleo + testes)
├── Genese.Core/                   ★ Núcleo de simulação — C# PURO (sem Unity), determinístico
│   ├── E01 Rng·Simulation · E02 Genes·Genome · E03 Environment · E04 Creature·Population · E05 Social
├── Genese.Core.Tests/             Testes xUnit do núcleo (rodam sem a engine) — 29 testes
├── Genese.Lab/                    Laboratório de console: roda o núcleo visível (genoma/herança/deriva)
│
├── demo3d/                        ★ Termômetro de arte 3D (WebGL/Three.js) — alvo visual p/ Unity
│   ├── index.html                 Abrir no navegador (autossuficiente)
│   ├── README.md                  Doc da demo + tabela de personalização da criatura
│   ├── css/ · js/ · vendor/ · screenshots/
│
├── unity/Genese/                  ★ Projeto Unity (3D, C#) — porta do termômetro; Etapa 1 nasce aqui
│   ├── Assets/Genese/Scripts/     Palette, Genome, CreatureBuilder, WorldBuilder, Agent, SimManager…
│   ├── Assets/Genese/Editor/      Menu "Gênese ▸ Criar cena Mundo Vivo"
│   ├── Packages/ · ProjectSettings/  (Unity 6 LTS 6000.0.76f1)
│   └── README.md                  Como instalar o Editor, abrir e rodar
│
├── Genese_GDD.docx                Game Design Document (regras/visão — dimensão-agnóstico)
├── explicação mecânicas/          M00–M14: cada mecânica detalhada (independe de 2D/3D)
├── plano_desenvolvimento/         Roadmap 3D (E_Roadmap_3D) + saídas E00–E12
├── DV_Design_Visual_3D.docx       Direção de arte 3D
├── AA_Catalogo_Assets_3D.docx     Catálogo de assets 3D
├── E10_Render_UX_Loja_3D.docx     Render, UX e loja (3D)
├── imagens_de_referencia/         Referências visuais
└── todos_arquivos_claude_design/  Assets vetoriais do Claude Design — REFERÊNCIA (paleta, ícones,
                                    glifos, "forma-mãe"); não é a arte 3D final
```

---

## Estado atual

- **Protótipo 3D (Unity) concluído e reaproveitado** como camada de apresentação `Genese.Game`: mundo
  procedural rico (biomas, terreno com montanhas/rio/água, criaturas paramétricas, marcos, aldeias/cultura,
  nudges, dia/noite, colisões).
- **Núcleo `Genese.Core` (roadmap):** **E00–E05 concluídos, 29 testes xUnit verdes** — RNG/ticks/snapshot
  (E01), genoma herdável (E02), ambiente clima/recursos/geologia (E03), **agentes com IA de utilidade**
  (E04) e **comportamento coletivo emergente** (E05: relações, grupos por clustering, hierarquia, Figuras).
- **★ Integração na Unity:** o projeto **roda o núcleo de verdade** e o desenha — cena **Mundo (Núcleo)**
  (menu *Gênese ▸ Criar cena Mundo (Núcleo)*): terreno suave por bioma + criaturas reais, nudges, dia/noite,
  **HUD clicável** (célula → temperatura/umidade/recursos; criatura → genoma, **grupo/papel/prestígio/Figura**).
- **Próximo:** **E06 — Mutação & Especiação** + escala adaptativa (M03). Ver [ROADMAP.md](ROADMAP.md).
