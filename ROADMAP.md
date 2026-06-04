# Gênese — Roadmap de Construção

Fonte: `plano_desenvolvimento/E_Roadmap_3D.docx` + `out_E00…E12`. Jogo **3D em Unity/C#**.
Princípio arquitetural: **núcleo de simulação em C# puro** (determinístico, testável) + **Unity como
apresentação**. Princípio de causalidade (GDD §1.5): toda probabilidade deriva do estado do mundo.

Legenda: ✅ concluído · 🔜 próximo · ⬜ pendente

## Etapas de engenharia (E)

| Etapa | Tema | Status |
|---|---|---|
| **Protótipo** | Termômetro 3D / arte procedural (vira `Genese.Game`) | ✅ |
| **E00** | Fundação: solução, separação núcleo/engine, determinismo, CI | ✅ (CI ⬜) |
| **E01** | Núcleo: loop de ticks, RNG semeado + sub-streams, WorldState, snapshot/restore | ✅ (7/7 testes) |
| **E02** | **Genoma**: genes biológicos + comportamentais, 4 modos de herança, mutação causal (M01) | ✅ (6 testes) |
| **E03** | Ambiente: grade, clima/estações, recursos, seca, geologia→barreiras (M06/M07) | ✅ (6 testes) |
| **E04** | Agentes: decisão individual (IA de utilidade) + integração na Unity (M04) | ✅ (5 testes) |
| **E05** | Comportamento coletivo: relações, grupos por clustering, hierarquia, Figuras (M05) | ✅ (5 testes) |
| **E06** | Mutação & especiação: pressão causal, isolamento reprodutivo, LOD (M03) | ✅ (5 testes) |
| **E07** | Camada simbólica: comunicação/linguagem, cultura, religião (M08–M10) | ✅ (6 testes) |
| **E08** | Mundo & eventos: múltiplas civilizações, eventos ramificados (M11/M14) | ✅ (7 testes) |
| **E09** | Influência & apresentação: Atenção, nudges, observação (M12/M13) | ✅ (6 testes) |
| **E10** | Render 3D, UX e Loja (DV; Steam/Play) | ✅ (SaveManager, overlays, SettingsManager) |
| **E11** | QA & conformidade | ✅ (61 testes; auditoria causalidade) |
| **E12** | Lançamento | ✅ (GameVersion, CHANGELOG, REVISAO_GERAL) |

## Documentos de mecânica (M) — especificação de cada sistema

`explicação mecânicas/`: M00 Índice · M01 Genoma · M02 Traços · M03 Mutação/Especiação ·
M04 Decisão Individual · M05 Comportamento Coletivo · M06 Ambiente · M07 Recursos/Economia ·
M08 Comunicação/Linguagem · M09 Cultura · M10 Religião · M11 Múltiplas Civilizações ·
M12 Influência do Jogador · M13 Observação · M14 Eventos & Finais.

## Como o protótipo se encaixa

A arte procedural do protótipo (criaturas, biomas, terreno, aldeia, marcos, efeitos) **não se perde**:
ela é a camada **`Genese.Game`**. Conforme o `Genese.Core` ganha genoma (E02), ambiente (E03) e agentes
(E04), a Unity passa a **desenhar o estado do Core** com esses mesmos geradores, trocando os placeholders
visuais (movimento/“mutação” atuais) pela simulação real.

## Princípio de causalidade (bloqueador em toda etapa)
Nenhum número aleatório é consultado sem que sua distribuição derive do estado do mundo. Não há “evento
aleatório”, só “resultado incerto de causas conhecidas”. Todo subsistema recebe um sub-stream do RNG
central semeado (`Rng.Fork`).
