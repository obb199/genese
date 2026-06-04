# Gênese — Guia de Testes (tudo que dá para testar hoje)

Há **duas camadas** e cada uma se testa de um jeito. Use este guia para experimentar tudo e propor
mudanças. No fim há um **mapa de “onde mexer”** e um modelo para registrar sugestões.

---

## Parte A — Núcleo de simulação (lógica) · sem Unity

Precisa só do **.NET SDK 8** (já instalado). Tudo roda no terminal.

### A1. Testes automáticos (prova que a lógica está correta)
```bash
cd /home/user/Desktop/apps/genese
dotnet test Genese.Core.Tests/Genese.Core.Tests.csproj
```
Esperado: **13 passed**. O que eles garantem:
- **E01:** replay determinístico (mesma semente ⇒ mesmo resultado), snapshot/restore exato, sub-streams
  de RNG independentes, limites do RNG.
- **E02:** herança exata sem mutação, recessivo só expressa se ambos portam, reprodução determinística,
  genes ficam na faixa após 2000 gerações, frequência de mutação ≈ taxa, distância genética cresce com isolamento.

### A2. Laboratório interativo (VER o núcleo funcionando)
```bash
dotnet run --project Genese.Lab -- <semente> <gerações> <pressão>
# exemplos:
dotnet run --project Genese.Lab -- 12345 300 0.4
dotnet run --project Genese.Lab -- 777 1000 0.8     # mais gerações/pressão → mais deriva
```
Imprime 6 seções para você inspecionar e comparar entre execuções:
1. **Determinismo/RNG/Snapshot** — replay idêntico, round-trip, amostras de RNG.
2. **Genoma fundador** — todos os 24 genes (genético/comportamental/regulatório) em barras.
3. **Herança** — mãe × pai → filho, mostrando cada modo (média/dominante/recessivo/poligênico).
4. **Linhagem evoluindo** — como traços derivam ao longo das gerações + distância do fundador.
5. **Especiação (prévia)** — duas linhagens isoladas e a distância genética entre elas crescendo.
6. **População** — histograma de um traço (média e dispersão) — base das macro-estatísticas.

**O que testar / propor aqui:** taxas de mutação, faixas e modos de herança dos genes, força da pressão,
quão rápido as linhagens divergem. Tudo isso vive em:
- `Genese.Core/Genes.cs` — lista de genes, **taxa de mutação**, plasticidade, faixa, **modo de herança**.
- `Genese.Core/Reproduction.cs` — fórmulas de **herança** e **mutação causal** (passo, pressão, fator).
- `Genese.Lab/Program.cs` — o que o laboratório mostra (acrescente seções se quiser ver outra coisa).

---

## Parte B — Protótipo 3D (arte / “feel”) · no Unity

Precisa do **Unity 6** aberto no projeto `unity/Genese`.

### B1. Rodar
1. Unity Hub → **Open** → pasta `unity/Genese`.
2. Menu **Gênese ▸ Criar cena Mundo Vivo** → abre a cena.
3. **▶ Play**. (Se a câmera não girar: *Project Settings ▸ Player ▸ Active Input Handling* → **Both**.)

### B2. Roteiro de teste (marque e anote o que mudaria)
**Mundo / biomas** (botões `‹ bioma / bioma ›`)
- [ ] Cada bioma regenera **tudo** (chão, árvores, rochas, props, água, espécie).
- [ ] Achou **montanhas**? E o **rio** (aparece ~40% das vezes)?
- [ ] Achou o **marco** do bioma? (vulcão, pirâmide, iceberg, árvore gigante, arco, pico, salgueiro)

**Criaturas / espécie**
- [ ] As criaturas do mesmo bioma **parecem da mesma espécie**, mas **variam** entre si?
- [ ] O **saltinho** (squash & stretch) e as peças (antenas, orelhas) acompanham o corpo?
- [ ] **⟳ Nova geração** re-sorteia indivíduos da espécie.

**Aldeia / cultura** (botões `‹ cultura / cultura ›`)
- [ ] Casas variadas (1–2 andares, telhados diferentes, janelas, chaminé, bandeira)?
- [ ] O **monumento** muda por cultura (zigurate/torre/cristal/obelisco/antena/totens)?
- [ ] Fogueira + **pedra-coração**, totem e cores mudam por cultura?

**Interação**
- [ ] **Sinal / Faísca / Inspiração**: clicar no chão dispara efeitos suaves; Atenção cai e regenera.
- [ ] **Colisão**: criaturas **não atravessam** casas/árvores/rochas nem entram na **água**.
- [ ] **☀/☾ dia-noite** muda céu/luz; **câmera** gira, dá zoom e **inclina até quase o topo**.

**O que testar / propor aqui** — onde mexer:
- `unity/Genese/Assets/Genese/Scripts/CreatureBuilder.cs` — forma/peças das criaturas.
- `WorldBuilder.cs` — terreno, biomas, árvores, rochas, água, aldeia, marcos.
- `WorldData.cs` — paletas de bioma, perfis (densidade/props), culturas.
- `SimManager.cs` — nudges, dia/noite, HUD; `Fx.cs` — efeitos; `CameraOrbit.cs` — câmera.

---

## Parte C — Como propor mudanças

Para cada ideia, me mande algo no formato:

> **O quê:** “rios deveriam desaguar em lagos” / “mutação de cor rápida demais” / “casas medievais com muralha”
> **Onde (se souber):** bioma/cultura/gene/efeito — ou o arquivo do mapa acima
> **Por quê:** o efeito desejado no jogo

Eu aplico no lugar certo (núcleo **ou** apresentação) e, no núcleo, **adiciono/atualizo os testes**.

### Mapa rápido
| Quero mudar… | Camada | Arquivo(s) |
|---|---|---|
| Genes, taxas de mutação, herança | Núcleo | `Genese.Core/Genes.cs`, `Reproduction.cs` |
| Tempo, RNG, save/replay | Núcleo | `Genese.Core/Simulation.cs`, `Rng.cs` |
| O que o laboratório mostra | Núcleo | `Genese.Lab/Program.cs` |
| Aparência das criaturas | Arte | `unity/Genese/.../CreatureBuilder.cs` |
| Terreno/biomas/água/marcos/aldeia | Arte | `unity/Genese/.../WorldBuilder.cs`, `WorldData.cs` |
| Nudges/efeitos/câmera/HUD | Arte | `SimManager.cs`, `Fx.cs`, `CameraOrbit.cs` |

> Hoje as duas camadas ainda são **separadas** (o protótipo Unity usa movimento placeholder; o núcleo
> roda no laboratório). Elas se unem em **E04 (agentes)**, quando o Unity passa a desenhar o estado real
> do núcleo. Por isso teste **lógica** na Parte A e **arte/feel** na Parte B.
